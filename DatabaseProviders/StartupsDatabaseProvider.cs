using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class StartupsDatabaseProvider : DatabaseProvider<ApiStartup>
{
    public StartupsDatabaseProvider(ILogger<DatabaseProvider<ApiStartup>> logger, RedisConnectionProvider provider)
        : base(logger, provider)
    {
    }
}
