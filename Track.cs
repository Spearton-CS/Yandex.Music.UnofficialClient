namespace Yandex.Music.UnofficialClient
{
    public sealed class Track : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            if (Parent?.TracksCache?.ContainsKey(this) == true)
                if (Parent.TracksCache[this] == 0)
                    Parent.TracksCache.Remove(this);
                else
                    return;
            Disposed = true;
            if (Albums is not null)
                foreach (var album in Albums)
                {
                    if (Parent?.AlbumsCache?.ContainsKey(album) == true)
                        Parent.AlbumsCache[album]--;
                    album?.Dispose();
                }
            Albums = null;
            if (Artists is not null)
                foreach (var artist in Artists)
                {
                    if (Parent?.ArtistsCache?.ContainsKey(artist) == true)
                        Parent.ArtistsCache[artist]--;
                    artist?.Dispose();
                }
            Artists = null;
            ID = Title = ImageURL = null;
            Parent = null;
            GC.SuppressFinalize(this);
        }
        public YMusicApi? Parent { get; private set; }
        public Track(YMusicApi? api, string id, string? title = null, uint? duration = null, bool? exp = null, string? img = null, (KeyValuePair<float, float>, KeyValuePair<float, float>)? fade = null, Artist[]? artists = null, Album[]? albums = null, KeyValuePair<string, string>? dlnks = null)
        {
            Parent = api;
            Full = title is not null && duration is not null && exp is not null && img is not null && fade is not null && artists is not null && albums is not null;
            ID = id;
            Title = title;
            Duration = duration;
            Explicit = exp;
            ImageURL = img;
            Fade = fade;
            Artists = artists;
            Albums = albums;
        }
        public bool Full { get; private set; }
        public async Task GetFullAsync()
        {
            if (Full)
                return;
            if (Parent is null)
                throw new InvalidOperationException("Parent is null");
        }
        public string ID { get; private set; }
        public string? Title { get; private set; }
        public uint? Duration { get; }
        public bool? Explicit { get; }
        public string? ImageURL { get; private set; }
        public (KeyValuePair<float, float> In, KeyValuePair<float, float> Out)? Fade { get; }
        public Artist[]? Artists { get; private set; }
        public Album[]? Albums { get; private set; }
        public KeyValuePair<string, string>? DirectLinks { get; private set; }
        public bool Downloaded => Disposed ? throw new ObjectDisposedException("Track") : File.Exists($"{Cache.DownloadedTracksCacheDir}\\{ID}.mp3");
        public async Task<Stream> GetStreamAsync(bool hq = true)
        {
            if (Parent is null)
                throw new InvalidOperationException("Parent is null");
            DirectLinks ??= await Parent.GetDirectLinksAsync(ID);
            return Disposed ? throw new ObjectDisposedException("Track") : await Temp.HttpClient.GetStreamAsync(hq ? DirectLinks.Value.Key : DirectLinks.Value.Value);
        }
        public async Task DownloadAsync(bool hq = true, string? path = null)
        {
            if (Disposed)
                throw new ObjectDisposedException("Track");
            using Stream ns = await GetStreamAsync(hq);
            path ??= $"{Cache.DownloadedTracksCacheDir}\\{ID}.mp3";
            using FileStream fs = File.Create(path);
            await ns.CopyToAsync(fs);
        }
        public async Task<Image> DownloadImageAsync(int width, int height, bool useECM = true, bool validate = false)
        {
            if (Disposed)
                throw new ObjectDisposedException("Track");
            Cache.Init();
            string path = $"{Cache.ImagesCacheDir}\\{ID}.track.{width}x{height}";
            FileStream fs = null;
            try
            {
                if (!File.Exists(path))
                    fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                else
                    fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Length == 0)
                    await Download();
                fs.Position = 0;
                return Image.FromStream(fs, useECM, validate);
                async Task Download()
                {
                    using Stream ns = await Temp.HttpClient.GetStreamAsync($"https://{ImageURL.Replace("%%", $"{width}x{height}")}");
                    await ns.CopyToAsync(fs);
                }
            }
            catch
            {
                fs?.Close();
                throw;
            }
        }
        public async Task Like()
        {
            if (Disposed)
                throw new ObjectDisposedException("Artist");
            List<string> id = new() { ID };
            try
            {
                await Parent.TrackApi.LikesTracks(id, Parent.UserID);
            }
            catch (InvalidOperationException)
            {
                await Parent.GetUserInfoAsync();
                await Parent.TrackApi.LikesTracks(id, Parent.UserID);
            }
        }
        public async Task Unlike()
        {
            if (Disposed)
                throw new ObjectDisposedException("Artist");
            List<string> id = new() { ID };
            try
            {
                await Parent.TrackApi.RemoveLikesTracks(id, Parent.UserID);
            }
            catch (InvalidOperationException)
            {
                await Parent.GetUserInfoAsync();
                await Parent.TrackApi.RemoveLikesTracks(id, Parent.UserID);
            }
        }
        public override string? ToString() => Title;
    }
}