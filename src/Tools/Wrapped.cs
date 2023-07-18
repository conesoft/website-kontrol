using Conesoft_Website_Kontrol.Services;

namespace Conesoft_Website_Kontrol.Tools
{
    public static class Wrapped
    {
        public static async Task AddPeriodicWrapped<T>(this IServiceCollection services, Func<Task<T>> generator, TimeSpan every) where T : class
        {
            services
                .AddSingleton(await Wrapped<T>.Wrap(generator, every))
                .AddTransient(s => s.GetRequiredService<Wrapped<T>>().Current);
        }
    }
}
