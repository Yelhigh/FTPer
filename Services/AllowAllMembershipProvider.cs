using System.Security.Claims;
using FubarDev.FtpServer.AccountManagement;

namespace FTPer.Services;

/// <summary>
/// Membership provider that accepts any username/password combination,
/// including empty credentials. This makes connecting from mobile FTP
/// clients frictionless — no need to remember to type "anonymous".
/// </summary>
public sealed class AllowAllMembershipProvider : IMembershipProvider
{
    public Task<MemberValidationResult> ValidateUserAsync(
        string username,
        string password)
    {
        var identity = new ClaimsIdentity("custom", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, string.IsNullOrWhiteSpace(username) ? "anonymous" : username));
        identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, "user"));

        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(new MemberValidationResult(MemberValidationStatus.AuthenticatedUser, principal));
    }
}
