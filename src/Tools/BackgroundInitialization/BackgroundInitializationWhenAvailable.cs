namespace Conesoft_Website_Kontrol.Tools.BackgroundInitialization;

public abstract class BackgroundInitializationWhenAvailable<T> : IHostedService, IWhenAvailable<T>
{
    readonly WhenAvailableImplementation<T> whenAvailable = new();

    T? IWhenAvailable<T>.Instance => ((IWhenAvailable<T>)whenAvailable).Instance;
    void IWhenAvailable<T>.WhenAvailable<T1>(T1 reference, Action<T1> callback) where T1 : class => ((IWhenAvailable<T>)whenAvailable).WhenAvailable(reference, callback);

    public abstract Task<T> Initialize(CancellationToken cancellationToken);

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var _ = Task.Run(async () => whenAvailable.Set(await Initialize(cancellationToken)), cancellationToken);
        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}