using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using FTPer.Models;

namespace FTPer.Services;

/// <inheritdoc />
public sealed class FtpServerManager : IFtpServerManager
{
    private readonly ILogger<FtpServerManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private ServiceProvider? _ftpServiceProvider;
    private IFtpServerHost? _ftpServerHost;

    public FtpServerStatus Status { get; private set; } = FtpServerStatus.Stopped;
    public string? ErrorMessage { get; private set; }
    public FtpServerConfig? CurrentConfig { get; private set; }

    public event Action? OnStatusChanged;

    public FtpServerManager(ILogger<FtpServerManager> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public async Task StartAsync(FtpServerConfig config)
    {
        await _lock.WaitAsync();
        try
        {
            if (Status == FtpServerStatus.Running)
                throw new InvalidOperationException("FTP server is already running. Stop it first.");

            UpdateStatus(FtpServerStatus.Starting);
            EnsureDirectoryExists(config.RootPath);

            // Build a dedicated DI container for the FTP server so we can
            // configure it dynamically and dispose it cleanly on stop.
            var services = new ServiceCollection();

            // Share the host application's logging infrastructure
            services.AddSingleton(_loggerFactory);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // Configure the .NET file system provider with the chosen root folder
            services.Configure<DotNetFileSystemOptions>(opt =>
            {
                opt.RootPath = config.RootPath;
            });

            services.Configure<FtpAuthenticationOptions>(opt =>
            {
                opt.RequireAuthentication = config.RequireAuthentication;
                opt.Username = config.Username;
                opt.Password = config.Password;
            });

            services.AddFtpServer(builder =>
            {
                builder.UseDotNetFileSystem();
                builder.EnableAnonymousAuthentication();
            });

            services.AddSingleton<IMembershipProvider, ConfigurableMembershipProvider>();

            // Bind the FTP server to the selected local IP and port
            services.Configure<FtpServerOptions>(opt =>
            {
                opt.ServerAddress = config.IpAddress;
                opt.Port = config.Port;
            });

            _ftpServiceProvider = services.BuildServiceProvider();
            _ftpServerHost = _ftpServiceProvider.GetRequiredService<IFtpServerHost>();

            await _ftpServerHost.StartAsync(CancellationToken.None);

            CurrentConfig = config.Clone();
            UpdateStatus(FtpServerStatus.Running);

            _logger.LogInformation(
                "FTP server started — ftp://{IpAddress}:{Port} serving '{RootPath}'",
                config.IpAddress, config.Port, config.RootPath);
        }
        catch (Exception ex)
        {
            UpdateStatus(FtpServerStatus.Error, ex.Message);
            _logger.LogError(ex, "Failed to start FTP server");
            await CleanupAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (Status != FtpServerStatus.Running)
                return;

            UpdateStatus(FtpServerStatus.Stopping);
            await CleanupAsync();

            CurrentConfig = null;
            UpdateStatus(FtpServerStatus.Stopped);

            _logger.LogInformation("FTP server stopped");
        }
        catch (Exception ex)
        {
            UpdateStatus(FtpServerStatus.Error, ex.Message);
            _logger.LogError(ex, "Error stopping FTP server");
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gracefully shuts down and disposes the FTP server and its DI container.
    /// </summary>
    private async Task CleanupAsync()
    {
        if (_ftpServerHost is not null)
        {
            try
            {
                await _ftpServerHost.StopAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while stopping FTP server host");
            }
            _ftpServerHost = null;
        }

        if (_ftpServiceProvider is not null)
        {
            await _ftpServiceProvider.DisposeAsync();
            _ftpServiceProvider = null;
        }
    }

    private void UpdateStatus(FtpServerStatus status, string? errorMessage = null)
    {
        Status = status;
        ErrorMessage = errorMessage;
        OnStatusChanged?.Invoke();
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            _logger.LogInformation("Creating directory: {Path}", path);
            Directory.CreateDirectory(path);
        }
    }

    public void Dispose()
    {
        if (_ftpServerHost is not null)
        {
            try
            {
                _ftpServerHost.StopAsync(CancellationToken.None)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during FTP server disposal");
            }
            _ftpServerHost = null;
        }

        _ftpServiceProvider?.Dispose();
        _ftpServiceProvider = null;
        _lock.Dispose();
    }
}
