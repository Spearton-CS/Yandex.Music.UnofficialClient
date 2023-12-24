namespace Yandex.Music.UnofficialClient
{
    public sealed class User : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            if (Parent?.UsersCache?.ContainsKey(this) == true)
                if (--Parent.UsersCache[this] == 0)
                    Parent.UsersCache.Remove(this);
                else
                    return;
            Disposed = true;
            Login = FullName = DisplayName = Sex = null;
            Parent = null;
            GC.SuppressFinalize(this);
        }
        public YMusicApi? Parent { get; private set; }
        public User(YMusicApi? api, uint id, string? login = null, uint? region = null, string? fullName = null, string? displayName = null, string? sex = null, bool? verified = null, DateTime? registeredAt = null)
        {
            Parent = api;
            ID = id;
            Login = login;
            Region = region;
            FullName = fullName;
            DisplayName = displayName;
            Sex = sex;
            Verified = verified;
            RegisteredAt = registeredAt;
        }
        public uint ID { get; }
        public string? Login { get; private set; }
        public uint? Region { get; private set; }
        public string? FullName { get; private set; }
        public string? DisplayName { get; private set; }
        public string? Sex { get; private set; }
        public bool? Verified { get; }
        public DateTime? RegisteredAt { get; private set; }
    }
}