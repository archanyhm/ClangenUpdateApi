using Microsoft.AspNetCore.Authentication;

namespace ClangenUpdateApi.Authentication;

public static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        return AddApiKey<TAuthService>(builder, ApiKeyAuthenticationDefaults.AuthenticationScheme, _ => { });
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, string authenticationScheme)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        return AddApiKey<TAuthService>(builder, authenticationScheme, _ => { });
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, Action<ApiKeyAuthenticationOptions> configureOptions)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        return AddApiKey<TAuthService>(builder, ApiKeyAuthenticationDefaults.AuthenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, string authenticationScheme, Action<ApiKeyAuthenticationOptions> configureOptions)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        // builder.Services.AddSingleton<IPostConfigureOptions<ApiKeyAuthenticationOptions>, BasicAuthenticationPostConfigureOptions>();
        builder.Services.AddTransient<IApiKeyAuthenticationService, TAuthService>();

        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            authenticationScheme, configureOptions);
    }
}
