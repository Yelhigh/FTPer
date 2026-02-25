using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using FTPer.Models;

namespace FTPer.Services;

/// <inheritdoc />
public sealed class NetworkService : INetworkService
{
    private readonly ILogger<NetworkService> _logger;

    public NetworkService(ILogger<NetworkService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<NetworkInterfaceInfo> GetAvailableLocalAddresses()
    {
        var results = new List<NetworkInterfaceInfo>();

        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProperties = networkInterface.GetIPProperties();

                foreach (var unicastAddress in ipProperties.UnicastAddresses)
                {
                    if (unicastAddress.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (!IsPrivateIpAddress(unicastAddress.Address))
                        continue;

                    results.Add(new NetworkInterfaceInfo
                    {
                        InterfaceName = networkInterface.Name,
                        InterfaceType = networkInterface.NetworkInterfaceType.ToString(),
                        IpAddress = unicastAddress.Address.ToString(),
                        SubnetMask = unicastAddress.IPv4Mask?.ToString() ?? "255.255.255.0"
                    });

                    _logger.LogDebug(
                        "Found local address {Ip} on interface {Name} ({Type})",
                        unicastAddress.Address,
                        networkInterface.Name,
                        networkInterface.NetworkInterfaceType);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating network interfaces");
        }

        _logger.LogInformation("Discovered {Count} local network address(es)", results.Count);
        return results.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an IP address belongs to one of the RFC 1918 private ranges:
    /// 10.0.0.0/8, 172.16.0.0/12, or 192.168.0.0/16.
    /// </summary>
    private static bool IsPrivateIpAddress(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return bytes[0] switch
        {
            10 => true,
            172 => bytes[1] >= 16 && bytes[1] <= 31,
            192 => bytes[1] == 168,
            _ => false
        };
    }
}
