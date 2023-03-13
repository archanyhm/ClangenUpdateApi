using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.Update;

[ApiController]
[Route("/Update/Channels")]
public class UpdateController : Controller
{
    private static string _basePath = "/Users/philippmicke/Projects/Misc/ClangenUpdateApi/ClangenUpdateApi/testdata";

    /// <summary>
    /// Gets a list of available update channels -- e.g. "stable" or "development"
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<string> GetChannels()
    {
        var channels = Directory.GetDirectories(_basePath);
        var channelNames = channels.Select(name => name.Split("/").Last());

        return channelNames;
    }
}
