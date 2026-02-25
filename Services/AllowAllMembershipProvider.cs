using System.Security.Claims;
using FubarDev.FtpServer.AccountManagement;
using Microsoft.Extensions.Options;

namespace FTPer.Services;

/// <summary>
/// Options that configure credential validation for the FTP server.
/// </summary>
public sealed class FtpAuthenticationOptions
{
    public bool RequireAuthentication { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Membership provider that supports two modes:
/// - Anonymous: accepts any credentials (default)
/// - Authenticated: only accepts the configured username/password
/// </summary>
public sealed class ConfigurableMembershipProvider : IMembershipProvider
{
    private readonly FtpAuthenticationOptions _options;

    public ConfigurableMembershipProvider(IOptions<FtpAuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
        if (_options.RequireAuthentication)
        {
            var usernameMatch = string.Equals(username, _options.Username, StringComparison.OrdinalIgnoreCase);
            var passwordMatch = string.Equals(password, _options.Password, StringComparison.Ordinal);

            if (!usernameMatch || !passwordMatch)
            {
                return Task.FromResult(
                    new MemberValidationResult(MemberValidationStatus.InvalidLogin));
            }
        }

        var identity = new ClaimsIdentity(
            "custom",
            ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);

        identity.AddClaim(new Claim(
            ClaimsIdentity.DefaultNameClaimType,
            string.IsNullOrWhiteSpace(username) ? "anonymous" : username));

        identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, "user"));

        return Task.FromResult(
            new MemberValidationResult(
                MemberValidationStatus.AuthenticatedUser,
                new ClaimsPrincipal(identity)));
    }
}
