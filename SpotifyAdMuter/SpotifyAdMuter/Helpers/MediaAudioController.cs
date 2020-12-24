using SpotifyAdMuter.Helpers;
using System.Diagnostics;
using System.Threading;

namespace SpotifyAdMuter.Helpers
{
    public static class MediaAudioController
    {

        public static int[] GetSpotifyProcessIDs()
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

        public static void MuteSpotify(bool mute)
        {
            foreach (var spotifyProcessId in GetSpotifyProcessIDs())
            {
                VolumeMixer.SetApplicationMute(spotifyProcessId, mute);
            }
        }

        private static float restoreVolume = 100;
        public static void FadeSpotifyVolume(bool fadeDown, int fadeSpeed = 10)
        {
            foreach (var spotifyProcessId in GetSpotifyProcessIDs())
            {
                // Get the current volume
                float? initVol = VolumeMixer.GetApplicationVolume(spotifyProcessId);

                if (initVol != null)
                {
                    float animateVolume = (float)initVol;

                    if (!fadeDown)
                    {
                        // If the initial volume is already faded in, then there's no need to fade in.
                        if (initVol == restoreVolume) { return; };
                    }

                    // Animate volume up/down.
                    new Thread(new ThreadStart(() =>
                    {
                        if (fadeDown)
                        {
                            while (animateVolume > 0)
                            {
                                animateVolume -= fadeSpeed;
                                VolumeMixer.SetApplicationVolume(spotifyProcessId, animateVolume);
                                Thread.Sleep(8);
                            }
                            VolumeMixer.SetApplicationVolume(spotifyProcessId, 0);
                        }
                        else
                        {
                            while (animateVolume < restoreVolume)
                            {
                                animateVolume += fadeSpeed;
                                VolumeMixer.SetApplicationVolume(spotifyProcessId, animateVolume);
                                Thread.Sleep(8);
                            }
                            VolumeMixer.SetApplicationVolume(spotifyProcessId, restoreVolume);
                        }
                    })).Start();
                }
            }
        }

    }

}