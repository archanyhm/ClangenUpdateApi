using ClangenUpdateApi.Authentication;
using ClangenUpdateApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.v1.Update;

[ApiController]
[Route("/api/v{version:apiVersion}/Update/Channels/{channelName}/Releases")]
[ApiVersion("1.0")]
public class ReleaseController : ReleaseControllerBase
{
    /// <summary>
    /// Get all of the releases for the specified update channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get(string channelName)
    {
        var channel = new Channel(channelName);

        if (ValidateChannel(channel) is var result && result != null) return result;

        var confirmedDir = channel.ChannelDirectoryInfo.GetDirectories().FirstOrDefault(directory => directory.Name == "Confirmed");

        if (confirmedDir == null)
        {
            return Ok(new { success = true, releases = Array.Empty<string>() });
        }

        return Ok(new { success = true, releases = confirmedDir.EnumerateDirectories().Where(directory => directory.Name != "Latest").Select(directory => directory.Name) });
    }
    
    /// <summary>
    /// Get the latest release for the specified update channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    [HttpGet("Latest")]
    public IActionResult GetLatest(string channelName)
    {
        var channel = new Channel(channelName);

        if (ValidateChannel(channel) is var result && result != null) return result;

        var latest = channel.GetLatestRelease();

        if (latest == null)
        {
            return NotFound(new { success = false });
        }

        return Ok( new { success = true, release = new { name = latest.ReleaseName } } );
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
        var channel = new Channel(channelName);

        if (ValidateChannel(channel) is var result && result != null) return result;

        var appointedPath = $"{channel.ChannelDirectoryInfo.FullName}/Appointed/{releaseName}";
        var confirmedPath = $"{channel.ChannelDirectoryInfo.FullName}/Confirmed/{releaseName}";

        if (Directory.Exists(appointedPath))
        {
            return Conflict(new { success = false, messages = new[] { $"""A release with the name "{releaseName}" has already been appointed.""" } });
        }
        
        if (Directory.Exists(confirmedPath))
        {
            return Conflict(new { success = false, messages = new[] { $"""A release with the name "{releaseName}" already exists.""" } });
        }
        
        Directory.CreateDirectory($"{channel.ChannelDirectoryInfo.FullName}/Appointed/{releaseName}");

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
        var channel = new Channel(channelName);

        if (ValidateChannel(channel) is var result && result != null) return result;

        var appointedPath = $"{channel.ChannelDirectoryInfo.FullName}/Appointed/{releaseName}";
        
        var confirmedDirectoryInfo = new DirectoryInfo($"{channel.ChannelDirectoryInfo.FullName}/Confirmed/{releaseName}");

        if (confirmedDirectoryInfo.Exists)
        {
            return BadRequest(new { success = false, messages = new[] { $"""Release "{releaseName}" is already confirmed.""" } });
        }

        if (!Directory.Exists(appointedPath))
        {
            return BadRequest(new { success = false, messages = new[] { $"""No appointed release with the name "{releaseName}" found.""" } });
        }

        Directory.CreateDirectory($"{channel.ChannelDirectoryInfo.FullName}/Confirmed");
        
        var dirInfo = new DirectoryInfo(appointedPath);
        dirInfo.MoveTo(confirmedDirectoryInfo.FullName);

        var latestPath = new DirectoryInfo($"{channel.ChannelDirectoryInfo.FullName}/Confirmed/Latest");

        if (latestPath.Exists)
        {
            latestPath.Delete(true);
        }
        
        latestPath.CreateAsSymbolicLink(confirmedDirectoryInfo.FullName);

        return Ok();
    }
}
