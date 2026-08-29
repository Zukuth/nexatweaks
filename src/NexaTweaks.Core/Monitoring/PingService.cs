using System.Net.NetworkInformation;

namespace NexaTweaks.Core.Monitoring;

public sealed class PingService
{
    public async Task<long?> PingMsAsync(string host = "1.1.1.1", CancellationToken ct = default)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, 1500).WaitAsync(ct);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : null;
        }
        catch
        {
            return null;
        }
    }
}
