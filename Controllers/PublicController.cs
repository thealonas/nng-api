using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using nng_api.DatabaseProviders;
using nng_api.Helpers;
using nng_api.Models;

namespace nng_api.Controllers;

[Route("")]
public class PublicController : BaseController
{
    private readonly UsersDatabaseProvider _provider;

    public PublicController(UsersDatabaseProvider provider, SettingsDatabaseProvider settings) : base(settings,
        provider)
    {
        _provider = provider;
    }

    [HttpGet("bnnd")]
    public IActionResult Banned()
    {
        var query = _provider.Collection.ToList().Where(x => x.BannedInfo != null).ToList();

        var output = query.Select(user => new BannedUserOutput(user.Name, user.UserId, user.Banned, user.BannedInfo))
            .ToList();

        return OutputResult.Ok(output);
    }

    [HttpGet("thx")]
    public IActionResult Thanks()
    {
        var query = _provider.Collection.ToList().Where(x => x.Thanks).ToList();

        var users = query.Select(x => new ThanksInfo(x.Name, x.UserId));

        return OutputResult.Ok(users);
    }
}

[Serializable]
public class BannedUserOutput
{
    public BannedUserOutput(string name, long userId, bool banned, BannedInfo? info)
    {
        Name = name;
        UserId = userId;
        Banned = banned;
        Info = info;
    }

    [JsonPropertyName("bnnd")] public bool Banned { get; set; }

    [JsonPropertyName("bnnd_info")] public BannedInfo? Info { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("user_id")] public long UserId { get; set; }
}

[Serializable]
public class ThanksInfo
{
    public ThanksInfo(string name, long userId)
    {
        Name = name;
        UserId = userId;
    }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("user_id")] public long UserId { get; set; }
}
