using ClangenUpdateApi.Authentication;
using ClangenUpdateApi.Database;
using ClangenUpdateApi.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClangenUpdateApi.Controllers.v1.Update;

[ApiController]
[Route("/api/v{version:apiVersion}/Update/Channels/{channelName}/Releases")]
[ApiVersion("1.0")]
public class ReleaseController : Controller
{
    public MainContext DbContext { get; set; }

    public ReleaseController(MainContext dbContext)
    {
        DbContext = dbContext;
    }
    
    /// <summary>
    /// Get all of the releases for the specified update channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get(string channelName)
    {
        var releaseChannel =
            DbContext.ReleaseChannels
                .FirstOrDefault(releaseChannel => releaseChannel.Name.ToLower() == channelName.ToLower());
        
        if (releaseChannel == null)
        {
            return NotFound(new { success = false, messages = new[] { $"""No channel with name "{channelName}" found.""" } });        
        }

        var confirmedReleases = DbContext
            .Entry(releaseChannel)
            .Collection(channel => channel.Releases)
            .Query()
            .Where(release => release.IsConfirmed)
            .Select(release => release.VersionNumber);

        return Ok(new { releases = confirmedReleases});
    }
    
    /// <summary>
    /// Get the latest release for the specified update channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    [HttpGet("Latest")]
    public IActionResult GetLatest(string channelName)
    {
        var releaseChannel =
            DbContext.ReleaseChannels
                .Include(releaseChannel => releaseChannel.LatestRelease)
                .FirstOrDefault(releaseChannel => releaseChannel.Name.ToLower() == channelName.ToLower());

        if (releaseChannel == null)
        {
            return NotFound(new { success = false, messages = new[] { $"""No channel with name "{channelName}" found.""" } });        
        }


        if (releaseChannel.LatestRelease == null)
        {
            return NotFound();
        }

        return Ok(releaseChannel.LatestRelease.VersionNumber);
    }
    
    /// <summary>
    /// Appoints a release with the the specified release name in the specified channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="releaseName"></param>
    /// <returns></returns>
    [HttpPost("{releaseName}/Appoint")]
    [Authorize(AuthenticationSchemes=ApiKeyAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult AppointRelease(string channelName, string releaseName)
    {
        var releaseChannel =
            DbContext.ReleaseChannels
                .FirstOrDefault(releaseChannel => releaseChannel.Name.ToLower() == channelName.ToLower());

        if (releaseChannel == null)
        {
            return NotFound(new { success = false, messages = new[] { $"""No channel with name "{channelName}" found.""" } });        
        }

        var release = DbContext
            .Entry(releaseChannel)
            .Collection(channel => channel.Releases)
            .Query()
            .FirstOrDefault(release => release.VersionNumber.ToLower() == releaseName);

        if (release != null)
        {
            if (release.IsConfirmed)
            {
                return Conflict(new { success = false, messages = new[] { $"""A release with the name "{releaseName}" already exists.""" } });
            }
            
            return Conflict(new { success = false, messages = new[] { $"""A release with the name "{releaseName}" has already been appointed.""" } });
        }

        var newRelease = new Release { VersionNumber = releaseName, BuildRef = "Unknown", CreatedAt = DateTime.UtcNow, IsConfirmed = false, ReleaseChannels = new List<ReleaseChannel> { releaseChannel }};
        DbContext.Releases.Add(newRelease);
        DbContext.SaveChanges();
        
        return Created($"{channelName}/Releases/{releaseName}", new { });
    }

    /// <summary>
    /// Confirms a release -- making it available for downloads and discovery through the other endpoints
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="releaseName"></param>
    /// <returns></returns>
    [HttpPost("{releaseName}/Confirm")]
    [Authorize(AuthenticationSchemes=ApiKeyAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult ConfirmRelease(string channelName, string releaseName)
    {
        var releaseChannel =
            DbContext.ReleaseChannels
                .FirstOrDefault(releaseChannel => releaseChannel.Name.ToLower() == channelName.ToLower());

        if (releaseChannel == null)
        {
            return NotFound(new { success = false, messages = new[] { $"""No channel with name "{channelName}" found.""" } });        
        }
        
        var release = DbContext
            .Entry(releaseChannel)
            .Collection(channel => channel.Releases)
            .Query()
            .FirstOrDefault(release => release.VersionNumber.ToLower() == releaseName);

        if (release == null)
        {
            return NotFound(new { messages = new[] { $"""No appointed release with the name "{releaseName}" found.""" } });
        }

        if (release.IsConfirmed)
        {
            return Conflict(new { messages = new[] { $"""A release with the name "{releaseName}" is already confirmed.""" } });
        }

        release.IsConfirmed = true;

        DbContext
            .Entry(releaseChannel)
            .Reference(channel => channel.LatestRelease)
            .Load();

        if (releaseChannel.LatestRelease != null)
        {
            if (releaseChannel.LatestRelease.CreatedAt != null)
            {
                if (release.CreatedAt != null)
                {
                    var latestReleaseDate = (DateTime) releaseChannel.LatestRelease.CreatedAt;
                    var currentReleaseDate = (DateTime) release.CreatedAt;

                    Console.WriteLine($"{latestReleaseDate.ToShortTimeString()} <-> {currentReleaseDate.ToShortTimeString()}");
                    
                    if (latestReleaseDate.CompareTo(currentReleaseDate) < 0)
                    {
                        releaseChannel.LatestRelease = release;
                    }
                }
            }
            else
            {
                releaseChannel.LatestRelease = release;
            }
        }
        else
        {
            releaseChannel.LatestRelease = release;
        }
        
        DbContext.SaveChanges();

        return Ok();
    }
}
