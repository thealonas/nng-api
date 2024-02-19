using Microsoft.AspNetCore.Mvc;
using nng_api.DatabaseProviders;
using nng_api.Helpers;
using nng_api.Models;

namespace nng_api.Controllers;

[Route("[controller]")]
public class StatsController : BaseController
{
    private readonly GroupStatsDatabaseProvider _provider;

    public StatsController(GroupStatsDatabaseProvider provider, SettingsDatabaseProvider settings,
        UsersDatabaseProvider usersDatabaseProvider) : base(settings, usersDatabaseProvider)
    {
        _provider = provider;
    }

    [HttpGet("groups:{groupId:long}")]
    public async Task<IActionResult> Get(long groupId)
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        GroupStats group;
        try
        {
            group = await _provider.Collection.FindByIdAsync(groupId.ToString()) ??
                    throw new InvalidOperationException();
        }
        catch (InvalidOperationException e)
        {
            return OutputResult.TeaPot(e);
        }

        return OutputResult.Ok(group);
    }
}
