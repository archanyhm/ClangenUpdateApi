namespace ClangenUpdateApi.Authentication;

public class ApiKeyAuthenticationService : IApiKeyAuthenticationService
{
    public Task<bool> IsValidKey(string apiKey)
    {
        var apiKeys = Environment.GetEnvironmentVariable("API_KEYS")?.Split(' ') ?? Array.Empty<string>();
        return Task.FromResult(apiKeys.Contains(apiKey));
    }
}
