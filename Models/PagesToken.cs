using System.Text.Json.Serialization;
using Redis.OM.Modeling;

namespace nng_api.Models;

[Document(StorageType = StorageType.Json, Prefixes = new[] {"settings:secrets:pages"},
    IndexName = "settings:secrets:pages")]
public class PagesToken
{
    [RedisIdField]
    [JsonPropertyName("user_id")]
    [Indexed(PropertyName = "user_id")]
    public long UserId { get; set; } = 0;

    [JsonPropertyName("token")]
    [Indexed(PropertyName = "token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("perms")]
    [Indexed(PropertyName = "perms")]
    public string Permissions { get; set; } = string.Empty;

    private IEnumerable<string> GetPermissions()
    {
        return Permissions.Split(",").Select(x => x.Trim().ToLower());
    }

    public bool HasPermission(string permission)
    {
        if (Permissions.ToLower().Equals("none")) return false;
        var perms = GetPermissions().ToList();
        return perms.Contains(permission) || perms.Contains("all");
    }
}
