using NAudio.Wave;
using System.IO;

namespace Yandex.Music.UnofficialClient
{
    public sealed class Player : IDisposable
    {
        private WaveOut wo = new();
        private Stream? Current = null;
        private bool Own = false;
        /// <summary>Initializes local player</summary>
        public Player() { }
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            wo.Stop();
            wo.Dispose();
            wo = null;
            if (Own)
                Current?.Close();
            Current = null;
            GC.SuppressFinalize(this);
        }
        public float Volume
        {
            get => Disposed ? throw new ObjectDisposedException("Player") : wo.Volume;
            set
            {
                if (Disposed)
                    throw new ObjectDisposedException("Player");
                wo.Volume = value;
            }
        }
        public void Play()
        {
            if (Disposed)
                throw new ObjectDisposedException("Player");
            wo.Play();
        }
        public void Play(string file, bool start = true)
        {
            if (Disposed)
                throw new ObjectDisposedException("Player");
            wo.Stop();
            if (Own)
                Current?.Close();
            Current = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            Own = false;
            try
            {
                using Mp3FileReader reader = new(Current);
                wo.Init(reader);
                if (start)
                    wo.Play();
            }
            catch
            {
                Current = null;
                throw;
            }
        }
        public void Play(Stream stream, bool start = true)
        {
            if (Disposed)
                throw new ObjectDisposedException("Player");
            wo.Stop();
            if (Own)
                Current?.Close();
            Current = stream;
            Own = false;
            try
            {
                using Mp3FileReader reader = new(stream);
                wo.Init(reader);
                if (start)
                    wo.Play();
            }
            catch
            {
                Current = null;
                throw;
            }
        }
        public async Task PlayAsync(Track track, bool hq = true, bool start = true)
        {
            if (Disposed)
                throw new ObjectDisposedException("Player");
            wo.Stop();
            if (Own)
                Current?.Close();
            Current = await track.GetStreamAsync(hq);
            Own = true;
            try
            {
                using Mp3FileReader reader = new(Current);
                wo.Init(reader);
                if (start)
                    wo.Play();
            }
            catch
            {
                Current.Close();
                Current = null;
                throw;
            }
        }
        public void Stop()
        {
            if (Disposed)
                throw new ObjectDisposedException("Player");
            wo.Stop();
            if (Own)
                Current?.Close();
            Current = null;
            Own = false;
        }
        public void Pause()
        {
            if (Disposed)
                throw new ObjectDisposedException("Player");
            wo.Pause();
        }
        public void Resume()
        {
            if (Disposed)
                throw new ObjectDisposedException("Player");
            wo.Resume();
        }
        public event EventHandler<StoppedEventArgs> PlaybackStopped
        {
            remove
            {
                if (Disposed)
                    throw new ObjectDisposedException("Player");
                wo.PlaybackStopped -= value;
            }
            add
            {
                if (Disposed)
                    throw new ObjectDisposedException("Player");
                wo.PlaybackStopped += value;
            }
        }
    }
}