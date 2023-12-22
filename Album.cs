namespace Yandex.Music.UnofficialClient
{
    public sealed class Album : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            if (Parent?.AlbumsCache.ContainsKey(this) == true)
                if (Parent.AlbumsCache[this] > 0)
                    return;
            Disposed = true;
            ID = Title = Genre = ImageURL = null;
            if (Parent?.Disposed == false)
                foreach (Artist artist in Artists)
                {
                    var founded = Parent.ArtistsCache.Where((x) => x.Key == artist);
                    if (!founded.Any())
                    {
                        artist?.Dispose();
                        continue;
                    }
                    Parent.ArtistsCache[artist]--;
                    if (Parent.ArtistsCache[artist] == 0)
                        artist.Dispose();
                }
            Artists = null;
            Parent = null;
            GC.SuppressFinalize(this);
        }
        /// <summary>Y.Music api instance, where cached this album's info</summary>
        public YMusicApi Parent { get; private set; }
        public Album(YMusicApi api, string id, string? title = null, string? genre = null, bool? single = null, uint? year = null, DateTime? release = null, bool? exp = null, uint? tracks = null, uint? likes = null, bool? recent = null, bool? vimp = null, string? img = null, Artist[]? artists = null)
        {
            Parent = api;
            Full = title is not null && genre is not null && single is not null && year is not null && release is not null && exp is not null && tracks is not null && likes is not null && recent is not null && vimp is not null && img is not null && artists is not null;
            ID = id;
            Title = title;
            Genre = genre;
            Single = single;
            Year = year;
            ReleaseDate = release;
            Explicit = exp;
            TrackCount = tracks;
            LikesCount = likes;
            Recent = recent;
            VeryImportant = vimp;
            ImageURL = img;
            Artists = artists;
        }
        public bool Full { get; private set; }
        public async Task GetFullAsync()
        {
            if (Full)
                return;
            var album = (await Parent.AlbumApi.InformAlbum(int.Parse(ID)))["result"];
            Title = album.Value<string>("title");
            Genre = album.Value<string>("genre");
            try
            {
                Single = album.Value<string>("type") == "single";
            }
            catch
            {
                Single ??= false;
            }
            Year = album.Value<uint>("year");
            ReleaseDate = album.Value<DateTime>("releaseDate");
            Explicit = album.Value<string>("contentWarning") == "explicit";
            TrackCount = album.Value<uint>("trackCount");
            LikesCount = album.Value<uint>("likesCount");
            Recent = album.Value<bool>("recent");
            VeryImportant = album.Value<bool>("veryImportant");
            ImageURL = album.Value<string>("coverUri");
            var artInfo = album["artists"];
            Artists = new Artist[artInfo.Count()];
            for (int i = 0; i < Artists.Length; i++)
            {
                var artist = artInfo[i];
                string artID = artist.Value<string>("id");
                var foundedArts = Parent.ArtistsCache.Where((x) => x.Key.ID == artID);
                if (foundedArts.Any())
                {
                    var inf = foundedArts.First();
                    Parent.ArtistsCache[inf.Key]++;
                    Artists[i] = inf.Key;
                    continue;
                }
                Artist artInf = await Parent.GetArtistAsync(artID);
                Artists[i] = artInf;
            }
        }
        /// <summary>Album's ID</summary>
        public string ID { get; private set; }
        /// <summary>Album's title</summary>
        public string? Title { get; private set; }
        /// <summary>Album's genre</summary>
        public string? Genre { get; private set; }
        /// <summary>Indicates, when album is single track</summary>
        public bool? Single { get; private set; }
        /// <summary>Year, when album is published</summary>
        public uint? Year { get; private set; }
        /// <summary>Exact date of album publication</summary>
        public DateTime? ReleaseDate { get; private set; }
        /// <summary>Indicates, when album has explicit warning</summary>
        public bool? Explicit { get; private set; }
        /// <summary>Count of tracks in this album</summary>
        public uint? TrackCount { get; private set; }
        /// <summary>Count of likes on this album</summary>
        public uint? LikesCount { get; private set; }
        public bool? Recent { get; private set; }
        public bool? VeryImportant { get; private set; }
        /// <summary>Album's image url base (symbols after https:// and needed to replace %% to WIDTHxHEIGHT)</summary>
        public string? ImageURL { get; private set; }
        public Artist[]? Artists { get; private set; }
        public async Task<Image> DownloadImageAsync(int width, int height, bool useECM = true, bool validate = false)
        {
            if (Disposed)
                throw new ObjectDisposedException("Album");
            Cache.Init();
            string path = $"{Cache.ImagesCacheDir}\\{ID}.album.{width}x{height}";
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
            List<int> id = new() { int.Parse(ID) };
            try
            {
                await Parent.AlbumApi.LikesAlbums(id, Parent.UserID);
            }
            catch (InvalidOperationException)
            {
                await Parent.GetUserInfoAsync();
                await Parent.AlbumApi.LikesAlbums(id, Parent.UserID);
            }
        }
        public async Task Unlike()
        {
            if (Disposed)
                throw new ObjectDisposedException("Artist");
            List<int> id = new() { int.Parse(ID) };
            try
            {
                await Parent.AlbumApi.RemoveLikesAlbums(id, Parent.UserID);
            }
            catch (InvalidOperationException)
            {
                await Parent.GetUserInfoAsync();
                await Parent.AlbumApi.RemoveLikesAlbums(id, Parent.UserID);
            }
        }
        public async IAsyncEnumerable<Track> EnumerateTracks()
        {
            yield break;
        }
    }
}