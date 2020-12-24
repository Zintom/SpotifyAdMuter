using SpotifyAdMuter.Helpers;
using System.Diagnostics;
using System.Threading;

namespace SpotifyAdMuter
{
    /// <summary>
    /// Actively identifies when the Spotify application is playing an advertisement and mutes the mixer audio when this is detected.
    /// </summary>
    public class MuterService
    {
        public delegate void StatusChangedHandler(bool advertPlaying);
        public event StatusChangedHandler? StatusChanged;

        private readonly Thread _serviceThread;
        private volatile bool _serviceThreadStop = false;
        private readonly ManualResetEvent _serviceThreadGate = new ManualResetEvent(true);

        public MuterService()
        {
            // The worker thread should be low impact on the system,
            // so make it Background and BelowNormal priority.
            _serviceThread = new Thread(ServiceThread)
            {
                Priority = ThreadPriority.BelowNormal
            };
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        public void BeginMuting()
        {
            // If the service was previously running,
            // wait for it to finish.
            _serviceThreadGate.WaitOne();

            _serviceThread.Start();
        }

        /// <summary>
        /// Ends the service and unmutes audio.
        /// </summary>
        public void EndMuting()
        {
            // Indicate to the service thread that it needs to stop.
            _serviceThreadStop = true;

            // Wait till the service has stopped before adjusting the volume
            // as the service thread may still be trying to mute the volume.
            _serviceThreadGate.WaitOne();

            _serviceThreadStop = false;

            MediaAudioController.MuteSpotify(false);
            MediaAudioController.FadeSpotifyVolume(false, 15);
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
                    MediaAudioController.FadeSpotifyVolume(true, 10);

                    Debug.WriteLine("Adverts detected, muting audio.");
                }
                else if (wasAdvertPlaying && !isAdvertPlaying)
                {
                    MediaAudioController.MuteSpotify(false);
                    MediaAudioController.FadeSpotifyVolume(false, 10);

                    Debug.WriteLine("Adverts finished, unmuted audio.");
                }

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

            // If all the MainWindowTitle's include the name Spotify,
            // this indicates that a Sponsored Message is showing
            // as the MainWindowTitle should be the name of a song or "Advertisement".
            bool allTitlesIncludeSpotify = true;

            for (int i = 0; i < spotifyProcesses.Length; i++)
            {
                // If the MainWindowTitle is blatantly "Advertisement" then immediately return.
                if (spotifyProcesses[i].MainWindowTitle == "Advertisement")
                {
                    return true;
                }

                if (!spotifyProcesses[i].MainWindowTitle.StartsWith("Spotify") &&
                    !string.IsNullOrEmpty(spotifyProcesses[i].MainWindowTitle))
                {
                    allTitlesIncludeSpotify = false;
                }
            }

            return allTitlesIncludeSpotify;
        }

    }
}