namespace Conesoft_Website_Kontrol.Features.ClimateSensors.Options;

public class NetatmoOptions()
{
    public string ClientId { get; init; } = "";
    public string Secret { get; init; } = "";

    public static string Section => "netatmo";
}
