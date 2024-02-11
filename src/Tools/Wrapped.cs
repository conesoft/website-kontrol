namespace Conesoft_Website_Kontrol.Tools;

public class Wrapped<T>
{
    private T? current;
    private readonly Task<T> t;

    private Wrapped(Func<Task<T>> generator, TimeSpan every)
    {
        t = generator();
        Task.Run(async () =>
        {
            var timer = new PeriodicTimer(every);
            do
            {
                current = await (current == null ? t : generator());
            } while (await timer.WaitForNextTickAsync());
        });
    }

    public static async Task<Wrapped<T>> Wrap(Func<Task<T>> generator, TimeSpan every)
    {
        var wrapped = new Wrapped<T>(generator, every);
        await wrapped.Initialized;
        return wrapped;
    }

    public T Current => current!; // only accessed after awaiting initialization
    public Task Initialized => t;
}

public static class Wrapped
{
    public static async Task AddPeriodicWrapped<T>(this IServiceCollection services, Func<Task<T>> generator, TimeSpan every) where T : class
    {
        services
            .AddSingleton(await Wrapped<T>.Wrap(generator, every))
            .AddTransient(s => s.GetRequiredService<Wrapped<T>>().Current);
    }
}
