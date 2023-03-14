using ClangenUpdateApi.Authentication;
using ClangenUpdateApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.v1.Update;

[ApiController]
[Route("/api/v{version:apiVersion}/Update/Channels")]
[ApiVersion("1.0")]
public class UpdateController : UpdateControllerBase
{
    /// <summary>
    /// Gets a list of available update channels -- e.g. "stable" or "development"
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes=ApiKeyAuthenticationDefaults.AuthenticationScheme)]

    public IEnumerable<string> GetChannels()
    {
        var channels = Directory.GetDirectories(Channel.BasePath);
        var channelNames = channels.Select(name => name.Split("/").Last());

        return channelNames;
    }
}
