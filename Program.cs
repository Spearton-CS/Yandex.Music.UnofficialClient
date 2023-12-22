namespace Yandex.Music.UnofficialClient
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Task.Run(async () => //Test
            {
                using YMusicApi api = new("[TOKEN]");
                await api.GetUserInfoAsync();
                var tracks = api.EnumerateLikedTracksAsync();
                var enumerator = tracks.GetAsyncEnumerator();
                if (!Directory.Exists("Likes"))
                    Directory.CreateDirectory("Likes");
                while (await enumerator.MoveNextAsync())
                {
                    var track = enumerator.Current;
                    using StreamWriter writer = new(File.Create($"Likes\\{track.ID}.txt"));
                    await writer.WriteLineAsync("Title: " + track.Title);
                    for (int i = 0; i < track.Albums.Length; i++)
                    {
                        if (i == 0)
                            await writer.WriteLineAsync("Albums:");
                        await writer.WriteLineAsync("\t" + track.Albums[i].Title);
                    }
                    for (int i = 0; i < track.Artists.Length; i++)
                    {
                        if (i == 0)
                            await writer.WriteLineAsync("Artists:");
                        await writer.WriteLineAsync("\t" + track.Artists[i].Name);
                    }
                    await writer.WriteLineAsync("Img: " + track.ImageURL);
                    await writer.WriteLineAsync("Duration (ms): " + track.Duration);
                    await writer.FlushAsync();
                }
            }).Wait();
            Cache.Init();
            Application.Run(new MainWindow());
            Cache.Clear();
        }
    }
}