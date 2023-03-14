using Microsoft.AspNetCore.Authentication;

namespace ClangenUpdateApi.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; }
}
