using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using nng_api.DatabaseProviders;
using nng_api.Extensions;
using nng_api.Helpers;
using nng_api.Models;

namespace nng_api.Controllers;

[Route("[controller]")]
public class SettingsController : BaseController
{
    private readonly SettingsDatabaseProvider _settings;
    private readonly UsersDatabaseProvider _users;

    public SettingsController(SettingsDatabaseProvider settings, UsersDatabaseProvider users) : base(settings, users)
    {
        _settings = settings;
        _users = users;
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        if (!_settings.Collection.TryGetById("main", out var data))
            return OutputResult.TeaPot("Invalid input");

        return OutputResult.Ok(data);
    }

    private bool IsAdmin(long user)
    {
        var exists = _users.Provider.Connection.Execute("EXISTS", $"users:{user}")
            .ToBoolean(CultureInfo.CurrentCulture);
        if (!exists) return false;

        // баганутый редис ом ломается на .ToBoolean()
        var isAdmin = bool.Parse(_users.Provider.Connection.Execute("JSON.GET", $"users:{user}", ".admin"));
        return isAdmin;
    }

    [HttpPost]
    public IActionResult Post([FromBody] Settings settings)
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        if (!ModelState.IsValid || !settings.Validate()) return OutputResult.InvalidInput();

        if (!IsAdmin(settings.LogUser)) return OutputResult.InvalidInput();

        if (!_settings.Provider.Connection.Execute("EXISTS", "settings:main")
                .ToBoolean(CultureInfo.CurrentCulture))
            return OutputResult.TeaPot("Invalid input");

        _settings.Provider.Connection.Execute("JSON.SET", "settings:main", ".",
            JsonSerializer.Serialize(settings));

        return OutputResult.SettingsUpdated();
    }
}
