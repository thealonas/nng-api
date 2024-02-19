using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class RequestsDatabaseProvider : DatabaseProvider<Request>
{
    public RequestsDatabaseProvider(ILogger<DatabaseProvider<Request>> logger, RedisConnectionProvider provider) :
        base(logger, provider)
    {
    }
}
