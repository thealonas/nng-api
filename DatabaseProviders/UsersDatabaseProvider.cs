using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class UsersDatabaseProvider : DatabaseProvider<User>
{
    public UsersDatabaseProvider(ILogger<DatabaseProvider<User>> logger, RedisConnectionProvider provider) : base(
        logger, provider)
    {
    }
}
