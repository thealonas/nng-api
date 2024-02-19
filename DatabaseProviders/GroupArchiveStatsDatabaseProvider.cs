using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class GroupArchiveStatsDatabaseProvider : DatabaseProvider<GroupArchiveStats>
{
    public GroupArchiveStatsDatabaseProvider(ILogger<DatabaseProvider<GroupArchiveStats>> logger,
        RedisConnectionProvider provider) : base(logger, provider)
    {
    }
}
