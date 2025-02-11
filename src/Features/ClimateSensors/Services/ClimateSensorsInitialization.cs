using Conesoft_Website_Kontrol.Tools.BackgroundInitialization;
using Microsoft.Extensions.Options;
using Conesoft_Website_Kontrol.Features.ClimateSensors.Extensions;
using Conesoft.Blazor.NetatmoAuth.Services;

namespace Conesoft_Website_Kontrol.Features.ClimateSensors.Services;

public class ClimateSensorsInitialization(IOptions<Options.NetatmoOptions> options, Storage storage) : BackgroundInitializationWhenAvailable<ClimateSensors>
{
    public override async Task<ClimateSensors> Initialize(CancellationToken cancellationToken)
    {
        return await ClimateSensors.Connect(options.Value.ClientId, options.Value.Secret, storage.GeneratePath("token"));

    }
}