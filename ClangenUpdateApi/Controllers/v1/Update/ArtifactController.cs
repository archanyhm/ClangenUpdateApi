using System.Net;
using ClangenUpdateApi.Authentication;
using ClangenUpdateApi.Database;
using ClangenUpdateApi.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Npgsql;
using System;

namespace ClangenUpdateApi.Controllers.v1.Update;

[ApiController]
[Route("/api/v{version:apiVersion}/Update/Channels/{channelName}/Releases/{releaseName}/Artifacts")]
public class ArtifactController : Controller
{
    public MainContext DbContext { get; set; }

    public ArtifactController(MainContext dbContext)
    {
        DbContext = dbContext;
    }
    
    /// <summary>
    /// Gets a list of artifacts for the specified release in the specified channel.
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="releaseName"></param>
    [HttpGet]
    public IActionResult GetReleaseArtifacts(string channelName, string releaseName)
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

        var artifacts = DbContext
            .Entry(release)
            .Collection(release => release.Artifacts)
            .Query()
            .ToList();

        return Ok(artifacts.Select(artifact => new
            {ArtifactName = artifact.Name,
                artifact.FileName,
                artifact.FileSize}));
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
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutArtifact(string channelName, string releaseName, string artifactName,
        List<IFormFile> fileBundle)
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
        
        var npgsqlConnection = (NpgsqlConnection) DbContext.Database.GetDbConnection();
        var manager = new NpgsqlLargeObjectManager(npgsqlConnection);

        try
        {
            await npgsqlConnection.OpenAsync();
            
            var oid = manager.Create();
            long fileSize = 0;
            var fileName = "";

            await using (var transaction = await npgsqlConnection.BeginTransactionAsync())
            {
                await using (var stream = await manager.OpenReadWriteAsync(oid))
                {
                    var releaseBundle = fileBundle.First(file => !file.FileName.EndsWith(".sig"));
                    fileName = releaseBundle.FileName;

                    await using (var fileReadStream = releaseBundle.OpenReadStream())
                    {
                        await fileReadStream.CopyToAsync(stream);
                        fileSize = releaseBundle.Length;
                    }
                }
            
                await transaction.CommitAsync();
            }

            var artifact = new Artifact
            {
                Release = release,
                Name = artifactName,
                FileName = fileName, 
                FileOid = oid,
                GpgSignature = null,
                FileSize = fileSize,
                DownloadCount = 0,
                HasValidSignature = true
            };

            DbContext.Artifacts.Add(artifact);
            await DbContext.SaveChangesAsync();

            return Created($"{channelName}/Releases/{releaseName}/Artifacts/{artifactName}", new { });
        }
        finally
        {
            await npgsqlConnection.CloseAsync();
        }
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
        Response.StatusCode = 200;
        
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
            return NotFound(new { messages = new[] { $"""No confirmed release with the name "{releaseName}" found.""" } });
        }

        var artifact = DbContext
            .Entry(release)
            .Collection(release => release.Artifacts)
            .Query()
            .FirstOrDefault(artifact => artifact.Name.ToLower() == artifactName.ToLower());

        if (artifact == null)
        {
            return NotFound(new { success = false, messages = new[] { $"""No artifact with name "{artifactName}" found.""" } });
        }
        
        var npgsqlConnection = (NpgsqlConnection) DbContext.Database.GetDbConnection();
        var manager = new NpgsqlLargeObjectManager(npgsqlConnection);

        try
        {
            await npgsqlConnection.OpenAsync();
            Response.Headers.Add(HeaderNames.ContentDisposition,
                new ContentDispositionHeaderValue("attachment") {FileName = artifact.FileName}.ToString());
            Response.Headers.Add(HeaderNames.ContentType, "application/octet-stream");
            Response.Headers.Add(HeaderNames.ContentLength, $"{artifact.FileSize}");

            await using var transaction = await npgsqlConnection.BeginTransactionAsync();
            
            await using (var stream = await manager.OpenReadAsync(artifact.FileOid))
            {
                await stream.CopyToAsync(Response.Body);
                await Response.Body.FlushAsync();
            }

            await transaction.CommitAsync();
        }
        finally
        {
            await npgsqlConnection.CloseAsync();
        }

        return Empty;
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
    [HttpGet("/api/v{version:apiVersion}/Update/Channels/{channelName}/Releases/Latest/Artifact/{artifactName}")]
    public async Task<IActionResult> GetLatestReleaseArtifact(string channelName, string artifactName)
    {
        Response.StatusCode = 200;
        
        var releaseChannel =
            DbContext.ReleaseChannels
                .Include(releaseChannel => releaseChannel.LatestRelease)
                .FirstOrDefault(releaseChannel => releaseChannel.Name.ToLower() == channelName.ToLower());

        if (releaseChannel == null)
        {
            return NotFound(new { success = false, messages = new[] { $"""No channel with name "{channelName}" found.""" } });        
        }

        var release = releaseChannel.LatestRelease;

        if (release == null)
        {
            return NotFound(new { messages = new[] { $"""No releases for "{releaseChannel.Name}" found.""" } });
        }

        var artifact = DbContext
            .Entry(release)
            .Collection(release => release.Artifacts)
            .Query()
            .FirstOrDefault(artifact => artifact.Name.ToLower() == artifactName.ToLower());

        if (artifact == null)
        {
            return NotFound(new { success = false, messages = new[] { $"""No artifact with name "{artifactName}" found.""" } });
        }
        
        var npgsqlConnection = (NpgsqlConnection) DbContext.Database.GetDbConnection();
        var manager = new NpgsqlLargeObjectManager(npgsqlConnection);

        try
        {
            await npgsqlConnection.OpenAsync();
            Response.Headers.Add(HeaderNames.ContentDisposition,
                new ContentDispositionHeaderValue("attachment") {FileName = artifact.FileName}.ToString());
            Response.Headers.Add(HeaderNames.ContentType, "application/octet-stream");
            Response.Headers.Add(HeaderNames.ContentLength, $"{artifact.FileSize}");

            await using var transaction = await npgsqlConnection.BeginTransactionAsync();
            
            await using (var stream = await manager.OpenReadAsync(artifact.FileOid))
            {
                await stream.CopyToAsync(Response.Body);
                await Response.Body.FlushAsync();
            }

            await transaction.CommitAsync();
        }
        finally
        {
            await npgsqlConnection.CloseAsync();
        }

        return Empty;
    }
}
