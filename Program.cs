using Conesoft.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
var app = builder.Build();

app.UseHostingDefaults(useDefaultFiles: true, useStaticFiles: true);
app.UseRouting();
app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.Run();
