namespace Yandex.Music.UnofficialClient
{
    public sealed class Playlist : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            if (Parent?.PlaylistsCache?.ContainsKey(this) == true)
                if (--Parent.PlaylistsCache[this] == 0)
                    Parent.PlaylistsCache.Remove(this);
                else
                    return;
            Disposed = true;
            if (Creator is not null)
                Creator?.Dispose();
            Creator = null;
            if (Tracks is not null)
                foreach (var track in Tracks)
                {
                    if (Parent?.TracksCache?.ContainsKey(track) == true)
                        Parent.TracksCache[track]--;
                    track?.Dispose();
                }
            Tracks = null;
            Title = ImageURL = null;
            Parent = null;
            GC.SuppressFinalize(this);
        }
        public YMusicApi? Parent { get; private set; }
        public Playlist(YMusicApi? api, uint uid, uint kind, User? creator = null, string? title = null, uint? trackCount = null, bool? priv = null, DateTime? created = null, DateTime? modify = null, uint? duration = null, string? img = null, (string, string, string, string)? mosaic = null, Track[]? tracks = null)
        {
            Parent = api;
            UID = uid;
            Kind = kind;
            Full = creator is not null && title is not null && trackCount is not null && priv is not null && created is not null && modify is not null && duration is not null && img is not null && mosaic is not null && tracks is not null;
            Creator = creator;
            Title = title;
            TrackCount = trackCount;
            Private = priv;
            CreationDate = created;
            ModifyDate = modify;
            Duration = duration;
            ImageURL = img;
            MosaicImagesURL = mosaic;
            Tracks = tracks;
        }
        public bool Full { get; private set; }
        public async Task GetFullAsync()
        {
            if (Full)
                return;
            if (Parent is null)
                throw new InvalidOperationException("Parent is null");
            Full = true;
        }
        public uint UID { get; }
        public uint Kind { get; }
        public User? Creator { get; private set; }
        public string? Title { get; private set; }
        public uint? TrackCount { get; private set; }
        public bool? Private { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public DateTime? ModifyDate { get; private set; }
        public uint? Duration { get; private set; }
        public string? ImageURL { get; private set; }
        public (string First, string Second, string Third, string Fourth)? MosaicImagesURL { get; private set; }
        public Track[]? Tracks { get; private set; }
        public override string? ToString() => Title;
    }
}