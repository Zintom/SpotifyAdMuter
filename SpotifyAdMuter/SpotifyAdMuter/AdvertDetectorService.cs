using SpotifyAdMuter.Blockers;
using System.Diagnostics;
using System.Threading;

namespace SpotifyAdMuter
{
    /// <summary>
    /// Actively identifies when the Spotify application is playing an advertisement and mutes the mixer audio when this is detected.
    /// </summary>
    public class AdvertDetectorService
    {
        public delegate void StatusChangedHandler(bool advertPlaying);
        public event StatusChangedHandler? StatusChanged;

        private readonly Thread _serviceThread;
        private volatile bool _serviceThreadStop = false;
        private readonly ManualResetEvent _serviceThreadGate = new(true);

        private IAdvertBlocker? _advertBlocker;
        public IAdvertBlocker? AdvertBlocker { get => _advertBlocker; set => _advertBlocker = value; }

        public AdvertDetectorService(IAdvertBlocker? advertBlocker = null)
        {
            _advertBlocker = advertBlocker;

            // The worker thread should be low impact on the system,
            // so make it Background and BelowNormal priority.
            _serviceThread = new Thread(ServiceThread)
            {
                Priority = ThreadPriority.BelowNormal
            };
        }

        /// <summary>
        /// Begins detecting ads.
        /// </summary>
        public void StartService()
        {
            // If the service was previously running,
            // end it and wait for it to exit.
            StopService();

            _serviceThread.Start();
        }

        /// <summary>
        /// Ends the service and unmutes audio.
        /// </summary>
        public void StopService()
        {
            // Indicate to the service thread that it needs to stop.
            _serviceThreadStop = true;

            // Wait till the service has stopped before adjusting the volume
            // as the service thread may still be trying to mute the volume.
            _serviceThreadGate.WaitOne();

            _serviceThreadStop = false;

            _advertBlocker?.Unblock();
        }

        /// <summary>
        /// The main method to execute on the ServiceThread
        /// </summary>
        private void ServiceThread()
        {
            Debug.WriteLine("Muter service running.");

            // Indicate that this thread has started.
            _serviceThreadGate.Reset();

            bool wasAdvertPlaying = false;
            while (!_serviceThreadStop)
            {
                bool isAdvertPlaying = IsAdvertPlaying();
                StatusChanged?.Invoke(isAdvertPlaying);

                if (!wasAdvertPlaying && isAdvertPlaying)
                {
                    Debug.WriteLine("Advert is playing, running blocker.");
                    _advertBlocker?.Block();
                }
                else if (wasAdvertPlaying && !isAdvertPlaying)
                {
                    Debug.WriteLine("Advert was playing, now transitioned to not playing.");
                    _advertBlocker?.Unblock();
                }

                Debug.WriteLine(isAdvertPlaying ? "Advert playing." : "No advert playing.");

                wasAdvertPlaying = isAdvertPlaying;
                Thread.Sleep(250);
            }

            Debug.WriteLine("Muter service stopped.");

            // Allow the thread to be re-run.
            _serviceThreadGate.Set();
        }

        private static bool IsAdvertPlaying()
        {
            var spotifyProcesses = Process.GetProcessesByName("Spotify");

            if (spotifyProcesses.Length == 0) return false;

            for (int i = 0; i < spotifyProcesses.Length; i++)
            {
                // These are the known window titles that are present
                // when an advert is playing.
                if (spotifyProcesses[i].MainWindowTitle == "Advertisement" ||
                    spotifyProcesses[i].MainWindowTitle == "Shop now")
                {
                    return true;
                }
            }

            return false;
        }

    }
}