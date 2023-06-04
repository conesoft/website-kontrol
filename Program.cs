using Conesoft.Hosting;
using Conesoft.Users;
using Conesoft_Website_Kontrol;
using Conesoft_Website_Kontrol.Services;
using Conesoft_Website_Kontrol.Tools;
using Microsoft.Extensions.FileProviders;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder().AddJsonFile(Conesoft.Hosting.Host.GlobalSettings.Path).Build();

var phc = configuration.GetSection<PhilipsHueConfiguration>();
var nac = configuration.GetSection<NetAtmoConfiguration>();

builder.Services.AddSingleton<NetworkScanner>();
builder.Services.AddSingleton(await LightControls.ConnectToBridge(phc.AppKey));

await builder.Services.AddPeriodicWrapped(
    generator: async () => await ClimateSensors.Connect(nac.ClientId, nac.Secret, nac.Username, nac.Password),
    every: TimeSpan.FromHours(2)
);

builder.Services.AddUsers("Conesoft.Host.User", (Conesoft.Hosting.Host.GlobalStorage / "Users").Path);

builder.Services.AddTransient<User>();

builder.Services.AddHeadElementHelper();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

var app = builder.Build();

app.UseHeadElementServerPrerendering();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/content/feeds/thumbnail",
    FileProvider = new PhysicalFileProvider((Conesoft.Hosting.Host.GlobalStorage / "FromSources" / "Feeds" / "Entries").Path)
});

app.UseDeveloperExceptionPage();
app.UseHostingDefaults(useDefaultFiles: true, useStaticFiles: true);
app.UseStaticFiles();
app.UseRouting();
app.MapUsers();
app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.Run();




record PhilipsHueConfiguration(string AppKey) : TypedConfigurationHelper.ISection
{
    public static string SectionName => "philips-hue";
}

record NetAtmoConfiguration(string ClientId, string Secret, string Username, string Password) : TypedConfigurationHelper.ISection
{
    public static string SectionName => "netatmo";
}