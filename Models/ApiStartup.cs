using System.Text.Json.Serialization;
using Redis.OM.Modeling;

namespace nng_api.Models;

[Document(StorageType = StorageType.Json, Prefixes = new[] {"info:startups:api"}, IndexName = "startups")]
public class ApiStartup
{
    [RedisIdField]
    [JsonPropertyName("operation")]
    [Indexed(PropertyName = "operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("finished")]
    [Indexed(PropertyName = "finished")]
    public DateTime FinishedDate { get; set; }
}
