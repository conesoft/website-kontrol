﻿using Q42.HueApi;
using Q42.HueApi.Models.Groups;

namespace Conesoft_Website_Kontrol.Services
{
    public class LightControls
    {
        required public LocalHueClient? Client { get; init; }

        public static async Task<LightControls> ConnectToBridge(string appKey)
        {
            var bridges = await HueBridgeDiscovery.CompleteDiscoveryAsync(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));

            if (bridges.Count != 0)
            {
                return new() { Client = new LocalHueClient(bridges.First().IpAddress, appKey) };
            }
            return new() { Client = null };
        }

        public async Task<IReadOnlyCollection<Group>> GetRooms() => Client != null ? await Client.GetGroupsAsync() : [];

        public async Task<IEnumerable<Light>> GetLights() => Client != null ? await Client.GetLightsAsync() : [];
    }
}
