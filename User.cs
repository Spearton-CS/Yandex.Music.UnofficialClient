namespace Yandex.Music.UnofficialClient
{
    public sealed class User : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            Parent = null;
            GC.SuppressFinalize(this);
        }
        public YMusicApi Parent { get; private set; }
        public uint ID { get; }
        public string? Login { get; private set; }
        public string DisplayName { get; private set; }
        public string Sex { get; private set; }
        public bool Verified { get; }
    }
}