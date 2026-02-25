namespace FTPer.Models;

/// <summary>
/// Represents a discovered local network interface with its IP address.
/// </summary>
public sealed class NetworkInterfaceInfo
{
    public required string InterfaceName { get; init; }
    public required string InterfaceType { get; init; }
    public required string IpAddress { get; init; }
    public required string SubnetMask { get; init; }

    public string DisplayName => $"{InterfaceName} — {IpAddress}";
}
