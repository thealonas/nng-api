using System.Text.Json.Serialization;
using Redis.OM.Modeling;

namespace nng_api.Models;

[Document(StorageType = StorageType.Json, Prefixes = new[] {"request"}, IndexName = "request")]
public class Request
{
    [RedisIdField]
    [JsonPropertyName("ulid")]
    [Indexed(PropertyName = "ulid")]
    public Ulid Ulid { get; set; }

    [JsonPropertyName("user_id")]
    [Indexed(PropertyName = "user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("reviewed")]
    [Indexed(PropertyName = "reviewed")]
    public ReviewObject Review { get; set; } = null!;

    [JsonPropertyName("date")]
    [Indexed(PropertyName = "date")]
    public DateTime Date { get; set; }

    public bool Validate()
    {
        return UserId > 0 && Ulid != default;
    }
}

public class ReviewObject
{
    [JsonPropertyName("reviewed")]
    [Indexed(PropertyName = "reviewed")]
    public bool Reviewed { get; set; }

    [JsonPropertyName("decision")]
    [Indexed(PropertyName = "decision")]
    public bool Decision { get; set; }

    [JsonPropertyName("review_date")]
    [Indexed(PropertyName = "review_date")]
    public DateTime ReviewDate { get; set; }
}
