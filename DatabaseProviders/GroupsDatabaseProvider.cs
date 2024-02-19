using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class GroupsDatabaseProvider : DatabaseProvider<GroupInfo>
{
    public GroupsDatabaseProvider(ILogger<DatabaseProvider<GroupInfo>> logger, RedisConnectionProvider provider)
        : base(logger, provider)
    {
    }
}
