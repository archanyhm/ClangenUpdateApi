using System.Net;
using ClangenUpdateApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.Update;

[ApiController]
[Route("/Update/Channels/{channelName}/Releases/{releaseName}/Artifacts")]
public class ArtifactController : ReleaseControllerBase
{
    /// <summary>
    /// Gets a list of artifacts for the specified release in the specified channel.
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="releaseName"></param>
    [HttpGet]
    public IActionResult GetReleaseArtifacts(string channelName, string releaseName)
    {
        var channel = new Channel(channelName);

        if (ValidateChannel(channel) is var result && result != null) return result;
        
        if (!Directory.Exists($"{channel.ChannelDirectoryInfo.FullName}/Confirmed/{releaseName}"))
        {
            return BadRequest(new { success = false, messages = new[] { $"""No confirmed release with with name "{releaseName}" found.""" } });
        }

        return Ok(new { success = true, artifacts = new DirectoryInfo($"{channel.ChannelDirectoryInfo.FullName}/Confirmed/{releaseName}").EnumerateDirectories().Select(directory => directory.Name) });
    }
    
    /// <summary>
    /// Attaches a list of files to the specified artifact of the specified release in the specified channel.
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="releaseName"></param>
    /// <param name="artifactName"></param>
    /// <param name="fileBundle"></param>
    /// <returns></returns>
    [HttpPut("{artifactName}")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [ApiKey]
    public async Task<IActionResult> PutArtifact(string channelName, string releaseName, string artifactName, List<IFormFile> fileBundle)
    {
        var channel = new Channel(channelName);

        if (ValidateChannel(channel) is var result && result != null) return result;
        
        if (fileBundle.Count == 0)
        {
            return BadRequest(new { success = false, messages = new[] { "No files attached" } });
        }

        if (!fileBundle.Any(file => file.FileName.EndsWith(".sig")))
        {
            return BadRequest(new { success = false, messages = new[] { "No signature attached" } });
        }

        var releasePath = $"{channel.ChannelDirectoryInfo.FullName}/Appointed/{releaseName}";

        if (!Directory.Exists(releasePath))
        {
            releasePath = $"{channel.ChannelDirectoryInfo.FullName}/Confirmed/{releaseName}";
        }
        
        var targetPath = $"{releasePath}/{artifactName}";

        if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
        }

        Directory.CreateDirectory(targetPath);
        
        foreach (var formFile in fileBundle)
        {
            await using var stream = new FileStream($"{targetPath}/{formFile.FileName}", FileMode.Create);
            await formFile.CopyToAsync(stream);
        }

        return Created($"{channelName}/Releases/{releaseName}/Artifacts/{artifactName}", new { });
    }
    
    /// <summary>
    /// Gets the specified artifact for the specified release in the specified channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="releaseName"></param>
    /// <param name="artifactName"></param>
    [HttpGet("{artifactName}")]
    public async Task<IActionResult> GetReleaseArtifact(string channelName, string releaseName, string artifactName)
    {
        var channel = new Channel(channelName);
        if (ValidateChannel(channel) is var channelResult && channelResult != null) return channelResult;

        var release = new Release(releaseName, channel);
        if (ValidateRelease(release) is var releaseResult && releaseResult != null) return releaseResult;

        var artifactInfo = new DirectoryInfo($"{release.ReleaseDirectoryInfo.FullName}/{artifactName}");

        if (!artifactInfo.Exists)
        {
            return NotFound(new { success = false, messages = new[] { $"""No artifact with name "{artifactName}" found.""" } });
        }

        var artifactFile = artifactInfo.EnumerateFiles().First(file => file.Extension != "sig");
        var artifactSignature = artifactInfo.EnumerateFiles().First(file => file.Extension == ".sig");
        
        Response.Headers.Add("X-GPG-Signature", WebUtility.UrlEncode(await artifactSignature.OpenText().ReadToEndAsync()));

        return File(artifactFile.OpenRead(), "application/octet-stream", artifactFile.Name);
    }
    
    /// <summary>
    /// Gets a list of artifacts for the latest release in the specified channel.
    /// </summary>
    /// <param name="channelName"></param>
    // [HttpGet("{channelName}/Latest/Artifacts")]
    // public void GetLatestReleaseArtifacts(string channelName, string releaseName)
    // {
        
    // } 
    
    /// <summary>
    /// Gets the specified artifact for the latest release in the specified channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="artifactName"></param>
    [HttpGet("/Update/Channels/{channelName}/Releases/Latest/Artifacts/{artifactName}")]
    public async Task<IActionResult> GetLatestReleaseArtifact(string channelName, string artifactName)
    {
        var channel = new Channel(channelName);
        if (ValidateChannel(channel) is var channelResult && channelResult != null) return channelResult;

        var latestRelease = channel.GetLatestRelease();

        var artifactInfo = new DirectoryInfo($"{latestRelease.ReleaseDirectoryInfo.FullName}/{artifactName}");

        if (!artifactInfo.Exists)
        {
            return NotFound(new { success = false, messages = new[] { $"""No artifact with name "{artifactName}" found.""" } });
        }

        var artifactFile = artifactInfo.EnumerateFiles().First(file => file.Extension != "sig");
        var artifactSignature = artifactInfo.EnumerateFiles().First(file => file.Extension == ".sig");
        
        Response.Headers.Add("X-GPG-Signature", WebUtility.UrlEncode(await artifactSignature.OpenText().ReadToEndAsync()));

        return File(artifactFile.OpenRead(), "application/octet-stream", artifactFile.Name);
    } 
}
