using System.Diagnostics;
using System.Threading;

namespace SpotifyAdMuter.Helpers
{
    /// <summary>
    /// Provides helper methods to mute/fade the audio on the Spotify application.
    /// </summary>
    public class SpotifyAudioController
    {

        public enum FadeDirection
        {
            Up,
            Down
        }

        private readonly IVolumeMixer _volumeMixer;

        public SpotifyAudioController(IVolumeMixer volumeMixer)
        {
            _volumeMixer = volumeMixer;
        }

        private int[] GetSpotifyProcessIDs()
        {
            // Get all processes named "Spotify"
            Process[] pList = Process.GetProcessesByName("Spotify");
            int[] spotifyProcessIDs = new int[pList.Length];

            for (int p = 0; p < pList.Length; p++)
            {
                spotifyProcessIDs[p] = pList[p].Id;
            }

            return spotifyProcessIDs;
        }

        public void MuteSpotify(bool mute)
        {
            foreach (var spotifyProcessId in GetSpotifyProcessIDs())
            {
                _volumeMixer.SetApplicationMute(spotifyProcessId, mute);
            }
        }

        private readonly float _restoreVolume = 1;
        public void FadeSpotifyVolume(FadeDirection fadeDirection, float fadeSpeed = 0.1f)
        {
            foreach (var spotifyProcessId in GetSpotifyProcessIDs())
            {
                // Get the current volume
                float? initVol = _volumeMixer.GetApplicationVolume(spotifyProcessId);

                if (initVol != null)
                {
                    float animateVolume = (float)initVol;

                    if (fadeDirection == FadeDirection.Up)
                    {
                        // If the initial volume is already faded in, then there's no need to fade in.
                        if (initVol == _restoreVolume) { return; };
                    }

                    // Animate volume up/down.
                    new Thread(new ThreadStart(() =>
                    {
                        if (fadeDirection == FadeDirection.Down)
                        {
                            while (animateVolume > 0)
                            {
                                animateVolume -= fadeSpeed;
                                _volumeMixer.SetApplicationVolume(spotifyProcessId, animateVolume);
                                Thread.Sleep(8);
                            }
                            _volumeMixer.SetApplicationVolume(spotifyProcessId, 0);
                        }
                        else
                        {
                            while (animateVolume < _restoreVolume)
                            {
                                animateVolume += fadeSpeed;
                                _volumeMixer.SetApplicationVolume(spotifyProcessId, animateVolume);
                                Thread.Sleep(8);
                            }
                            _volumeMixer.SetApplicationVolume(spotifyProcessId, _restoreVolume);
                        }
                    })).Start();
                }
            }
        }

    }

}