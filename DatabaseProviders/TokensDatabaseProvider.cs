using nng_api.Models;
using Redis.OM;

namespace nng_api.DatabaseProviders;

public class TokensDatabaseProvider : DatabaseProvider<PagesToken>
{
    public TokensDatabaseProvider(ILogger<DatabaseProvider<PagesToken>> logger, RedisConnectionProvider provider) :
        base(logger, provider, "tokens")
    {
    }
}
