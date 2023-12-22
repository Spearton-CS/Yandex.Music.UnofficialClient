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
                using Settings settings = new();
                using YMusicApi api = new(settings.Token);
                await api.GetUserInfoAsync();
            }).Wait();
            GC.Collect();
            MessageBox.Show("Trash...");
            return;
            Cache.Init();
            Application.Run(new MainWindow());
            Cache.Clear();
        }
    }
}