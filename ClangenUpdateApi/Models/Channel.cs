namespace ClangenUpdateApi.Models;

public class Channel
{
    public string ChannelName { get; }
    public DirectoryInfo ChannelDirectoryInfo { get; }
    
    public static string BasePath => Environment.GetEnvironmentVariable("STORAGE_PATH") ?? "./";
    
    public Channel(string channelName)
    {
        ChannelName = channelName;
        ChannelDirectoryInfo = new DirectoryInfo($"{BasePath}/{channelName}");
    }

    public Release? GetRelease(string releaseName)
    {
        return new Release(releaseName, this);
    }

    public Release? GetLatestRelease()
    {
        var latestDirectoryInfo = new DirectoryInfo($"{ChannelDirectoryInfo.FullName}/Confirmed/Latest");

        var linkTarget = latestDirectoryInfo.ResolveLinkTarget(true);

        return linkTarget != null ? new Release(linkTarget.Name, this) : null;
    }
    
    public bool Exists => ChannelDirectoryInfo.Exists;
}
