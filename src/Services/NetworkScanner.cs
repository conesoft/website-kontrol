using ArpLookup;
using Conesoft.Tools;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Conesoft_Website_Kontrol.Services;

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

    private readonly Dictionary<string, string> MacVendors = [];

    public async Task<AddressWithName[]> Scan()
    {
        var network = Network;
        var vendorcounter = 0;
        var results = await Task.WhenAll(Enumerable.Range(1, 255).Select(async i =>
        {
            Ping ping = new();
            var result = await ping.SendPingAsync(network + i, 1);
            if (result.Status == IPStatus.Success)
            {
                var mac = await Arp.LookupAsync(IPAddress.Parse(network + i));
                var vendor = "";
                if (mac != null)
                {
                    if (MacVendors.ContainsKey(mac.ToString()) == false && vendorcounter < 1)
                    {
                        HttpClient client = new();
                        Console.WriteLine($"https://api.macvendors.com/{mac}");
                        var cts = new CancellationTokenSource(1000);
                        vendor = await Safe.TryAsync(async () =>
                        {
                            try
                            {
                                return await client.GetStringAsync($"https://api.macvendors.com/{mac}", cts.Token);
                            }
                            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                            {
                                return "";
                            }
                        });
                        Console.WriteLine(vendor);
                        if (vendor != null)
                        {
                            MacVendors[mac.ToString()] = vendor;
                        }
                        vendorcounter++;
                    }
                    MacVendors.TryGetValue(mac.ToString(), out vendor);
                }

                return Safe.Try(() => Dns.GetHostEntry(network + i).HostName) switch
                {
                    string name => new
                    {
                        Success = true,
                        Name = name.Split('.').First(),
                        Address = network + i,
                        Mac = mac,
                        Vendor = vendor
                    },
                    null => new
                    {
                        Success = true,
                        Name = "",
                        Address = network + i,
                        Mac = mac,
                        Vendor = vendor
                    }
                };
            }
            return new
            {
                Success = false,
                Name = "",
                Address = network + i,
                Mac = default(PhysicalAddress),
                Vendor = default(string)
            };
        }));
        return results.Where(r => r.Success).Select(r => new AddressWithName(r.Name, r.Address, r.Mac, r.Vendor)).ToArray();
    }

    public record AddressWithName(string Name, string Address, PhysicalAddress? Mac, string? Vendor)
    {
        public string MacFormatted => Mac != null ? string.Join(":", Mac.GetAddressBytes().Select(b => b.ToString("X2"))) : "";
    };
}
