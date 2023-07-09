using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sanctuary.Census.RealtimeHub.Objects;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Services;

/// <summary>
/// A really shitty static key bearer token authentication handler.
/// I don't even think you can call this a bearer...
/// </summary>
public class BearerTokenAuthenticationHandler : AuthenticationHandler<BearerTokenAuthenticationOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenAuthenticationHandler"/>.
    /// </summary>
    /// <param name="options">The authentication options to use.</param>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="encoder">The URL encoder to use.</param>
    /// <param name="clock">The clock to use.</param>
    public BearerTokenAuthenticationHandler
    (
        IOptionsMonitor<BearerTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    ) :  base(options, logger, encoder, clock)
    {
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? authHeader = Request.Headers.Authorization;
        if (authHeader?.StartsWith("bearer ", StringComparison.OrdinalIgnoreCase) is not true)
        {
            Response.StatusCode = 401;
            return Task.FromResult(AuthenticateResult.Fail("Invalid authorization header"));
        }

        string token = authHeader["Bearer ".Length..].Trim();
        if (token != OptionsMonitor.CurrentValue.Token)
        {
            Response.StatusCode = 401;
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        Claim[] claims =
        {
            new(ClaimTypes.Role, "collector", ClaimValueTypes.String, Options.ClaimsIssuer)
        };
        ClaimsIdentity identity = new(claims, Scheme.Name);

        return Task.FromResult
        (
            AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name))
        );
    }
}
