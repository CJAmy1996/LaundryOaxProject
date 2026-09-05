using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace LaundryOaxWebAPI.IoT;

// Development authentication is deliberately explicit and limited to loopback callers.
public sealed class DemoAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, IWebHostEnvironment environment)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var remote = Context.Connection.RemoteIpAddress;
        if (!environment.IsDevelopment() || remote == null || !System.Net.IPAddress.IsLoopback(remote))
            return Task.FromResult(AuthenticateResult.Fail("Demo authentication is local development only."));
        var identity = new ClaimsIdentity(new[] { new Claim("sub", "local-operator"),
            new Claim("tenant_id", "oaxaca-central"), new Claim(ClaimTypes.Role, "Operator") }, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
public sealed class DeviceCredential
{
    public string TenantId { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public string KeySha256 { get; set; } = "";
}
public sealed class DeviceAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var key = Request.Headers["X-Device-Key"].ToString();
        if (string.IsNullOrEmpty(key)) return Task.FromResult(AuthenticateResult.NoResult());
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        foreach (var credential in configuration.GetSection("Devices").Get<DeviceCredential[]>() ?? [])
        {
            if (credential.KeySha256.Length != 64) continue;
            byte[] expected;
            try { expected = Convert.FromHexString(credential.KeySha256); } catch (FormatException) { continue; }
            if (!CryptographicOperations.FixedTimeEquals(hash, expected)) continue;
            var identity = new ClaimsIdentity(new[] { new Claim("tenant_id", credential.TenantId),
                new Claim("device_id", credential.DeviceId), new Claim(ClaimTypes.Role, "Device") }, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
        return Task.FromResult(AuthenticateResult.Fail("Invalid device credential."));
    }
}
