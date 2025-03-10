using Conesoft.Files;
using Conesoft.Tools;
using Netatmo;
using Netatmo.Models.Client.Weather.StationsData.DashboardData;
using NodaTime;
using Serilog;
using System.Text.Json;
using static Conesoft.Blazor.NetatmoAuth.NetatmoAuthorization;
using IO = System.IO;

namespace Conesoft_Website_Kontrol.Features.ClimateSensors.Services;

public class ClimateSensors(string clientId, string secret, string tokenpath) : IDisposable
{
    CancellationTokenSource? cancellationTokenSource;
    void IDisposable.Dispose() => cancellationTokenSource?.Cancel();

    Client? client;

    public async Task RefreshToken()
    {
        if (client is not null)
        {
            try
            {
                var oldtoken = JsonSerializer.Deserialize<AuthToken>(await IO.File.ReadAllTextAsync(tokenpath));
                await client.RefreshToken();
                var newtoken = client.CredentialManager.CredentialToken;
                var token = new AuthToken(oldtoken!.Scope, newtoken.AccessToken, newtoken.ExpiresIn, newtoken.RefreshToken);
                await IO.File.WriteAllTextAsync(tokenpath, JsonSerializer.Serialize(token));
            }
            catch (Exception)
            {
                //IO.File.Delete(tokenpath);
            }
        }
    }

    AuthToken? authToken;

    public void LiveConnect()
    {
        var tokenfile = Conesoft.Files.File.From(tokenpath);
        cancellationTokenSource = tokenfile.Live(async () =>
        {
            if (tokenfile.Exists)
            {
                var token = await tokenfile.ReadFromJson<AuthToken>();
                Safe.Try(() =>
                {
                    var apiUrl = "https://api.netatmo.com";
                    var client = new Client(SystemClock.Instance, apiUrl, clientId, secret);
                    client.ProvideOAuth2Token(token!.AccessToken, token!.RefreshToken);
                    this.client = client;
                    authToken = token;
                });
            }
            else
            {
                client = null;
            }
        });
    }

    public async IAsyncEnumerable<Readout> GetReadouts()
    {
        if (client == null)
        {
            yield break;
        }
        Netatmo.Models.Client.Weather.StationsData.Device[] stations = [];
        try
        {
            stations = (await client.Weather.GetStationsData()).Body.Devices;
        }
        catch (Exception)
        {
            await RefreshToken();
        }
        try
        {
            stations = (await client.Weather.GetStationsData()).Body.Devices;
        }
        catch (Exception ex)
        {
            Log.Error("netatmo/getstationsdata failed: {ex}", ex);
        }
        foreach (var d in stations)
        {
            yield return new(d.ModuleName, d.DashboardData.Temperature, d.DashboardData.CO2, d.DashboardData.HumidityPercent);
            foreach (var m in d.Modules)
            {
                var outdoor = Safe.Try(() => m.Type == "NAModule1" ? m.GetDashboardData<OutdoorDashBoardData>() : null);
                var indoor = Safe.Try(() => m.Type == "NAModule4" ? m.GetDashboardData<IndoorDashBoardData>() : null);

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