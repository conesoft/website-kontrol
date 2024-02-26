using Netatmo;
using Netatmo.Models.Client.Weather.StationsData.DashboardData;
using NodaTime;
using System.Text.Json;
using static Conesoft.Blazor.NetatmoAuth.NetatmoAuthorization;

namespace Conesoft_Website_Kontrol.Services;

class ClimateSensors
{
    Func<Client?> GetClient { get; set; } = () => null;

    string tokenpath = "";

    public async Task RefreshToken()
    {
        var oldtoken = JsonSerializer.Deserialize<AuthToken>(await File.ReadAllTextAsync(tokenpath));
        await GetClient()!.RefreshToken();
        var newtoken = GetClient()!.CredentialManager.CredentialToken;
        var token = new AuthToken(oldtoken!.Scope, newtoken.AccessToken, newtoken.ExpiresIn, newtoken.RefreshToken);
        await File.WriteAllTextAsync(tokenpath, JsonSerializer.Serialize(token));
    }

    public static async Task<ClimateSensors> Connect(string clientId, string secret, string tokenpath)
    {
        var token = JsonSerializer.Deserialize<AuthToken>(await File.ReadAllTextAsync(tokenpath));
        var ApiUrl = "https://api.netatmo.com";
        return new()
        {
            tokenpath = tokenpath,
            GetClient = () =>
            {
                var client = new Client(SystemClock.Instance, ApiUrl, clientId, secret);

                client.ProvideOAuth2Token(token!.AccessToken, token!.RefreshToken);
                return client;
            }
        };
    }

    public async IAsyncEnumerable<Readout> GetReadouts()
    {
        Netatmo.Models.Client.DataResponse<Netatmo.Models.Client.Weather.GetStationsDataBody> stations = default!;
        var client = GetClient()!;
        try
        {
            stations = await client.Weather.GetStationsData();
        }
        catch (Exception)
        {
            await RefreshToken();
            stations = await client.Weather.GetStationsData();
        }
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

public record Readout(string Name, double Temperature, int? CO2, int Humidity);