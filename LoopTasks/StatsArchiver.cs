using nng_api.DatabaseProviders;
using nng_api.Models;
using nng_api.Services;

namespace nng_api.LoopTasks;

public class StatsArchiver : ScheduledLoop
{
    private readonly VkService _api;
    private readonly GroupArchiveStatsDatabaseProvider _archive;
    private readonly ILogger<ScheduledLoop> _logger;
    private readonly GroupStatsDatabaseProvider _stats;
    private bool _isFirstRun;

    public StatsArchiver(ILogger<ScheduledLoop> logger, GroupStatsDatabaseProvider stats,
        GroupArchiveStatsDatabaseProvider archive, VkService api) : base(new TimeSpan(0, 0, 0), logger, true)
    {
        _logger = logger;
        _stats = stats;
        _archive = archive;
        _api = api;
        _isFirstRun = true;
    }

    private protected override void Action()
    {
        base.Action();

        _api.Update();

        var stats = _stats.Collection.ToList();

        if (!stats.Any())
        {
            _logger.LogInformation("Статистика отсутсвует, архивная статисткика не может быть обновлена");
            _isFirstRun = false;
            return;
        }

        if (_isFirstRun)
        {
            _isFirstRun = false;
            _logger.LogInformation("При первом запуске архивная статистика не будет обновлена");
            return;
        }

        _logger.LogInformation("Обновляю архивную статистику");

        var stat = new GroupArchiveStats
        {
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Stats = stats.Select(x => new GroupArchiveStat
            {
                GroupId = x.GroupId,
                Managers = x.Managers,
                Members = x.Members
            }).ToList()
        };

        _archive.Collection.Insert(stat);
    }
}
