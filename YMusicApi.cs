using YandexMusicApi.Network;
using RApi = YandexMusicApi.Api.Rotor;
using TApi = YandexMusicApi.Api.Track;
using SApi = YandexMusicApi.Api.Default;
using AlbApi = YandexMusicApi.Api.Album;
using ArtApi = YandexMusicApi.Api.Artist;
using PApi = YandexMusicApi.Api.Playlist;
using AccApi = YandexMusicApi.Api.Account;

namespace Yandex.Music.UnofficialClient
{
    /*
     * Cache hierarchy:
     * Artist | User
     * Album | Playlist
     * Track | LocalTrack
     */
    public sealed class YMusicApi : IDisposable
    {
        public Dictionary<Track, uint> TracksCache = new();
        public Dictionary<Artist, uint> ArtistsCache = new();
        public Dictionary<Album, uint> AlbumsCache = new();
        public Dictionary<Playlist, uint> PlaylistsCache = new();
        public Dictionary<User, uint> UsersCache = new();
        private SApi searchApi;
        public SApi SearchApi => Disposed ? throw new ObjectDisposedException("YMusicApi") : searchApi;
        private TApi trackApi;
        public TApi TrackApi => Disposed ? throw new ObjectDisposedException("YMusicApi") : trackApi;
        private AlbApi albumApi;
        public AlbApi AlbumApi => Disposed ? throw new ObjectDisposedException("YMusicApi") : albumApi;
        private ArtApi artistApi;
        public ArtApi ArtistApi => Disposed ? throw new ObjectDisposedException("YMusicApi") : artistApi;
        private PApi playlistApi;
        public PApi PlaylistApi => Disposed ? throw new ObjectDisposedException("YMusicApi") : playlistApi;
        private AccApi accountApi;
        public AccApi AccountApi => Disposed ? throw new ObjectDisposedException("YMusicApi") : accountApi;
        private RApi rotorApi;
        public RApi RotorApi => Disposed ? throw new ObjectDisposedException("YMusicApi") : rotorApi;
        private string token;
        public string Token => Disposed ? throw new ObjectDisposedException("YMusicApi") : token;
        private NetworkParams networkParams;
        public NetworkParams NetworkParams => Disposed ? throw new ObjectDisposedException("YMusicApi") : networkParams;
        private string? userID;
        public string UserID => Disposed ? throw new ObjectDisposedException("YMusicApi") : userID ?? throw new InvalidOperationException("Please call GetUserInfo before UserID getting");
        private string? userFullName;
        public string UserFullName => Disposed ? throw new ObjectDisposedException("YMusicApi") : userFullName ?? throw new InvalidOperationException("Please call GetUserInfo before UserFullName getting");
        private string? userDisplayName;
        public string UserDisplayName => Disposed ? throw new ObjectDisposedException("YMusicApi") : userDisplayName ?? throw new InvalidOperationException("Please call GetUserInfo before UserDisplayName getting");
        private string? userEmail;
        public string UserEmail => Disposed ? throw new ObjectDisposedException("YMusicApi") : userEmail ?? throw new InvalidOperationException("Please call GetUserInfo before UserEmail getting");
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            userID = userFullName = userDisplayName = null;
            searchApi = null;
            trackApi = null;
            albumApi = null;
            artistApi = null;
            playlistApi = null;
            accountApi = null;
            rotorApi = null;
            token = null;
            networkParams = null;
            foreach (var track in TracksCache)
            {
                TracksCache.Remove(track.Key);
                track.Key.Dispose();
            }
            TracksCache = null;
            foreach (var artist in ArtistsCache)
            {
                ArtistsCache.Remove(artist.Key);
                artist.Key.Dispose();
            }
            ArtistsCache = null;
            foreach (var album in AlbumsCache)
            {
                AlbumsCache.Remove(album.Key);
                album.Key.Dispose();
            }
            AlbumsCache = null;
            foreach (var playlist in PlaylistsCache)
            {
                PlaylistsCache.Remove(playlist.Key);
                playlist.Key.Dispose();
            }
            PlaylistsCache = null;
            foreach (var user in UsersCache)
            {
                UsersCache.Remove(user.Key);
                user.Key.Dispose();
            }
            UsersCache = null;
            GC.SuppressFinalize(this);
        }
        public YMusicApi(string token, NetworkParams? np = null)
        {
            networkParams = np ?? new();
            this.token = token;
            searchApi = new(networkParams, token);
            trackApi = new(networkParams, token);
            albumApi = new(networkParams, token);
            artistApi = new(networkParams, token);
            playlistApi = new(networkParams, token);
            accountApi = new(networkParams, token);
            rotorApi = new(networkParams, token);
        }
        public async Task GetUserInfoAsync()
        {
            if (Disposed)
                throw new ObjectDisposedException("YMusicApi");
            var result = (await AccountApi.ShowInformAccount())["result"];
            var account = result["account"];
            userID = account.Value<string>("uid");
            userFullName = account.Value<string>("fullName");
            userDisplayName = account.Value<string>("displayName");
            userEmail = result.Value<string>("defaultEmail");
        }
        public async Task GetSettingsAsync(Settings settings)
        {
            var info = (await AccountApi.ShowSettings())["result"];
            if (settings.Theme == "black" || settings.Theme == "light")
            {
                string theme = info.Value<string>("theme");
                if (theme == "black" || theme == "light")
                    settings.Theme = theme;
                else
                    throw new NotSupportedException("Unknown server-side theme");
            }
            //Etc...
        }
        public async Task SendSettingsAsync(Settings settings)
        {
            Dictionary<string, string> sync = new()
            {

            };
            var result = await AccountApi.SettingsChanged(sync);
        }
        public async IAsyncEnumerable<Track> SearchTracksAsync(string request, int page = 0, int size = 10, bool noCorrect = false, bool best = false)
        {
            var search = (best ? (await SearchApi.SearchBest(request, page, size, "track", noCorrect)) : (await SearchApi.Search(request, page, size, "track", noCorrect)))?["result"]?["tracks"];
            if (search is null || search.Value<uint>("total") == 0)
                yield break;
            foreach (var track in search["results"])
            {
                string id = track.Value<string>("id");
                var founded = TracksCache.Where((x) => x.Key.ID == id);
                if (founded.Any())
                {
                    using Track inf = founded.First().Key;
                    TracksCache[inf]++;
                    yield return inf;
                    TracksCache[inf]--;
                    continue;
                }
                string title, img;
                bool exp;
                uint duration;
                (KeyValuePair<float, float>, KeyValuePair<float, float>) fade;
                Artist[] artists;
                Album[] albums;
                title = track.Value<string>("title");
                duration = track.Value<uint>("durationMs");
                exp = track.Value<string>("contentWarning") == "explicit";
                var fadeInfo = track["fade"];
                fade = fadeInfo is null ? (new(), new()) : (new(fadeInfo.Value<float>("inStart"), fadeInfo.Value<float>("inStop")), new(fadeInfo.Value<float>("outStart"), fadeInfo.Value<float>("outStop")));
                img = track.Value<string>("coverUri");
                var artInfo = track["artists"];
                artists = new Artist[artInfo.Count()];
                for (int i = 0; i < artists.Length; i++)
                {
                    var artist = artInfo[i];
                    string artID = artist.Value<string>("id");
                    var foundedArts = ArtistsCache.Where((x) => x.Key.ID == artID);
                    if (foundedArts.Any())
                    {
                        Artist inf = foundedArts.First().Key;
                        ArtistsCache[inf]++;
                        artists[i] = inf;
                        continue;
                    }
                    string name, artImg;
                    name = artist.Value<string>("name");
                    artImg = artist["cover"]?.Value<string>("uri");
                    Artist artInf = new(this, artID, name, artImg);
                    ArtistsCache.Add(artInf, 1);
                    artists[i] = artInf;
                }
                var albInfo = track["albums"];
                albums = new Album[albInfo.Count()];
                for (int i = 0; i < albums.Length; i++)
                {
                    var album = albInfo[i];
                    string albID = album.Value<string>("id");
                    var foundedAlbs = AlbumsCache.Where((x) => x.Key.ID == albID);
                    if (foundedAlbs.Any())
                    {
                        var inf = foundedAlbs.First();
                        AlbumsCache[inf.Key]++;
                        albums[i] = inf.Key;
                    }
                    string albTitle, albImg, genre;
                    uint year, tracks, likes;
                    DateTime release;
                    bool single, albExp, recent, vimp;
                    Artist[] albArtists;
                    albTitle = album.Value<string>("title");
                    genre = album.Value<string>("genre");
                    try
                    {
                        single = album.Value<string>("type") == "single";
                    }
                    catch
                    {
                        single = false;
                    }
                    year = album.Value<uint>("year");
                    release = album.Value<DateTime>("releaseDate");
                    albExp = album.Value<string>("contentWarning") == "explicit";
                    tracks = album.Value<uint>("trackCount");
                    likes = album.Value<uint>("likesCount");
                    recent = album.Value<bool>("recent");
                    vimp = album.Value<bool>("veryImportant");
                    albImg = album.Value<string>("coverUri");
                    var albArtInfo = album["artists"];
                    albArtists = new Artist[albArtInfo.Count()];
                    for (int j = 0; j < albArtists.Length; j++)
                    {
                        var artist = albArtInfo[j];
                        string artID = artist.Value<string>("id");
                        var foundedArts = ArtistsCache.Where((x) => x.Key.ID == artID);
                        if (foundedArts.Any())
                        {
                            Artist inf = foundedArts.First().Key;
                            ArtistsCache[inf]++;
                            albArtists[i] = inf;
                            continue;
                        }
                        string name, artImg;
                        name = artist.Value<string>("name");
                        artImg = artist["cover"].Value<string>("uri");
                        Artist artInf = new(this, artID, name, artImg);
                        ArtistsCache.Add(artInf, 1);
                        albArtists[j] = artInf;
                    }
                    Album albInf = new(this, albID, albTitle, genre, single, year, release, albExp, tracks, likes, recent, vimp, albImg, albArtists);
                    AlbumsCache.Add(albInf, 1);
                    albums[i] = albInf;
                }
                using Track info = new(this, id, title, duration, exp, img, fade, artists, albums); //Replace to get track by id....
                TracksCache.Add(info, 1);
                yield return info;
                if (--TracksCache[info] == 0)
                    TracksCache.Remove(info);
            }
        }
        public async IAsyncEnumerable<Artist> SearchArtistsAsync(string request, int page = 0, int size = 10, bool noCorrect = false, bool best = false)
        {
            var search = (best ? (await SearchApi.SearchBest(request, page, size, "artist", noCorrect)) : (await SearchApi.Search(request, page, size, "artist", noCorrect)))["result"]["artists"];
            if (search is null || search.Value<uint>("total") == 0)
                yield break;
            foreach (var artist in search["results"])
            {
                string id = artist.Value<string>("id");
                var founded = ArtistsCache.Where((x) => x.Key.ID == id);
                if (founded.Any())
                {
                    using Artist inf = founded.First().Key;
                    ArtistsCache[inf]++;
                    yield return inf;
                    ArtistsCache[inf]--;
                    continue;
                }
                string name, img;
                string[] genres;
                uint likes, ownTracks, ownAlbums, albums, tracks;
                (uint, uint, uint) ratings;
                (string, string, string, string)[] links;
                name = artist.Value<string>("name");
                img = artist.Value<string>("ogImage");
                var genresInfo = artist["genres"];
                genres = new string[genresInfo.Count()];
                for (int i = 0; i < genres.Length; i++)
                    genres[i] = genresInfo[i].ToString();
                likes = artist.Value<uint>("likesCount");
                var counts = artist["counts"];
                ownTracks = counts.Value<uint>("tracks");
                ownAlbums = counts.Value<uint>("directAlbums");
                albums = counts.Value<uint>("alsoAlbums");
                tracks = counts.Value<uint>("alsoTracks");
                var ratingsInfo = artist["ratings"];
                ratings = (ratingsInfo.Value<uint>("month"), ratingsInfo.Value<uint>("week"), ratingsInfo.Value<uint>("day"));
                var linksInfo = artist["links"];
                links = new (string, string, string, string)[linksInfo.Count()];
                for (int i = 0; i < links.Length; i++)
                {
                    var linkInfo = linksInfo[i];
                    links[i] = (linkInfo.Value<string>("socialNetwork"), linkInfo.Value<string>("type"), linkInfo.Value<string>("href"), linkInfo.Value<string>("title"));
                }
                using Artist info = new(this, id, name, img, genres, likes, ownTracks, ownAlbums, albums, tracks, ratings, links);
                ArtistsCache.Add(info, 1);
                yield return info;
                if (--ArtistsCache[info] == 0)
                    ArtistsCache.Remove(info);
            }
        }
        public async IAsyncEnumerable<Album> SearchAlbumsAsync(string request, int page = 0, int size = 10, bool noCorrect = false, bool best = false)
        {
            var search = (best ? (await SearchApi.SearchBest(request, page, size, "album", noCorrect)) : (await SearchApi.Search(request, page, size, "album", noCorrect)))["result"]["albums"];
            if (search is null || search.Value<uint>("total") == 0)
                yield break;
            foreach (var album in search["results"])
            {
                string id = album.Value<string>("id");
                var founded = AlbumsCache.Where((x) => x.Key.ID == id);
                if (founded.Any())
                {
                    using Album inf = founded.First().Key;
                    AlbumsCache[inf]++;
                    yield return inf;
                    AlbumsCache[inf]--;
                    continue;
                }
                string title, genre;
                bool single;
                uint year;
                DateTime release;
                bool exp;
                uint tracks, likes;
                bool recent, vimp;
                string img;
                Artist[] artists;
                title = album.Value<string>("title");
                genre = album.Value<string>("genre");
                try
                {
                    single = album.Value<string>("type") == "single";
                }
                catch
                {
                    single = false;
                }
                year = album.Value<uint>("year");
                release = album.Value<DateTime>("releaseDate");
                exp = album.Value<string>("contentWarning") == "explicit";
                tracks = album.Value<uint>("trackCount");
                likes = album.Value<uint>("likesCount");
                recent = album.Value<bool>("recent");
                vimp = album.Value<bool>("veryImportant");
                img = album.Value<string>("coverUri");
                var artInfo = album["artists"];
                artists = new Artist[artInfo.Count()];
                for (int i = 0; i < artists.Length; i++)
                {
                    var artist = artInfo[i];
                    string artID = artist.Value<string>("id");
                    var foundedArts = ArtistsCache.Where((x) => x.Key.ID == artID);
                    if (foundedArts.Any())
                    {
                        var inf = foundedArts.First();
                        ArtistsCache[inf.Key]++;
                        artists[i] = inf.Key;
                        continue;
                    }
                    string name, artImg;
                    name = artist.Value<string>("name");
                    artImg = artist["cover"]?.Value<string>("uri");
                    Artist artInf = new(this, artID, name, artImg);
                    ArtistsCache.Add(artInf, 1);
                    artists[i] = artInf;
                }
                using Album info = new(this, id, title, genre, single, year, release, exp, tracks, likes, recent, vimp, img, artists);
                AlbumsCache.Add(info, 1);
                yield return info;
                if (--AlbumsCache[info] == 0)
                    AlbumsCache.Remove(info);
            }
        }
        public async IAsyncEnumerable<Playlist> SearchPlaylistsAsync(string request, int page = 0, int size = 10, bool noCorrect = false, bool best = false)
        {
            
            yield break;
        }
        public async IAsyncEnumerable<Track> EnumerateLikedTracksAsync()
        {
            var search = (await TrackApi.GetLikesTrack(UserID))["result"]?["library"]?["tracks"];
            if (search is null)
                yield break;
            List<string> ids = new();
            foreach (var track in search)
                ids.Add(track.Value<string>("id"));
            search = await TrackApi.GetInformTrack(ids);
            foreach (var track in search["result"])
            {
                string id = track.Value<string>("id");
                var founded = TracksCache.Where((x) => x.Key.ID == id);
                if (founded.Any())
                {
                    using Track inf = founded.First().Key;
                    TracksCache[inf]++;
                    yield return inf;
                    TracksCache[inf]--;
                }
                string title, img;
                bool exp;
                uint duration;
                (KeyValuePair<float, float>, KeyValuePair<float, float>) fade;
                Artist[] artists;
                Album[] albums;
                title = track.Value<string>("title");
                duration = track.Value<uint>("durationMs");
                exp = track.Value<string>("contentWarning") == "explicit";
                var fadeInfo = track["fade"];
                fade = (new(fadeInfo.Value<float>("inStart"), fadeInfo.Value<float>("inStop")), new(fadeInfo.Value<float>("outStart"), fadeInfo.Value<float>("outStop")));
                img = track.Value<string>("coverUri");
                var artInfo = track["artists"];
                artists = new Artist[artInfo.Count()];
                for (int i = 0; i < artists.Length; i++)
                {
                    var artist = artInfo[i];
                    string artID = artist.Value<string>("id");
                    var foundedArts = ArtistsCache.Where((x) => x.Key.ID == artID);
                    if (foundedArts.Any())
                    {
                        Artist inf = foundedArts.First().Key;
                        ArtistsCache[inf]++;
                        artists[i] = inf;
                        continue;
                    }
                    string name, artImg;
                    name = artist.Value<string>("name");
                    artImg = artist["cover"]?.Value<string>("uri");
                    Artist artInf = new(this, artID, name, artImg);
                    ArtistsCache.Add(artInf, 1);
                    artists[i] = artInf;
                }
                var albInfo = track["albums"];
                albums = new Album[albInfo.Count()];
                for (int i = 0; i < albums.Length; i++)
                {
                    var album = albInfo[i];
                    string albID = album.Value<string>("id");
                    var foundedAlbs = AlbumsCache.Where((x) => x.Key.ID == albID);
                    if (foundedAlbs.Any())
                    {
                        Album inf = foundedAlbs.First().Key;
                        AlbumsCache[inf]++;
                        albums[i] = inf;
                    }
                    string albTitle, albImg, genre;
                    uint year, tracks, likes;
                    DateTime release;
                    bool single, albExp, recent, vimp;
                    Artist[] albArtists;
                    albTitle = album.Value<string>("title");
                    genre = album.Value<string>("genre");
                    try
                    {
                        single = album.Value<string>("type") == "single";
                    }
                    catch
                    {
                        single = false;
                    }
                    year = album.Value<uint>("year");
                    release = album.Value<DateTime>("releaseDate");
                    albExp = album.Value<string>("contentWarning") == "explicit";
                    tracks = album.Value<uint>("trackCount");
                    likes = album.Value<uint>("likesCount");
                    recent = album.Value<bool>("recent");
                    vimp = album.Value<bool>("veryImportant");
                    albImg = album.Value<string>("coverUri");
                    var albArtInfo = album["artists"];
                    albArtists = new Artist[albArtInfo.Count()];
                    for (int j = 0; j < albArtists.Length; j++)
                    {
                        var artist = albArtInfo[j];
                        string artID = artist.Value<string>("id");
                        var foundedArts = ArtistsCache.Where((x) => x.Key.ID == artID);
                        if (foundedArts.Any())
                        {
                            Artist inf = foundedArts.First().Key;
                            ArtistsCache[inf]++;
                            albArtists[i] = inf;
                            continue;
                        }
                        string name, artImg;
                        name = artist.Value<string>("name");
                        artImg = artist["cover"].Value<string>("uri");
                        Artist artInf = new(this, artID, name, artImg);
                        ArtistsCache.Add(artInf, 1);
                        albArtists[j] = artInf;
                    }
                    Album albInf = new(this, albID, title, genre, single, year, release, albExp, tracks, likes, recent, vimp, albImg, albArtists);
                    AlbumsCache.Add(albInf, 1);
                    albums[i] = albInf;
                }
                Track info = new(this, id, title, duration, exp, img, fade, artists, albums); //Replace to get track by id....
                TracksCache.Add(info, 1);
                yield return info;
                TracksCache[info]--;
            }
        }
        public async Task<Track> GetTrackAsync(string id)
        {
            var founded = TracksCache.Where((x) => x.Key.ID == id);
            if (founded.Any())
            {
                Track inf = founded.First().Key;
                TracksCache[inf]++;
                return inf;
            }
            var result = await TrackApi.GetInformTrack(new() { id });
            var track = result["result"][0];
            string title, img;
            bool exp;
            uint duration;
            (KeyValuePair<float, float>, KeyValuePair<float, float>) fade;
            Artist[] artists;
            Album[] albums;
            title = track.Value<string>("title");
            duration = track.Value<uint>("durationMs");
            exp = track.Value<string>("contentWarning") == "explicit";
            var fadeInfo = track["fade"];
            fade = fadeInfo is null ? (new(), new()) : (new(fadeInfo.Value<float>("inStart"), fadeInfo.Value<float>("inStop")), new(fadeInfo.Value<float>("outStart"), fadeInfo.Value<float>("outStop")));
            img = track.Value<string>("coverUri");
            var artInfo = track["artists"];
            artists = new Artist[artInfo.Count()];
            for (int i = 0; i < artists.Length; i++)
            {
                var artist = artInfo[i];
                string artID = artist.Value<string>("id");
                var foundedArts = ArtistsCache.Where((x) => x.Key.ID == artID);
                if (foundedArts.Any())
                {
                    Artist inf = foundedArts.First().Key;
                    ArtistsCache[inf]++;
                    artists[i] = inf;
                    continue;
                }
                artists[i] = await GetArtistAsync(artID);
            }
            var albInfo = track["albums"];
            albums = new Album[albInfo.Count()];
            for (int i = 0; i < albums.Length; i++)
            {
                var album = albInfo[i];
                string albID = album.Value<string>("id");
                var foundedAlbs = AlbumsCache.Where((x) => x.Key.ID == albID);
                if (foundedAlbs.Any())
                {
                    Album inf = foundedAlbs.First().Key;
                    AlbumsCache[inf]++;
                    albums[i] = inf;
                }
                albums[i] = await GetAlbumAsync(albID);
            }
            Track info = new(this, id, title, duration, exp, img, fade, artists, albums); //Replace to get track by id....
            TracksCache.Add(info, 1);
            return info;
        }
        public async Task<Album> GetAlbumAsync(string id)
        {
            var founded = AlbumsCache.Where((x) => x.Key.ID == id);
            if (founded.Any())
            {
                Album inf = founded.First().Key;
                AlbumsCache[inf]++;
                return inf;
            }
            var album = (await AlbumApi.InformAlbum(int.Parse(id)))["result"];
            string title, genre;
            bool single;
            uint year;
            DateTime release;
            bool exp;
            uint tracks, likes;
            bool recent, vimp;
            string img;
            Artist[] artists;
            title = album.Value<string>("title");
            genre = album.Value<string>("genre");
            try
            {
                single = album.Value<string>("type") == "single";
            }
            catch
            {
                single = false;
            }
            year = album.Value<uint>("year");
            release = album.Value<DateTime>("releaseDate");
            exp = album.Value<string>("contentWarning") == "explicit";
            tracks = album.Value<uint>("trackCount");
            likes = album.Value<uint>("likesCount");
            recent = album.Value<bool>("recent");
            vimp = album.Value<bool>("veryImportant");
            img = album.Value<string>("coverUri");
            var artInfo = album["artists"];
            artists = new Artist[artInfo.Count()];
            for (int i = 0; i < artists.Length; i++)
            {
                var artist = artInfo[i];
                string artID = artist.Value<string>("id");
                var foundedArts = ArtistsCache.Where((x) => x.Key.ID == artID);
                if (foundedArts.Any())
                {
                    var inf = foundedArts.First();
                    ArtistsCache[inf.Key]++;
                    artists[i] = inf.Key;
                    continue;
                }
                Artist artInf = await GetArtistAsync(artID);
                artists[i] = artInf;
            }
            using Album info = new(this, id, title, genre, single, year, release, exp, tracks, likes, recent, vimp, img, artists);
            AlbumsCache.Add(info, 1);
            return info;
        }
        public async Task<Artist> GetArtistAsync(string id)
        {
            var founded = ArtistsCache.Where((x) => x.Key.ID == id);
            if (founded.Any())
            {
                var inf = founded.First();
                ArtistsCache[inf.Key]++;
                return inf.Key;
            }
            var artist = (await ArtistApi.InformArtist(int.Parse(id)))["result"]["artist"];
            string name, img;
            string[] genres;
            uint likes, ownTracks, ownAlbums, albums, tracks;
            (uint, uint, uint) ratings;
            (string, string, string, string)[] links;
            name = artist.Value<string>("name");
            img = artist.Value<string>("ogImage");
            var genresInfo = artist["genres"];
            genres = new string[genresInfo.Count()];
            for (int i = 0; i < genres.Length; i++)
                genres[i] = genresInfo[i].ToString();
            likes = artist.Value<uint>("likesCount");
            var counts = artist["counts"];
            ownTracks = counts.Value<uint>("tracks");
            ownAlbums = counts.Value<uint>("directAlbums");
            albums = counts.Value<uint>("alsoAlbums");
            tracks = counts.Value<uint>("alsoTracks");
            var ratingsInfo = artist["ratings"];
            ratings = ratingsInfo is null ? (0, 0, 0) : (ratingsInfo.Value<uint>("month"), ratingsInfo.Value<uint>("week"), ratingsInfo.Value<uint>("day"));
            var linksInfo = artist["links"];
            links = new (string, string, string, string)[linksInfo.Count()];
            for (int i = 0; i < links.Length; i++)
            {
                var linkInfo = linksInfo[i];
                links[i] = (linkInfo.Value<string>("socialNetwork"), linkInfo.Value<string>("type"), linkInfo.Value<string>("href"), linkInfo.Value<string>("title"));
            }
            Artist info = new(this, id, name, img, genres, likes, ownTracks, ownAlbums, albums, tracks, ratings, links);
            ArtistsCache.Add(info, 1);
            return info;
        }
        public async Task<Playlist> GetPlaylistAsync(uint uid, uint kind)
        {
            var founded = PlaylistsCache.Where((x) => x.Key.UID == uid && x.Key.Kind == kind);
            if (founded.Any())
            {
                var inf = founded.First().Key;
                PlaylistsCache[inf]++;
                return inf;
            }
            
            Playlist info = new(this, uid, kind);
            return info;
        }
        public async Task<KeyValuePair<string, string>> GetDirectLinksAsync(string trackID)
        {
            var dinfo = (await TrackApi.GetDownloadInfoWithToken(trackID))["result"];
            return new(await TrackApi.GetDirectLink(dinfo[0].Value<string>("downloadInfoUrl")), await TrackApi.GetDirectLink(dinfo[1].Value<string>("downloadInfoUrl")));
        }
    }
}