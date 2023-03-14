using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ClangenUpdateApi.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string AuthorizationHeaderName = "X-API-Key";
    private const string SchemeName = "ApiKey";
    private readonly IApiKeyAuthenticationService _authenticationService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IApiKeyAuthenticationService authenticationService)
        : base(options, logger, encoder, clock)
    {
        _authenticationService = authenticationService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(AuthorizationHeaderName))
        {
            return AuthenticateResult.NoResult();
        }

        if(!AuthenticationHeaderValue.TryParse(Request.Headers[AuthorizationHeaderName], out AuthenticationHeaderValue headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = headerValue.Scheme;
        
        bool isValidKey = await _authenticationService.IsValidKey(apiKey);

        if (!isValidKey)
        {
            return AuthenticateResult.Fail("Invalid API key");
        }
        var claims = new[] { new Claim(ClaimTypes.Name, apiKey) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";
        await base.HandleChallengeAsync(properties);
    }
}
