using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using nng_api.DatabaseProviders;
using nng_api.Extensions;
using nng_api.Helpers;
using nng_api.Models;
using nng_api.Services;

namespace nng_api.Controllers;

[Route("")]
public class UsersController : BaseController
{
    private readonly UsersDatabaseProvider _provider;
    private readonly VkService _vk;

    public UsersController(UsersDatabaseProvider provider, SettingsDatabaseProvider settings, VkService vk)
        : base(settings, provider)
    {
        _provider = provider;
        _vk = vk;
    }

    [HttpGet("[controller]:{userId:long}")]
    public IActionResult Get(long userId)
    {
        if (!TryAuthorize(out var id)) return OutputResult.NotAuthorized();

        if (TryAuthorizeAsAdmin() || id.Equals(userId))
            return !_provider.Collection.TryGetById(userId.ToString(), out var user)
                ? OutputResult.NotFound()
                : OutputResult.Ok(user);

        return OutputResult.InsufficientRights();
    }

    [HttpPut("[controller]")]
    public IActionResult Put([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] UserCreated? user)
    {
        if (!TryAuthorize(out var id)) return OutputResult.NotAuthorized();

        if (user is null || user.UserId.Equals(id))
        {
            if (_provider.Collection.TryGetById(id.ToString(), out _)) return OutputResult.Conflict();
            _provider.Collection.Insert(GetEmptyProfile(VkExecution.ExecuteWithReturn(() =>
            {
                var userGet = _vk.Users.Users.Get(new[] {id}).First();
                return $"{userGet.FirstName} {userGet.LastName}";
            }), id, true));
            return OutputResult.UserCreated();
        }

        if (!TryAuthorizeAsAdmin()) return OutputResult.InsufficientRights();

        if (!ModelState.IsValid || !user.Validate()) return OutputResult.InvalidInput();

        if (_provider.Collection.TryGetById(user.UserId.ToString(), out _)) return OutputResult.Conflict();

        _provider.Collection.Insert(GetEmptyProfile(user.Name, user.UserId, true));

        return OutputResult.UserCreated();
    }

    private static User GetEmptyProfile(string name, long id, bool app)
    {
        return new User
        {
            Name = name,
            UserId = id,
            Admin = false,
            Thanks = false,
            Banned = false,
            App = app,
            LastUpdated = DateTime.Now
        };
    }

    [HttpPost("[controller]:{userId:long}")]
    public async Task<IActionResult> Post([FromBody] UserUpdated user, long userId)
    {
        if (!TryAuthorize()) return OutputResult.NotAuthorized();
        if (!ModelState.IsValid || user.Banned is null || user.Thanks is null) return OutputResult.InvalidInput();

        if (!_provider.Collection.TryGetById(userId.ToString(), out var dbUser)) return OutputResult.NotFound();

        dbUser.Banned = (bool) user.Banned;
        dbUser.Thanks = (bool) user.Thanks;
        dbUser.LastUpdated = DateTime.Now;

        // TODO: юзать Update() когда редис омы пофиксят
        await _provider.Collection.InsertAsync(dbUser);
        return OutputResult.UserUpdated();
    }
}

public class UserCreated
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("user_id")] public long UserId { get; set; }

    public bool Validate()
    {
        return UserId > 0;
    }
}

public class UserUpdated
{
    [JsonPropertyName("thx")] public bool? Thanks { get; set; }
    [JsonPropertyName("bnnd")] public bool? Banned { get; set; }
}
