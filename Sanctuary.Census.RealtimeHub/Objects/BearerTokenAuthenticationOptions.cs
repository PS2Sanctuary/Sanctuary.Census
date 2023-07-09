using Microsoft.AspNetCore.Authentication;

namespace Sanctuary.Census.RealtimeHub.Objects;

/// <summary>
/// Contains options for bearer token authentication.
/// </summary>
public class BearerTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets the configuration name.
    /// </summary>
    public const string OptionsName = "BearerTokenAuthenticationOptions";

    /// <summary>
    /// Gets the bearer token used for authentication.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenAuthenticationOptions"/> class.
    /// </summary>
    public BearerTokenAuthenticationOptions()
    {
        Token = string.Empty;
        ClaimsIssuer = "BearerTokenIssuer";
    }
}
