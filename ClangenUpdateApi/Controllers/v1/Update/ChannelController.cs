using ClangenUpdateApi.Database;
using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.v1.Update;

[ApiController]
[Route("/api/v{version:apiVersion}/Update/Channels")]
[ApiVersion("1.0")]
public class UpdateController : Controller
{
    public MainContext DbContext { get; set; }

    public UpdateController(MainContext dbContext)
    {
        DbContext = dbContext;
    }
    
    /// <summary>
    /// Gets a list of available update channels -- e.g. "stable" or "development"
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetChannels()
    {
        return Ok(DbContext.ReleaseChannels.Select(releaseChannel => releaseChannel.Name));
    }
}
