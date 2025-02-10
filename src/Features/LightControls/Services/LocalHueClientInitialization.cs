using Conesoft_Website_Kontrol.Tools.BackgroundInitialization;
using Microsoft.Extensions.Options;
using Q42.HueApi;

namespace Conesoft_Website_Kontrol.Features.LightControls.Services;

public class LocalHueClientInitialization(IOptions<LocalHueClientInitialization.Configuration> options) : BackgroundInitializationWhenAvailable<LocalHueClient>
{
    public class Configuration
    {
        public string AppKey { get; init; } = "";
        public static string Section => "philips-hue";
    };

    public override async Task<LocalHueClient> Initialize(CancellationToken cancellationToken)
    {
        var bridges = await HueBridgeDiscovery.CompleteDiscoveryAsync(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));
        if (bridges.Count != 0)
        {
            return new LocalHueClient(bridges.First().IpAddress, options.Value.AppKey);
        }
        throw new Exception("could not initialize hue bridge");
    }
}