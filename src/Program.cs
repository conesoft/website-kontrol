using Conesoft.Hosting;
using Conesoft.Users;
using Conesoft_Website_Kontrol.Components;
using Conesoft_Website_Kontrol.Services;
using Conesoft_Website_Kontrol.Tools;
using Microsoft.Extensions.FileProviders;

var configuration = new ConfigurationBuilder().AddJsonFile(Conesoft.Hosting.Host.GlobalSettings.Path).Build();

var nac = configuration.GetSection<NetAtmoConfiguration>();

var builder = WebApplication.CreateBuilder(args);

//await builder.Services.AddPeriodicWrapped(
//    generator: async () => await ClimateSensors.Connect(nac.ClientId, nac.Secret, nac.Username, nac.Password),
//    every: TimeSpan.FromHours(2)
//);

builder.Services
    .AddSingleton<NetworkScanner>()
    //.AddSingleton(await LightControls.ConnectToBridge(configuration.GetSection<PhilipsHueConfiguration>().AppKey))
    .AddHttpClient()
    .AddUsersWithStorage()
    .AddTransient<User>()
    .AddCascadingAuthenticationState()
    .AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

app
    .UseStaticFiles(new StaticFileOptions
    {
        RequestPath = "/content/feeds/thumbnail",
        FileProvider = new PhysicalFileProvider((Conesoft.Hosting.Host.GlobalStorage / "FromSources" / "Feeds" / "Entries").Path)
    })
    .UseDeveloperExceptionPage()
    .UseHostingDefaults(useDefaultFiles: true, useStaticFiles: true)
    .UseRouting() // fixes routes for Scoped CSS as well as static files
    .UseStaticFiles()
    .UseAntiforgery();

app.MapUsers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();




record PhilipsHueConfiguration(string AppKey) : TypedConfigurationHelper.ISection
{
    public static string SectionName => "philips-hue";
}

record NetAtmoConfiguration(string ClientId, string Secret, string Username, string Password) : TypedConfigurationHelper.ISection
{
    public static string SectionName => "netatmo";
}