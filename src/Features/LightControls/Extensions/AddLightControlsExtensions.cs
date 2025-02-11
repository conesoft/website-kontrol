using Conesoft.Hosting;
using Conesoft_Website_Kontrol.Features.ClimateSensors.Services;
using Conesoft_Website_Kontrol.Features.LightControls.Services;
using Conesoft_Website_Kontrol.Tools.BackgroundInitialization;
using Q42.HueApi;

namespace Conesoft_Website_Kontrol.Features.LightControls.Extensions;

static class AddLightControlsExtensions
{
    public static WebApplicationBuilder AddLightControls(this WebApplicationBuilder builder)
    {
        builder.Services
            .ConfigureOptionsSection<LocalHueClientInitialization.Configuration>(LocalHueClientInitialization.Configuration.Section)
            .AddBackgroundInitialization<LocalHueClient, LocalHueClientInitialization>()
            ;

        return builder;
    }
}