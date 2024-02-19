using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class GroupStatsDatabaseProvider : DatabaseProvider<GroupStats>
{
    public GroupStatsDatabaseProvider(ILogger<DatabaseProvider<GroupStats>> logger, RedisConnectionProvider provider) :
        base(
            logger, provider, "stats")
    {
    }
}
