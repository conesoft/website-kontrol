using Conesoft.Blazor.NetatmoAuth;
using Conesoft.Hosting;
using Conesoft_Website_Kontrol.Features.ClimateSensors.Services;
using Conesoft_Website_Kontrol.Tools.BackgroundInitialization;

namespace Conesoft_Website_Kontrol.Features.ClimateSensors.Extensions;

static class AddClimateSensorsExtensions
{
    public static WebApplicationBuilder AddClimateSensors(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<Storage>()
            .AddNetatmoTokenStorageOnDisk<Storage>()
            .ConfigureOptionsSection<Options.NetatmoOptions>(Options.NetatmoOptions.Section)
            .AddBackgroundInitialization<Services.ClimateSensors, ClimateSensorsInitialization>()
            ;

        return builder;
    }
}