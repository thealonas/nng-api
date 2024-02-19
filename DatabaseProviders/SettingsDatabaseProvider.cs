using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class SettingsDatabaseProvider : DatabaseProvider<Settings>
{
    public SettingsDatabaseProvider(ILogger<DatabaseProvider<Settings>> logger, RedisConnectionProvider provider) :
        base(logger, provider)
    {
    }
}
