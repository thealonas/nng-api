using System.Text.Json.Serialization;
using Redis.OM.Modeling;

namespace nng_api.Models;

[Document(StorageType = StorageType.Json, Prefixes = new[] {"watchdog"})]
public class WatchDog
{
    [RedisIdField]
    [JsonPropertyName("ulid")]
    [Indexed(PropertyName = "ulid")]
    public Ulid Ulid { get; set; }

    [JsonPropertyName("user_id")]
    [Indexed(PropertyName = "user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("name")]
    [Indexed(PropertyName = "name")]
    public string? Name { get; set; }

    [JsonPropertyName("group_id")]
    [Indexed(PropertyName = "group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("priority")]
    [Indexed(PropertyName = "priority")]
    public int Priority { get; set; }

    [JsonPropertyName("date")]
    [Indexed(PropertyName = "date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("reviewed")]
    [Indexed(PropertyName = "reviewed")]
    public bool Reviewed { get; set; }

    public bool Validate()
    {
        return UserId is null or > 0 && GroupId > 0 && Priority > 0 &&
               (Name == null || !string.IsNullOrWhiteSpace(Name));
    }
}
