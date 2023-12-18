using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yandex.Music.UnofficialClient
{
    public sealed class Settings : IDisposable
    {
        private JObject settings;
        public Settings()
        {
            if (!File.Exists("settings.json"))
                Clear();
            else
                Load();
        }
        public void Clear()
        {
            settings = new()
                {
                    { "theme", "black" }, //Sync only with values "black" and "light" on client side
                    { "volume", 1.0f }, //Sync, but low currency when getting value from server
                    { "autoPlayRadio", true }, //Sync
                    { "explicitForbidden", false }, //Sync
                    { "autoDownload", true }, //Can't sync
                    { "settingsSyncWithServer", "twoWay" }, //Can't sync
                    { "discordRPC", true }, //Can't sync
                    { "saveOnlyLikedTracks", false }, //Can't sync
                    { "token", "" }
                };
            Drop();
        }
        public async Task ClearAsync()
        {
            settings = new()
                {
                    { "theme", "black" }, //Sync only with values "black" and "light" on client side
                    { "volume", 1.0f }, //Sync, but low currency when getting value from server
                    { "autoPlayRadio", true }, //Sync
                    { "explicitForbidden", false }, //Sync
                    { "autoDownload", true }, //Can't sync
                    { "settingsSyncWithServer", "twoWay" }, //Can't sync
                    { "discordRPC", true }, //Can't sync
                    { "saveOnlyLikedTracks", false }, //Can't sync
                    { "token", "" }
                };
            await DropAsync();
        }
        public void Load()
        {
            using StreamReader sr = File.OpenText("settings.json");
            using JsonTextReader reader = new(sr);
            settings = JObject.Load(reader);
        }
        public async Task LoadAsync()
        {
            using StreamReader sr = File.OpenText("settings.json");
            using JsonTextReader reader = new(sr);
            settings = await JObject.LoadAsync(reader);
        }
        public void Drop()
        {
            using StreamWriter sw = new(File.Create("settings.json"));
            using JsonTextWriter writer = new(sw);
            settings.WriteTo(writer);
        }
        public async Task DropAsync()
        {
            using StreamWriter sw = new(File.Create("settings.json"));
            using JsonTextWriter writer = new(sw);
            await settings.WriteToAsync(writer);
        }
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            Drop();
            settings = null;
            GC.SuppressFinalize(this);
        }
        public string Theme
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<string>("theme");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                settings.Property("theme").Value = value;
            }
        }
        public SyncMode SettingsSyncWithServer
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<string>("settingsSyncWithServer") switch
            {
                "oneWay1" or "1Way" or "1WayClient" or "oneWayClient" => SyncMode.OneWayClient,
                "oneWay2" or "1WayServer" or "oneWayServer" => SyncMode.OneWayServer,
                "twoWay" or "2way" => SyncMode.TwoWay,
                _ => SyncMode.None
            };
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                settings.Property("settingsSyncWithServer").Value = value switch
                {
                    SyncMode.OneWayClient => "oneWayClient",
                    SyncMode.OneWayServer => "oneWayServer",
                    SyncMode.TwoWay => "twoWay",
                    _ => "none",
                };
            }
        }
        public string Token
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<string>("token");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                if (value.Length != 58)
                    throw new ArgumentException("Invalid length. Required 58", nameof(value));
                settings.Property("token").Value = value;
            }
        }
        public float Volume
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<float>("volume");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value));
                settings.Property("volume").Value = value;
            }
        }
        public bool AutoPlayRadio
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<bool>("autoPlayRadio");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                settings.Property("autoPlayRadio").Value = value;
            }
        }
        public bool ExplicitForbidden
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<bool>("explicitForbidden");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                settings.Property("explicitForbidden").Value = value;
            }
        }
        public bool AutoDownload
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<bool>("autoDownload");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                settings.Property("autoDownload").Value = value;
            }
        }
        public bool SaveOnlyLikedTracks
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<bool>("saveOnlyLikedTracks");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                settings.Property("saveOnlyLikedTracks").Value = value;
            }
        }
        public bool DiscordRPC
        {
            get => Disposed ? throw new ObjectDisposedException("Settings") : settings.Value<bool>("discordRPC");
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Settings");
                settings.Property("discordRPC").Value = value;
            }
        }
        public enum SyncMode
        {
            None,
            OneWayClient,
            OneWayServer,
            TwoWay
        }
    }
}