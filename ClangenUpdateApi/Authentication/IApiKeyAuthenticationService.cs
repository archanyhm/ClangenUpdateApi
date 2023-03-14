namespace ClangenUpdateApi.Authentication;

public interface IApiKeyAuthenticationService
{
    Task<bool> IsValidKey(string apiKey);
}
