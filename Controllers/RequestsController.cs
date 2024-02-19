using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using nng_api.DatabaseProviders;
using nng_api.Extensions;
using nng_api.Helpers;
using nng_api.Models;

namespace nng_api.Controllers;

[Route("")]
public class RequestsController : BaseController
{
    private readonly RequestsDatabaseProvider _provider;

    public RequestsController(RequestsDatabaseProvider provider, SettingsDatabaseProvider settings,
        UsersDatabaseProvider usersDatabaseProvider) : base(settings, usersDatabaseProvider)
    {
        _provider = provider;
    }

    public IActionResult Index()
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        return OutputResult.InvalidInput();
    }

    [HttpGet("[controller]")]
    public IActionResult Get()
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        var requests = _provider.Collection.ToList();

        return requests.Count == 0 ? OutputResult.NotFound() : OutputResult.Ok(requests);
    }

    [HttpPut("[controller]")]
    public IActionResult Put([FromBody] RequestCreate create)
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        if (!ModelState.IsValid || !create.Validate()) return OutputResult.InvalidInput();

        _provider.Collection.Insert(new Request
        {
            Date = DateTime.Now,
            Review = new ReviewObject
            {
                Decision = false,
                ReviewDate = DateTime.Now,
                Reviewed = false
            },
            UserId = create.UserId,
            Ulid = Ulid.NewUlid()
        });

        return OutputResult.RequestCreated();
    }

    [HttpPost("[controller]:{requestId}")]
    public IActionResult Post(string requestId, [FromBody] RequestUpdate update)
    {
        if (!TryAuthorizeAsAdmin()) return OutputResult.NotAuthorized();
        if (!ModelState.IsValid || !_provider.Collection.TryGetById(requestId, out var request) || !request.Validate())
            return OutputResult.InvalidInput();
        if (!update.Reviewed) return OutputResult.InvalidInput();

        var review = request.Review;
        review.Reviewed = true;
        review.Decision = update.Decision;
        review.ReviewDate = DateTime.Now;

        request.Review = review;

        _provider.Collection.Update(request);
        return OutputResult.RequestUpdated();
    }

    public class RequestCreate
    {
        [JsonPropertyName("user_id")] public long UserId { get; set; }

        public bool Validate()
        {
            return UserId > 0;
        }
    }

    public class RequestUpdate
    {
        [JsonPropertyName("reviewed")] public bool Reviewed { get; set; }
        [JsonPropertyName("decision")] public bool Decision { get; set; }
    }
}
