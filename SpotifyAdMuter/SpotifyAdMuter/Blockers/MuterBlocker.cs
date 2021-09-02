using SpotifyAdMuter.Helpers;
using System.Diagnostics;
using static SpotifyAdMuter.Helpers.SpotifyAudioController;

namespace SpotifyAdMuter.Blockers
{
    /// <summary>
    /// Blocks spotify Adverts by muting for the period of time that they occur.
    /// </summary>
    public class MuterBlocker : IAdvertBlocker
    {

        private readonly SpotifyAudioController _spotifyAudioController;

        public MuterBlocker(SpotifyAudioController audioController)
        {
            _spotifyAudioController = audioController;
        }

        public void Block()
        {
            _spotifyAudioController.FadeSpotifyVolume(FadeDirection.Down, 0.10f);

            Debug.WriteLine("Adverts detected, muting audio.");
        }

        public void Unblock()
        {
            _spotifyAudioController.MuteSpotify(false);
            _spotifyAudioController.FadeSpotifyVolume(FadeDirection.Up, 0.10f);

            Debug.WriteLine("Adverts finished, unmuted audio.");
        }
    }
}
