using FTPer.Models;

namespace FTPer.Services;

/// <summary>
/// Possible states of the FTP server.
/// </summary>
public enum FtpServerStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error
}

/// <summary>
/// Manages the lifecycle of the local FTP server (start, stop, status).
/// Registered as a singleton so the server state is shared across all UI connections.
/// </summary>
public interface IFtpServerManager : IDisposable
{
    /// <summary>Current status of the FTP server.</summary>
    FtpServerStatus Status { get; }

    /// <summary>Error message when <see cref="Status"/> is <see cref="FtpServerStatus.Error"/>.</summary>
    string? ErrorMessage { get; }

    /// <summary>The configuration the server was started with (null when stopped).</summary>
    FtpServerConfig? CurrentConfig { get; }

    /// <summary>Raised whenever the server status changes. Subscribers should call StateHasChanged.</summary>
    event Action? OnStatusChanged;

    /// <summary>Starts the FTP server with the given configuration.</summary>
    Task StartAsync(FtpServerConfig config);

    /// <summary>Stops the currently running FTP server.</summary>
    Task StopAsync();
}
