using Microsoft.AspNetCore.Http.Features;

namespace nng_api.Extensions;

public static class HttpRequestExtensions
{
    public static string GetRawUrl(this HttpRequest request)
    {
        var requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
        return $"https://{request.Host}{GetRawTarget(requestFeature)}";
    }

    private static string GetRawTarget(IHttpRequestFeature? feature)
    {
        return feature?.RawTarget ?? throw new InvalidOperationException();
    }
}
