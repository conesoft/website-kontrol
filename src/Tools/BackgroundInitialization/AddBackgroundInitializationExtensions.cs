namespace Conesoft_Website_Kontrol.Tools.BackgroundInitialization;

static class AddBackgroundInitializationExtensions
{
    public static IServiceCollection AddBackgroundInitialization<Client, Initializer>(this IServiceCollection collection) where Initializer : BackgroundInitializationWhenAvailable<Client>
    {
        collection
            .AddSingleton<Initializer>()
            .AddSingleton<IWhenAvailable<Client>>(s => s.GetRequiredService<Initializer>())
            .AddHostedService(s => s.GetRequiredService<Initializer>())
            ;

        return collection;
    }
}