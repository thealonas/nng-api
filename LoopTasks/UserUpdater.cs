using System.Globalization;
using nng_api.DatabaseProviders;
using nng_api.Helpers;
using nng_api.Models;
using nng_api.Services;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace nng_api.LoopTasks;

public class UserUpdater : IntervalLoop
{
    private readonly VkService _vk;

    private List<User> _cachedUsers;

    public UserUpdater(ILogger<UserUpdater> logger, UsersDatabaseProvider users, GroupsDatabaseProvider groups,
        StartupsDatabaseProvider startups, VkService vk) : base(TimeSpan.FromHours(2), logger, startups)
    {
        Users = users;
        Groups = groups;
        _vk = vk;
        Logger = logger;

        _cachedUsers = new List<User>();
    }

    private UsersDatabaseProvider Users { get; }
    private GroupsDatabaseProvider Groups { get; }
    private new ILogger<UserUpdater> Logger { get; }

    private Dictionary<long, List<long>> ManagerGroups { get; } = new();

    private protected override void Action()
    {
        _vk.Update();

        List<User> users;

        try
        {
            users = Users.Collection.ToList();
        }
        catch (NullReferenceException)
        {
            users = new List<User>();
        }

        _cachedUsers = users;
        Logger.LogInformation("Количество пользователей: {Count}", users.Count);

        var groups = Groups.Collection.ToList();
        Logger.LogInformation("Количество групп: {Count}", groups.Count);
        foreach (var group in groups)
        {
            Logger.LogInformation("Обрабатываю группу {Group}", group.GroupId);

            Logger.LogInformation("Получаю редакторов…");
            var managers = VkExecution.ExecuteWithReturn(() => _vk.Users.Groups.GetMembers(
                new GroupsGetMembersParams
                {
                    Count = 1000,
                    Fields = UsersFields.All,
                    Filter = GroupsMemberFilters.Managers,
                    GroupId = group.GroupId.ToString(),
                    Sort = GroupsSort.TimeAsc
                }));

            if (managers is null || !managers.Any())
            {
                Logger.LogInformation("Редакторы отсутсвуют");
                continue;
            }

            Logger.LogInformation("Количество редакторов: {Count}", managers.Count);
            foreach (var user in managers)
            {
                if (!_cachedUsers.Any(x => x.UserId.Equals(user.Id))) _cachedUsers.Add(GetUser(user.Id));

                if (ManagerGroups.ContainsKey(user.Id))
                {
                    ManagerGroups[user.Id].Add(group.GroupId);
                    continue;
                }

                ManagerGroups.Add(user.Id, new List<long> {group.GroupId});
            }
        }

        foreach (var user in _cachedUsers)
        {
            Logger.LogInformation("Обрабатываю пользователя {User}", user.UserId);

            UpdateUsersGroups(user);
            UpdateUsersTtl(user);

            Users.Collection.Insert(user);
        }

        ManagerGroups.Clear();

        Logger.LogInformation("Операция завершена");
    }

    private void UpdateUsersGroups(User user)
    {
        if (user.Banned)
        {
            Logger.LogInformation("Пользователь {User} забанен", user.UserId);

            if (user.Groups is not null)
            {
                Logger.LogInformation("Удаляю группы пользователя");
                user.LastUpdated = DateTime.Now;
                user.Groups = null;
            }

            if (user.Thanks)
            {
                Logger.LogInformation("Удаляю благодарности пользователя");
                user.LastUpdated = DateTime.Now;
                user.Thanks = false;
            }

            return;
        }

        if (!ManagerGroups.TryGetValue(user.UserId, out var list) && user.Groups is null)
        {
            Logger.LogInformation("У {User} не обнаружено никаких групп", user.UserId);
            return;
        }

        if (!ManagerGroups.TryGetValue(user.UserId, out list))
        {
            user.LastUpdated = DateTime.Now;
            user.Groups = null;
            Logger.LogInformation("Обнулили группы у {User}", user.UserId);
            return;
        }

        if (AreListsEqual(list, user.Groups))
        {
            Logger.LogInformation("У пользователя {User} не появилось новых групп", user.UserId);
            return;
        }

        user.Groups = list;
        user.LastUpdated = DateTime.Now;
        Logger.LogInformation("Обновлили группы у {User}", user.UserId);
    }

    private static bool AreListsEqual<T>(IEnumerable<T>? list1, IEnumerable<T>? list2)
    {
        if (list1 is null && list2 is null) return true;
        if (list1 is null || list2 is null) return false;

        var firstList = list1.ToList();
        var secondList = list2.ToList();

        return firstList.All(secondList.Contains) && secondList.All(firstList.Contains) &&
               firstList.Count.Equals(secondList.Count);
    }

    private void UpdateUsersTtl(User user)
    {
        if ((user.Groups is null || !user.Groups.Any()) && user.BannedInfo is null && user is
                {Banned: false, Admin: false, Thanks: false})
        {
            if (HasTtl(user.UserId))
            {
                Logger.LogInformation("Пользователь {User} уже имеет TTL", user.UserId);
                return;
            }

            user.LastUpdated = DateTime.Now;
            Logger.LogInformation("Пользователь {User} подходит под условия TTL", user.UserId);
            Users.Collection.Insert(user, TimeSpan.FromDays(30));
            return;
        }

        if (HasTtl(user.UserId))
        {
            Logger.LogInformation("Пользователь {User} не подходит под условия, однако имеет TTL",
                user.UserId);
            Logger.LogInformation("Удаляю TTL у пользователя {User}", user.UserId);
            Users.Provider.Connection.Execute("PERSIST", $"users:{user.UserId}");
        }

        Logger.LogInformation("Пользователь {User} не подходит под условия TTL", user.UserId);
    }

    private bool HasTtl(long userId)
    {
        return Users.Provider.Connection.Execute("TTL", $"users:{userId}").ToInt32(CultureInfo.CurrentCulture) > 0;
    }

    private User GetUser(long id)
    {
        Logger.LogInformation("Получаю пользователя {Id}", id);
        if (_cachedUsers.Any(x => x.UserId == id))
        {
            Logger.LogInformation("Пользователь уже есть в базе данных");
            return _cachedUsers.First(x => x.UserId == id);
        }

        Logger.LogInformation("Создаю профиль пользователя {Id}", id);

        return new User
        {
            Name = GetUserName(id),
            UserId = id,
            Thanks = false,
            App = false,
            Banned = false,
            LastUpdated = DateTime.Now
        };
    }

    private string GetUserName(long id)
    {
        var user = VkExecution.ExecuteWithReturn(() => _vk.Users.Users.Get(new[] {id}).First());
        return $"{user.FirstName} {user.LastName}";
    }
}
