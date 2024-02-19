using nng_api.DatabaseProviders;
using nng_api.Models;

namespace nng_api.LoopTasks;

public abstract class IntervalLoop : LoopTask
{
    private readonly StartupsDatabaseProvider _startups;
    private protected readonly ILogger<IntervalLoop> Logger;

    private Timer? _timer;

    protected IntervalLoop(TimeSpan interval, ILogger<IntervalLoop> logger, StartupsDatabaseProvider startups)
        : base(interval)
    {
        Logger = logger;
        _startups = startups;
    }

    private protected sealed override Task Loop()
    {
        base.Loop();

        if (!TryGetStartup(out var startup))
        {
            var newStartup = new ApiStartup
            {
                Operation = GetType().Name,
                FinishedDate = DateTime.Now - Interval
            };

            _startups.Collection.Insert(newStartup);

            Logger.LogInformation("Процедура {Name} запущена впервые, запускаю",
                GetType().Name);

            _timer = new Timer(_ => Iterate(), null, TimeSpan.Zero, Interval);
            return Task.CompletedTask;
        }

        var timeToStart = DateTime.Now - startup.FinishedDate;

        if (timeToStart < Interval)
        {
            Logger.LogInformation("Процедура {Name} запущена ранее, запуск отложен на {Interval}",
                GetType().Name, Interval - timeToStart);
            _timer = new Timer(_ => Iterate(), null, Interval - timeToStart, Interval);
            return Task.CompletedTask;
        }

        Logger.LogInformation("Истёк интервал запуска процедуры {Name}, запускаю процедуру",
            GetType().Name);

        Iterate();
        _timer = new Timer(_ => Iterate(), null, Interval, Interval);

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return base.StopAsync(cancellationToken);
    }

    private bool TryGetStartup(out ApiStartup startup)
    {
        try
        {
            startup = _startups.Collection.ToList().First(x => x.Operation == GetType().Name);
            return true;
        }
        catch (Exception)
        {
            Logger.LogWarning("Не удалось получить список запусков");
            startup = new ApiStartup();
            return false;
        }
    }

    private void Iterate()
    {
        Logger.LogInformation("Запускаю процедуру {Name}", GetType().Name);
        Action();
        if (!TryGetStartup(out var startup))
            startup = new ApiStartup
            {
                Operation = GetType().Name
            };

        startup.FinishedDate = DateTime.Now;
        _startups.Collection.Insert(startup);

        Logger.LogInformation("Записываю время завершения процедуры {Name}: {Date} в формате unixtime",
            GetType().Name, ((DateTimeOffset) startup.FinishedDate).ToUnixTimeMilliseconds());

        Logger.LogInformation("Процедура {Name} завершена, запуск отложен на {Interval}",
            GetType().Name, Interval);
    }
}
