using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace BasicServer;

/// <summary>
/// Provides anonymous authentication for development and sample scenarios.
/// For production, use JWT Bearer authentication instead.
/// </summary>
public static class AuthenticationConfig
{
    public static void AddAnonymousAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("Anonymous")
            .AddScheme<AuthenticationSchemeOptions, AnonymousAuthenticationHandler>("Anonymous", options => { });
    }
}

public class AnonymousAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public AnonymousAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity("Anonymous");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Anonymous");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
