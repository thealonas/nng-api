namespace nng_api.Config;

public class Config
{
    public Config(string redisUrl)
    {
        RedisUrl = redisUrl;
    }

    public string RedisUrl { get; }
}
