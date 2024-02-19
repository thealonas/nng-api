using nng_api.DatabaseProviders;
using nng_api.Extensions;
using nng_api.Helpers;
using nng_api.Models;
using nng_api.Services;
using VkNet.Utils;

namespace nng_api.LoopTasks;

public class StatsUpdater : IntervalLoop
{
    private readonly GroupsDatabaseProvider _groups;
    private readonly GroupStatsDatabaseProvider _stats;
    private readonly VkService _vk;

    public StatsUpdater(ILogger<IntervalLoop> logger, StartupsDatabaseProvider startups, VkService vk,
        GroupStatsDatabaseProvider stats, GroupsDatabaseProvider groups) : base(TimeSpan.FromHours(2), logger, startups)
    {
        _vk = vk;
        _stats = stats;
        _groups = groups;
    }

    private IEnumerable<GroupStats> GetGroupsStatsByChunks(IEnumerable<long> groups)
    {
        return VkExecution.ExecuteWithReturn(() => _vk.Stats.Call("execute.getMembersInGroups",
            new VkParameters
            {
                {"groups", groups}
            }).ToCollectionOf(GroupStats.FromJson));
    }

    private IEnumerable<GroupStats> GetOptimisedGroupsStats(IReadOnlyCollection<long> groups)
    {
        var output = new List<GroupStats>();

        var groupChunks = groups.TakeBy(12);

        var counter = 0;

        foreach (var groupsChunk in groupChunks)
        {
            counter += groupsChunk.Count;

            Logger.LogInformation("Обрабатываю {Counter} из {Total} групп", counter, groups.Count);

            output.AddRange(GetGroupsStatsByChunks(groupsChunk));
        }

        return output;
    }

    private protected override void Action()
    {
        base.Action();

        Logger.LogInformation("Обновляю статистику");

        var groups = _groups.Collection.ToList().Select(x => x.GroupId).ToList();
        var stats = GetOptimisedGroupsStats(groups);

        Logger.LogInformation("Начинаю загружать статистику в базу данных");

        foreach (var stat in stats) _stats.Collection.Insert(stat);
    }
}
