namespace Yandex.Music.UnofficialClient
{
    public sealed class Playlist : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {

        }
        public string ID { get; }
    }
}