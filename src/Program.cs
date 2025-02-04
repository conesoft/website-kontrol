//using Conesoft.Blazor.NetatmoAuth;
using Conesoft.Blazor.Components.Interfaces;
using Conesoft.Hosting;
using Conesoft.PwaGenerator;
using Conesoft.Users;
using Conesoft_Website_Kontrol.Components;
//using Conesoft_Website_Kontrol.Services;
using Conesoft_Website_Kontrol.Tools;
using Microsoft.Extensions.FileProviders;

//var configuration = new ConfigurationBuilder().AddJsonFile(Conesoft.Hosting.Host.GlobalSettings.Path).Build();

//var nac = configuration.GetSection<NetAtmoConfiguration>();

var builder = WebApplication.CreateBuilder(args);

builder
    .AddHostConfigurationFiles()
    .AddUsersWithStorage()
    .AddHostEnvironmentInfo()
    .AddLoggingService()
    ;

builder.Services
    .AddCompiledHashCacheBuster()
    .AddHttpClient()
    //.AddPeriodicGarbageCollection(TimeSpan.FromMinutes(5))
    //.AddSingleton(nac!)
    //.AddSingleton(await ClimateSensors.Connect(nac.ClientId, nac.Secret, @"D:\Hosting\Settings\Websites\Services\3rd Party Tokens\Netatmo - token.json"))
    //.AddNetatmoTokenStorageOnDisk(pathGenerator: name => $@"D:\Hosting\Settings\Websites\Services\3rd Party Tokens\Netatmo - {name}.json")
    //.AddSingleton<NetworkScanner>()
    //.AddSingleton(await LightControls.ConnectToBridge(configuration.GetSection<PhilipsHueConfiguration>().AppKey))
    .AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

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

//record PhilipsHueConfiguration(string AppKey) : TypedConfigurationHelper.ISection
//{
//    public static string SectionName => "philips-hue";
//}

//record NetAtmoConfiguration(string ClientId, string Secret, string Username, string Password) : TypedConfigurationHelper.ISection
//{
//    public static string SectionName => "netatmo";
//}

