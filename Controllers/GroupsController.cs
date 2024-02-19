using Microsoft.AspNetCore.Mvc;
using nng_api.DatabaseProviders;
using nng_api.Helpers;
using nng_api.Models;

namespace nng_api.Controllers;

[Route("[controller]")]
public class GroupsController : BaseController
{
    private readonly GroupsDatabaseProvider _provider;

    public GroupsController(GroupsDatabaseProvider provider, SettingsDatabaseProvider settings,
        UsersDatabaseProvider usersDatabaseProvider) : base(settings, usersDatabaseProvider)
    {
        _provider = provider;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var groups = await _provider.Collection.ToListAsync();
        return OutputResult.Ok(groups);
    }

    [HttpPut]
    public IActionResult Put([FromBody] GroupInfo group)
    {
        if (!TryAuthorize()) return OutputResult.NotAuthorized();
        if (!ModelState.IsValid || !group.Validate()) return OutputResult.InvalidInput();

        _provider.Collection.Insert(group);
        return OutputResult.GroupCreated();
    }
}
