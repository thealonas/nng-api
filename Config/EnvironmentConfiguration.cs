using nng_api.Helpers;

namespace nng_api.Config;

public class EnvironmentConfiguration
{
    private static EnvironmentConfiguration? _instance;

    private EnvironmentConfiguration()
    {
        Configuration = GetConfigFromEnv();
    }

    public Config Configuration { get; }

    public static EnvironmentConfiguration GetInstance()
    {
        return _instance ??= new EnvironmentConfiguration();
    }

    private static Config GetConfigFromEnv()
    {
        var dataUrl = EnvironmentHelper.GetString(Constants.Constants.RedisUrl);
        return new Config(dataUrl);
    }
}
