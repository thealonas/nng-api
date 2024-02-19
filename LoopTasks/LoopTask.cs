using Sentry;

namespace nng_api.LoopTasks;

public abstract class LoopTask : BackgroundService
{
    protected LoopTask(TimeSpan interval)
    {
        Interval = interval;
    }

    private protected TimeSpan Interval { get; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(Loop, stoppingToken).ContinueWith(t =>
        {
            if (t is {IsFaulted: true, Exception: { }})
                SentrySdk.CaptureException(t.Exception);
        }, stoppingToken);
        return Task.CompletedTask;
    }

    private protected virtual Task Loop()
    {
        return Task.CompletedTask;
    }

    private protected virtual void Action()
    {
    }
}
