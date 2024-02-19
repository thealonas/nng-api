using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using nng_api.DatabaseProviders;
using nng_api.Extensions;
using nng_api.Helpers;

namespace nng_api.Controllers;

[Route("")]
public class WatchDogController : BaseController
{
    private readonly WatchDogDatabaseProvider _watchdog;

    public WatchDogController(WatchDogDatabaseProvider watchdog, SettingsDatabaseProvider settings,
        UsersDatabaseProvider usersDatabaseProvider) : base(settings, usersDatabaseProvider)
    {
        _watchdog = watchdog;
    }

    [HttpGet("[controller]")]
    public IActionResult Get()
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        var logs = _watchdog.Collection.ToList();
        if (!logs.Any()) return OutputResult.NotFound();

        return OutputResult.Ok(logs);
    }

    [HttpPost("[controller]:{logId}")]
    public IActionResult Post(string logId, [FromBody] WatchDogUpdate log)
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        var logExists = _watchdog.Collection.TryGetById(logId, out var logEntry);
        if (!logExists || !ModelState.IsValid || !log.Validate() || !log.Reviewed) return OutputResult.InvalidInput();

        if (logEntry.Name is not null) logEntry.Name = log.Name;

        if (logEntry.UserId is not null) logEntry.UserId = log.UserId;

        logEntry.Reviewed = log.Reviewed;

        _watchdog.Collection.Update(logEntry);

        return OutputResult.WatchDogUpdated();
    }

    public class WatchDogUpdate
    {
        [JsonPropertyName("user_id")] public long UserId { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("reviewed")] public bool Reviewed { get; set; }

        public bool Validate()
        {
            return UserId > 0 && !string.IsNullOrWhiteSpace(Name);
        }
    }
}
