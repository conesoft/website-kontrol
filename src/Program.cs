using Conesoft.Hosting;
using Conesoft.PwaGenerator;
using Conesoft_Website_Kontrol.Components;
using Conesoft_Website_Kontrol.Features.ClimateSensors.Extensions;
using Conesoft_Website_Kontrol.Features.LightControls.Extensions;
using Conesoft_Website_Kontrol.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddHostConfigurationFiles()
    .AddUsersWithStorage()
    .AddHostEnvironmentInfo()
    .AddLoggingService()

    .AddClimateSensors()
    .AddLightControls()

    .AddCompiledHashCacheBuster()
    .AddHostingDefaults()
    ;

builder.Services
    .AddSingleton<NetworkScanner>()
    ;

var app = builder.Build();

app
    .UseCompiledHashCacheBuster()
    .UseStaticFiles(new StaticFileOptions
    {
        RequestPath = "/content/feeds/thumbnail",
        FileProvider = new PhysicalFileProvider((app.Services.GetRequiredService<HostEnvironment>().Global.Storage / "FromSources" / "Feeds" / "Entries").Path)
    })
    ;

app.MapPwaInformationFromAppSettings();
app.MapUsersWithStorage();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();