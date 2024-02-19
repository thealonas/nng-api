using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class WatchDogDatabaseProvider : DatabaseProvider<WatchDog>
{
    public WatchDogDatabaseProvider(ILogger<DatabaseProvider<WatchDog>> logger, RedisConnectionProvider provider) :
        base(
            logger, provider)
    {
    }
}
