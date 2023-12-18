namespace Yandex.Music.UnofficialClient
{
    public static class Cache
    {
        public const string CacheDir = "Cache", TracksCacheDir = @"Cache\Tracks", DownloadedTracksCacheDir = @"Cache\DownloadedTracks", InfoCacheDir = @"Cache\Info", ImagesCacheDir = @"Cache\Info\Images";
        public static void Init()
        {
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
            if (!Directory.Exists(TracksCacheDir))
                Directory.CreateDirectory(TracksCacheDir);
            if (!Directory.Exists(DownloadedTracksCacheDir))
                Directory.CreateDirectory(DownloadedTracksCacheDir);
            if (!Directory.Exists(InfoCacheDir))
                Directory.CreateDirectory(InfoCacheDir);
            if (!Directory.Exists(ImagesCacheDir))
                Directory.CreateDirectory(ImagesCacheDir);
        }
        public static void Clear()
        {
            Init();
            ClearTracksCache();
        }
        #region Tracks Cache
        public static void ClearTracksCache()
        {
            Init();
            foreach (string cached in Directory.EnumerateFiles(TracksCacheDir))
                File.Delete(cached);
        }
        public static async IAsyncEnumerable<Track> EnumerateCachedTracksAsync(this YMusicApi api)
        {
            Init();
            foreach (string cached in Directory.EnumerateFiles(TracksCacheDir))
                using (Track track = await api.GetTrackAsync(Path.GetFileNameWithoutExtension(cached)))
                    yield return track;
        }
        public static bool IsCached(this Track track)
        {
            Init();
            return TrackIsCached(track.ID);
        }
        public static bool TrackIsCached(string id)
        {
            Init();
            return File.Exists($"{TracksCacheDir}\\{id}.mp3");
        }
        public static void RemoveFromCache(this Track track)
        {
            Init();
            RemoveTrack(track.ID);
        }
        public static void RemoveTrack(string id)
        {
            Init();
            string path = Path.Combine(TracksCacheDir, id + ".mp3");
            if (File.Exists(path))
                File.Delete(path);
        }
        public static async Task CacheTrackAsync(Track track, bool hq = true, bool overwrite = false)
        {
            Init();
            string path = Path.Combine(TracksCacheDir, track.ID + ".mp3");
            if (!File.Exists(path) || overwrite)
                await track.DownloadAsync(hq, path);
        }
        public static async Task CacheTrackAsync(this YMusicApi api, string id, bool hq = true, bool overwrite = false)
        {
            Init();
            string path = Path.Combine(TracksCacheDir, id + ".mp3");
            if (!File.Exists(path) || overwrite)
            {
                using Track track = await api.GetTrackAsync(id);
                await track.DownloadAsync(hq, path);
            }
        }
        #endregion
        #region Offline Cache (Downloaded/liked items images and info, downloaded tracks)
        #region Info
        #endregion
        #region Images
        public static bool ImageIsCached(string id, string type, int width, int height)
        {
            Init();
            return File.Exists($"{ImagesCacheDir}\\{id}.{type}.{width}x{height}");
        }
        #endregion
        #region Downloaded Tracks
        #endregion
        #endregion
    }
}