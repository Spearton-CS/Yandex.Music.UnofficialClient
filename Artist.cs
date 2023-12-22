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
        public Artist(YMusicApi api, string id, string? name = null, string? img = null, string[]? genres = null, uint? likes = null, uint? ownTracks = null, uint? ownAlbums = null, uint? albums = null, uint? tracks = null, (uint, uint, uint)? ratings = null, (string, string, string, string)[]? links = null)
        {
            Parent = api;
            Full = name is not null && img is not null && genres is not null && likes is not null && ownTracks is not null && ownAlbums is not null && albums is not null && tracks is not null && ratings is not null && links is not null;
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
        public bool Full { get; private set; }
        public async Task GetFullAsync()
        {
            if (Full)
                return;
            var artist = (await Parent.ArtistApi.InformArtist(int.Parse(ID)))["result"]["artist"];
            Name = artist.Value<string>("name");
            ImageURL = artist.Value<string>("ogImage");
            var genresInfo = artist["genres"];
            Genres = new string[genresInfo.Count()];
            for (int i = 0; i < Genres.Length; i++)
                Genres[i] = genresInfo[i].ToString();
            LikesCount = artist.Value<uint>("likesCount");
            var counts = artist["counts"];
            TracksCount = counts.Value<uint>("tracks");
            DirectAlbumsCount = counts.Value<uint>("directAlbums");
            AlsoAlbumsCount = counts.Value<uint>("alsoAlbums");
            AlsoTracksCount = counts.Value<uint>("alsoTracks");
            var ratingsInfo = artist["ratings"];
            Ratings ??= ratingsInfo is null ? (0, 0, 0) : (ratingsInfo.Value<uint>("month"), ratingsInfo.Value<uint>("week"), ratingsInfo.Value<uint>("day"));
            var linksInfo = artist["links"];
            Links = new (string, string, string, string)[linksInfo.Count()];
            for (int i = 0; i < Links.Length; i++)
            {
                var linkInfo = linksInfo[i];
                Links[i] = (linkInfo.Value<string>("socialNetwork"), linkInfo.Value<string>("type"), linkInfo.Value<string>("href"), linkInfo.Value<string>("title"));
            }
        }
        /// <summary>Artist's ID</summary>
        public string ID { get; private set; }
        /// <summary>Artist's nickname</summary>
        public string? Name { get; private set; }
        /// <summary>Artist's image url base (symbols after https:// and needed to replace %% to WIDTHxHEIGHT)</summary>
        public string? ImageURL { get; private set; }
        /// <summary>List of genres in which the artist released his tracks</summary>
        public string[]? Genres { get; private set; }
        /// <summary>Count of likes on this artist</summary>
        public uint? LikesCount { get; private set; }
        /// <summary>Count of own tracks</summary>
        public uint? TracksCount { get; private set; }
        /// <summary>Count of own albums</summary>
        public uint? DirectAlbumsCount { get; private set; }
        /// <summary>Count of albums, where artist is co-creator</summary>
        public uint? AlsoAlbumsCount { get; private set; }
        /// <summary>Count of tracks, where artist is co-creator</summary>
        public uint? AlsoTracksCount { get; private set; }
        public (uint Month, uint Week, uint Day)? Ratings { get; private set; }
        /// <summary>List of social or commercial networks, where artist is registered and connected with Y.Music</summary>
        public (string Network, string Type, string Link, string Title)[]? Links { get; private set; }
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
        public override string? ToString() => Name;
    }
}