namespace Yandex.Music.UnofficialClient
{
    public sealed class Artist : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            if (Parent?.ArtistsCache.ContainsKey(this) == true)
                if (Parent.ArtistsCache[this] > 0)
                    return;
            Disposed = true;
            ID = Name = ImageURL = null;
            Genres = null;
            Links = null;
            Parent = null;
            GC.SuppressFinalize(this);
        }
        /// <summary>Y.Music api instance, where cached this artist's info</summary>
        public YMusicApi Parent { get; private set; }
        public Artist(YMusicApi api, string id, string name, string img, string[] genres, uint likes, uint ownTracks, uint ownAlbums, uint albums, uint tracks, (uint, uint, uint) ratings, (string, string, string, string)[] links)
        {
            Parent = api;
            ID = id;
            Name = name;
            ImageURL = img;
            Genres = genres;
            LikesCount = likes;
            TracksCount = ownTracks;
            DirectAlbumsCount = ownAlbums;
            AlsoAlbumsCount = albums;
            AlsoTracksCount = tracks;
            Ratings = ratings;
            Links = links;
        }
        /// <summary>Artist's ID</summary>
        public string ID { get; private set; }
        /// <summary>Artist's nickname</summary>
        public string Name { get; private set; }
        /// <summary>Artist's image url base (symbols after https:// and needed to replace %% to WIDTHxHEIGHT)</summary>
        public string ImageURL { get; private set; }
        /// <summary>List of genres in which the artist released his tracks</summary>
        public string[] Genres { get; private set; }
        /// <summary>Count of likes on this artist</summary>
        public uint LikesCount { get; }
        /// <summary>Count of own tracks</summary>
        public uint TracksCount { get; }
        /// <summary>Count of own albums</summary>
        public uint DirectAlbumsCount { get; }
        /// <summary>Count of albums, where artist is co-creator</summary>
        public uint AlsoAlbumsCount { get; }
        /// <summary>Count of tracks, where artist is co-creator</summary>
        public uint AlsoTracksCount { get; }
        public (uint Month, uint Week, uint Day) Ratings { get; }
        /// <summary>List of social or commercial networks, where artist is registered and connected with Y.Music</summary>
        public (string Network, string Type, string Link, string Title)[] Links { get; private set; }
        public async Task<Image> DownloadImageAsync(int width, int height, bool useECM = true, bool validate = false)
        {
            if (Disposed)
                throw new ObjectDisposedException("Artist");
            Cache.Init();
            string path = $"{Cache.ImagesCacheDir}\\{ID}.artist.{width}x{height}";
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
                await Parent.ArtistApi.LikesArtists(id, Parent.UserID);
            }
            catch (InvalidOperationException)
            {
                await Parent.GetUserInfoAsync();
                await Parent.ArtistApi.LikesArtists(id, Parent.UserID);
            }
        }
        public async Task Unlike()
        {
            if (Disposed)
                throw new ObjectDisposedException("Artist");
            List<int> id = new() { int.Parse(ID) };
            try
            {
                await Parent.ArtistApi.RemoveLikesArtists(id, Parent.UserID);
            }
            catch (InvalidOperationException)
            {
                await Parent.GetUserInfoAsync();
                await Parent.ArtistApi.RemoveLikesArtists(id, Parent.UserID);
            }
        }
        public async IAsyncEnumerable<Track> EnumerateTracks()
        {
            yield break;
        }
        public async IAsyncEnumerable<Album> EnumerateAlbums()
        {
            yield break;
        }
    }
}