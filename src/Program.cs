using Conesoft.Hosting;
using Conesoft.PwaGenerator;
using Conesoft_Website_Kontrol.Components;
using Conesoft_Website_Kontrol.Features.LightControls.Extensions;
using Conesoft_Website_Kontrol.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddHostConfigurationFiles()
    .AddUsersWithStorage()
    .AddHostEnvironmentInfo()
    .AddLoggingService()

    .AddLightControls()
    ;

builder.Services
    .AddCompiledHashCacheBuster()
    .AddHttpClient()
    //.AddSingleton(await ClimateSensors.Connect("", "", ""))
    //.AddNetatmoTokenStorageOnDisk(pathGenerator: name => $@"{name}.json")
    .AddSingleton<NetworkScanner>()

    .AddRazorComponents().AddInteractiveServerComponents().AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(0);
        options.DisconnectedCircuitMaxRetained = 0;
    });

var app = builder.Build();

var section = app.Configuration.GetSection("philips-hue");

app
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