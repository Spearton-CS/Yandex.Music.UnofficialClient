namespace Yandex.Music.UnofficialClient
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Cache.Init();
            Application.Run(new MainWindow());
            Cache.Clear();
        }
    }
}