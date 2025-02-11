using Conesoft.Blazor.NetatmoAuth.Services;
using Conesoft.Files;
using Conesoft.Hosting;

namespace Conesoft_Website_Kontrol.Features.ClimateSensors.Extensions;

public class Storage(HostEnvironment environment) : DiskStorage
{
    public override string GeneratePath(string path) => (environment.Local.Storage / Filename.From(path, "json")).Path;
}