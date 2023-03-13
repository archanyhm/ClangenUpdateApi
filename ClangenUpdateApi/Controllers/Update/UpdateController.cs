using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.Update;

[ApiController]
[Route("/Update/Channels")]
public class UpdateController : Controller
{
    public static readonly string BasePath = "./testdata";

    /// <summary>
    /// Gets a list of available update channels -- e.g. "stable" or "development"
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<string> GetChannels()
    {
        var channels = Directory.GetDirectories(BasePath);
        var channelNames = channels.Select(name => name.Split("/").Last());

        return channelNames;
    }
}
