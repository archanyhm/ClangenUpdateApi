namespace ClangenUpdateApi.Models;

public class Release
{
    public string ReleaseName { get; }
    public DirectoryInfo ReleaseDirectoryInfo { get; set; }
    public bool IsConfirmed { get; set; }
    private DirectoryInfo _appointed { get; }
    private DirectoryInfo _confirmed { get; }
        
    public Release(string releaseName, Channel channel)
    {
        ReleaseName = releaseName;

        _appointed = new DirectoryInfo($"{channel.ChannelDirectoryInfo.FullName}/Appointed/{releaseName}");
        _confirmed = new DirectoryInfo($"{channel.ChannelDirectoryInfo.FullName}/Confirmed/{releaseName}");

        if (_confirmed.Exists)
        {
            ReleaseDirectoryInfo = _confirmed;
            IsConfirmed = true;
        }
        else
        {
            ReleaseDirectoryInfo = _appointed;
            IsConfirmed = false;
        }
    }

    public void Appoint()
    {
        if (_confirmed.Exists) return;
        
        _appointed.Create();
        ReleaseDirectoryInfo = _appointed;
        IsConfirmed = false;
    }

    public void Confirm()
    {
        if (!_appointed.Exists) return;
        
        _appointed.MoveTo(_confirmed.FullName);
        ReleaseDirectoryInfo = _confirmed;
        IsConfirmed = true;
    }
    
    public bool Exists => ReleaseDirectoryInfo.Exists;
}
