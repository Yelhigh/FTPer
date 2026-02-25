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
    /// When true, the server requires the configured <see cref="Username"/>
    /// and <see cref="Password"/>. When false, anonymous access is allowed.
    /// </summary>
    public bool RequireAuthentication { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public FtpServerConfig Clone() => new()
    {
        RootPath = RootPath,
        IpAddress = IpAddress,
        Port = Port,
        RequireAuthentication = RequireAuthentication,
        Username = Username,
        Password = Password
    };
}
