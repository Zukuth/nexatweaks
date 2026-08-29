using System.Net.NetworkInformation;

namespace NexaTweaks.Core.Monitoring;

public sealed record DnsProviderResult(string Provider, string Ip, long? RoundtripMs);

/// <summary>
/// Pings the well-known public DNS resolvers and ranks them by latency - the same idea behind
/// the "DNS Scanner" tool Optimizer PRO advertises, so the DNS tweak in the Network tab can be
/// backed by a real measurement instead of a hardcoded assumption.
/// </summary>
public static class DnsBenchmarkService
{
    private static readonly (string Provider, string Ip)[] Providers =
    {
        ("Cloudflare", "1.1.1.1"),
        ("Google", "8.8.8.8"),
        ("Quad9", "9.9.9.9"),
        ("OpenDNS", "208.67.222.222"),
        ("ISP / actual", GetCurrentDnsServer() ?? "0.0.0.0"),
    };

    public static async Task<List<DnsProviderResult>> RunAsync(CancellationToken ct = default)
    {
        var tasks = Providers
            .Where(p => p.Ip != "0.0.0.0")
            .Select(async p =>
            {
                long? best = null;
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        using var ping = new Ping();
                        var reply = await ping.SendPingAsync(p.Ip, 1200).WaitAsync(ct);
                        if (reply.Status == IPStatus.Success && (best is null || reply.RoundtripTime < best))
                            best = reply.RoundtripTime;
                    }
                    catch
                    {
                        // provider unreachable - leave as null
                    }
                }
                return new DnsProviderResult(p.Provider, p.Ip, best);
            });

        var results = await Task.WhenAll(tasks);
        return results.OrderBy(r => r.RoundtripMs ?? long.MaxValue).ToList();
    }

    private static string? GetCurrentDnsServer()
    {
        try
        {
            foreach (var nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                var dns = nic.GetIPProperties().DnsAddresses
                    .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (dns is not null) return dns.ToString();
            }
        }
        catch
        {
            // best effort
        }
        return null;
    }
}
