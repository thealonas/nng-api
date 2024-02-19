using Microsoft.AspNetCore.Mvc;
using nng_api.DatabaseProviders;
using nng_api.Extensions;
using nng_api.Helpers;

namespace nng_api.Controllers;

public class BaseController : ControllerBase
{
    private readonly string _secret;
    private readonly UsersDatabaseProvider _usersDatabaseProvider;

    public BaseController(SettingsDatabaseProvider settingsDatabaseProvider,
        UsersDatabaseProvider usersDatabaseProvider)
    {
        if (!settingsDatabaseProvider.Collection.TryGetById("main", out var settings))
            throw new InvalidOperationException();

        _secret = settings.AppSecret;
        _usersDatabaseProvider = usersDatabaseProvider;
    }

    [NonAction]
    private protected bool TryAuthorize()
    {
        return TryCheckSign();
    }

    [NonAction]
    private protected bool TryAuthorize(out long id)
    {
        return TryGetId(out id, out _) && TryCheckSign();
    }

    [NonAction]
    private protected bool TryAuthorizeAsAdmin()
    {
        return TryCheckSign() && TryGetId(out var id, out _) && GetProfile(id).Admin;
    }

    [NonAction]
    private bool TryCheckSign()
    {
        try
        {
            var url = Request.GetRawUrl();
            return SignCheckHelper.Check(url, _secret);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private RequestProfile GetProfile(long id)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!_usersDatabaseProvider.Collection.TryGetById(id.ToString(), out var user))
            return new RequestProfile(false, id);

        return new RequestProfile(user.Admin, id);
    }

    private bool TryGetId(out long id, out Exception? exception)
    {
        id = 0;
        exception = null;
        try
        {
            var url = Request.GetRawUrl();
            id = SignCheckHelper.GetId(new Uri(url));
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    private class RequestProfile
    {
        public RequestProfile(bool admin, long id)
        {
            Admin = admin;
            Id = id;
        }

        public long Id { get; }
        public bool Admin { get; }
    }
}
