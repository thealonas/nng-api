namespace nng_api.LoopTasks;

public abstract class ScheduledLoop : LoopTask
{
    private readonly ILogger<ScheduledLoop> _logger;

    private readonly bool _startImmediately;
    private Timer? _timer;

    protected ScheduledLoop(TimeSpan schedule, ILogger<ScheduledLoop> logger, bool startImmediately) : base(schedule)
    {
        _logger = logger;
        _startImmediately = startImmediately;
    }

    private protected sealed override Task Loop()
    {
        base.Loop();

        if (_startImmediately)
        {
            Task.Run(RunAction).ContinueWith(_ =>
            {
                _timer = new Timer(_ => RunAction(), null, CalculateTimeSpan(), TimeSpan.FromDays(1));
            });

            return Task.CompletedTask;
        }

        _timer = new Timer(_ => RunAction(), null, CalculateTimeSpan(), TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private TimeSpan CalculateTimeSpan()
    {
        var time = DateTime.Now.TimeOfDay;
        var span = Interval;

        var timeSpan = new TimeSpan(span.Hours - time.Hours, span.Minutes - time.Minutes, span.Seconds - time.Seconds);

        if (timeSpan < TimeSpan.Zero) timeSpan = new TimeSpan(24, 0, 0) + timeSpan;

        return timeSpan;
    }

    private void RunAction()
    {
        _logger.LogInformation("Запускаю процедуру {Name}", GetType().Name);
        Action();
        _logger.LogInformation("Процедура {Name} завершена, запуск отложен на {Interval}",
            GetType().Name, Interval);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
