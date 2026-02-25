namespace FTPer.Models;

/// <summary>
/// Represents the configuration for the local FTP server instance.
/// </summary>
public sealed class FtpServerConfig
{
    public string RootPath { get; set; } = @"F:\Memory\temp";
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 21;

    /// <summary>
    /// Creates a deep copy of this configuration.
    /// </summary>
    public FtpServerConfig Clone() => new()
    {
        RootPath = RootPath,
        IpAddress = IpAddress,
        Port = Port
    };
}
