using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Conesoft_Website_Kontrol.Services
{
    public class NetworkScanner
    {
        public string My => NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Where(n => n.GetIPProperties()?.GatewayAddresses.Any(g => g?.Address != null) ?? false)
            .SelectMany(adapter => adapter.GetIPProperties().UnicastAddresses)
            .Where(adr => adr.Address.AddressFamily == AddressFamily.InterNetwork && adr.IsDnsEligible)
            .Select(adr => adr.Address.ToString())
            .First();

        public string Network => $"{My[..My.LastIndexOf('.')]}.";

        public async Task<AddressWithName[]> Scan()
        {
            var network = Network;
            var results = await Task.WhenAll(Enumerable.Range(1, 255).Select(async i =>
            {
                Ping ping = new();
                var result = await ping.SendPingAsync(network + i, 1);
                if(result.Status == IPStatus.Success)
                {
                    var name = Dns.GetHostEntry(network + i).HostName;
                    if(name.Split('.').Length - 1 >= 2)
                    {
                        name = name[..name.IndexOf('.')];
                    }
                    return new
                    {
                        Success = true,
                        Name = name,
                        Address = network + i
                    };
                }
                else
                {
                    return new
                    {
                        Success = false,
                        Name = "",
                        Address = network + i
                    };
                }
            }));
            return results.Where(r => r.Success).Select(r => new AddressWithName(r.Name, r.Address)).ToArray();
        }

        public record AddressWithName(string Name, string Address);
    }
}
