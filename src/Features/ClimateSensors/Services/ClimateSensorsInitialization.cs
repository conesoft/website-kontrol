using Conesoft_Website_Kontrol.Features.ClimateSensors.Extensions;
using Conesoft_Website_Kontrol.Tools.BackgroundInitialization;
using Microsoft.Extensions.Options;

namespace Conesoft_Website_Kontrol.Features.ClimateSensors.Services;

public class ClimateSensorsInitialization(IOptions<Options.NetatmoOptions> options, Storage storage) : BackgroundInitializationWhenAvailable<ClimateSensors>
{
    public override Task<ClimateSensors> Initialize(CancellationToken cancellationToken)
    {
        var sensors = new ClimateSensors(options.Value.ClientId, options.Value.Secret, storage.GeneratePath("token"));
        sensors.LiveConnect();
        return Task.FromResult(sensors);

    }
}