using nng_api.DatabaseProviders;
using VkNet;
using VkNet.Model;

namespace nng_api.Services;

public class VkService
{
    private readonly ILogger<VkService> _logger;
    private readonly TokensDatabaseProvider _tokens;

    public VkService(TokensDatabaseProvider tokens, ILogger<VkService> logger)
    {
        _tokens = tokens;
        _logger = logger;
        Stats = new VkApi();
        Users = new VkApi();

        Update();
    }

    public VkApi Stats { get; }
    public VkApi Users { get; }

    public void Update()
    {
        try
        {
            var tokens = _tokens.Collection.ToList();
            var usersToken = tokens.First(x => x.HasPermission("users"));
            if (usersToken.UserId != Users.UserId)
            {
                _logger.LogInformation("Получен токен пользователей на пользователя с ID {0}", usersToken.UserId);
                Users.Authorize(new ApiAuthParams
                {
                    AccessToken = usersToken.Token
                });

                _logger.LogInformation("Авторизация в ВКонтакте с правами users прошла успешно");

                Users.UserId = usersToken.UserId;
            }

            var statsToken = tokens.First(x => x.HasPermission("stats"));
            if (statsToken.UserId != Stats.UserId)
            {
                _logger.LogInformation("Получен токен статистики на пользователя с ID {0}", statsToken.UserId);
                Stats.Authorize(new ApiAuthParams
                {
                    AccessToken = statsToken.Token
                });

                _logger.LogInformation("Авторизация в ВКонтакте с правами stats прошла успешно");

                Stats.UserId = statsToken.UserId;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка при обновлении токенов ВКонтакте");
        }
    }
}
