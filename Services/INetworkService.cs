using FTPer.Models;

namespace FTPer.Services;

/// <summary>
/// Discovers available local network interfaces and their private IP addresses.
/// </summary>
public interface INetworkService
{
    /// <summary>
    /// Returns all active network interfaces with private (RFC 1918) IPv4 addresses.
    /// These are the addresses that devices on the same WiFi network can reach.
    /// </summary>
    IReadOnlyList<NetworkInterfaceInfo> GetAvailableLocalAddresses();
}
