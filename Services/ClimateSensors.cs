using Netatmo;
using Netatmo.Models.Client.Weather.StationsData.DashboardData;
using NodaTime;

namespace Conesoft_Website_Kontrol.Services
{
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

    class ClimateSensors
    {
        required public Client? Client { get; init; }

        private ClimateSensors() { }

        public static async Task<ClimateSensors> Connect(string clientId, string secret, string username, string password)
        {
            Console.WriteLine("Connecting to NetAtmo...");
            var client = await NetatmoHelpers.Connect(clientId, secret, username, password);
            return new ClimateSensors { Client = client };
        }

        public IAsyncEnumerable<Readout> GetReadouts() => Client?.GetReadouts() ?? AsyncEnumerable.Empty<Readout>();
    }
}

public record Readout(string Name, double Temperature, int? CO2, int Humidity);

static class NetatmoHelpers
{
    public static async Task<Client?> Connect(string clientId, string secret, string username, string password)
    {
        try
        {
            var client = new Client(SystemClock.Instance, "https://api.netatmo.com/", clientId, secret);

            await client.GenerateToken(username, password, new[] { Scope.HomecoachRead, Scope.PresenceRead, Scope.StationRead, Scope.ThermostatRead });

            return client;
        }
        catch (Exception)    
        {
            return null;
        }
    }

    public static async IAsyncEnumerable<Readout> GetReadouts(this Client client)
    {
        var stations = await client.Weather.GetStationsData();
        foreach (var d in stations.Body.Devices)
        {
            yield return new(d.ModuleName, d.DashboardData.Temperature, d.DashboardData.CO2, d.DashboardData.HumidityPercent);
            foreach (var m in d.Modules)
            {
                var outdoor = m.Type == "NAModule1" ? m.GetDashboardData<OutdoorDashBoardData>() : null;
                var indoor = m.Type == "NAModule4" ? m.GetDashboardData<IndoorDashBoardData>() : null;

                if (outdoor != null || indoor != null)
                {
                    yield return new(
                        Name: m.ModuleName,
                        Temperature: outdoor?.Temperature ?? indoor!.Temperature,
                        CO2: indoor?.CO2 ?? null,
                        Humidity: outdoor?.HumidityPercent ?? indoor!.HumidityPercent
                    );
                }
                else
                {
                    yield return new(
                        Name: m.ModuleName,
                        Temperature: double.NegativeInfinity,
                        CO2: null,
                        Humidity: 0
                    );
                }
            }
        }
    }
}