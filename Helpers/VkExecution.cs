using VkNet.Exception;

namespace nng_api.Helpers;

public static class VkExecution
{
    public static TimeSpan WaitTime { get; set; }

    public static T ExecuteWithReturn<T>(Func<T> func)
    {
        return BaseExecuteWithReturn(func, true);
    }

    public static T ExecuteWithReturn<T>(Func<T> func, bool captchaWait)
    {
        return BaseExecuteWithReturn(func, captchaWait);
    }

    public static void Execute(Action action)
    {
        BaseExecute(action, true);
    }

    public static void Execute(Action action, bool captchaWait)
    {
        BaseExecute(action, captchaWait);
    }

    private static T BaseExecuteWithReturn<T>(Func<T> action, bool captchaWait)
    {
        if (action == null) throw new NullReferenceException(nameof(action));
        try
        {
            var obj = action.Invoke();
            return obj;
        }
        catch (TooManyRequestsException)
        {
            Task.Delay(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();
            Console.WriteLine("Жду 3 секунды…");
            return BaseExecuteWithReturn(action, captchaWait);
        }
        catch (CaptchaNeededException)
        {
            if (!captchaWait) throw;
            Console.WriteLine($"Каптча! Ожидаем {WaitTime}");
            Task.Delay(WaitTime).GetAwaiter().GetResult();
            return BaseExecuteWithReturn(action, captchaWait);
        }
    }

    private static void BaseExecute(Action action, bool captchaWait)
    {
        if (action == null) throw new NullReferenceException(nameof(action));
        try
        {
            action.Invoke();
        }
        catch (TooManyRequestsException)
        {
            Task.Delay(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();
            Console.WriteLine("Жду 3 секунды…");
            BaseExecute(action, captchaWait);
        }
        catch (CaptchaNeededException)
        {
            if (!captchaWait) throw;
            Console.WriteLine($"Каптча! Ожидаем {WaitTime}");
            Task.Delay(WaitTime).Wait();
            BaseExecute(action, captchaWait);
        }
    }
}
