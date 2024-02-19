using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace nng_api.Helpers;

public static class SignCheckHelper
{
    private const string EncodingType = "UTF-8";

    public static bool Check(string url, string secret)
    {
        var queryParams = GetQueryParams(new Uri(url));

        var checkString = string.Join("&", queryParams
            .Where(entry => entry.Key.StartsWith("vk_"))
            .OrderBy(entry => entry.Key)
            .Select(entry => Encode(entry.Key) + "=" + Encode(entry.Value)));

        var sign = GetHash(checkString, secret);
        var expectedSign = queryParams.GetValueOrDefault("sign", "");
        return sign == expectedSign;
    }

    public static Dictionary<string, string> GetQueryParams(Uri uri)
    {
        return uri.Query.TrimStart('?')
            .Split('&')
            .Select(pair => pair.Split('='))
            .ToDictionary(pair => Decode(pair[0]), pair => (pair.Length > 1 ? Decode(pair[1]) : null)
                                                           ?? string.Empty);
    }

    public static long GetId(Uri uri)
    {
        return long.Parse(GetQueryParams(uri)["vk_user_id"]);
    }

    private static string GetHash(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.GetEncoding(EncodingType).GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.GetEncoding(EncodingType).GetBytes(data));
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Decode(string value)
    {
        return WebUtility.UrlDecode(value);
    }

    private static string Encode(string value)
    {
        return WebUtility.UrlEncode(value);
    }
}
