namespace Yandex.Music.UnofficialClient
{
    internal static class Program
    {
        [STAThread]
        private static async Task Main()
        {
            ApplicationConfiguration.Initialize();
            Cache.Init();
            bool test = true;
            if (test)
            {
                const string token = "y0_AgAAAABXvh0yAAG8XgAAAADj8VogRlMiilunQ6yb41lwjprSXVBLnHI";
                using YMusicApi api = new(token);
                await api.GetUserInfoAsync();
                File.WriteAllText("a.txt", (await api.TrackApi.GetLikesTrack(api.UserID))["result"]["library"]["tracks"].ToString());
                return;
                //using Player player = new();
                //player.Volume = 0.2f;
                var tracks = (await api.TrackApi.GetLikesTrack(api.UserID))["result"]["library"]["tracks"];
                List<Track> list = new();
                List<KeyValuePair<int, Exception>> errors = new();
                int i = 0;
                foreach (var trackInf in tracks)
                {
                    try
                    {
                        string id = trackInf.Value<string>("id");
                        Track track = await api.GetTrackAsync(id);
                        list.Add(track);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new(i, ex));
                    }
                    i++;
                    File.WriteAllText("i.txt", i.ToString());
                }
                using (StreamWriter writer = new(File.Create("errors.txt")))
                    foreach (var error in errors)
                        await writer.WriteLineAsync($"[{error.Key}] {error.Value}");
               MessageBox.Show($"Check all [{list.Count}] with {errors.Count} errors");
                list.ForEach((x) =>
                {
                    api.TracksCache.Remove(x);
                    x.Dispose();
                });
                list.Clear();
                MessageBox.Show("Check empty");
            }
            else
                Application.Run(new MainWindow());
            Cache.Clear();
        }
    }
}