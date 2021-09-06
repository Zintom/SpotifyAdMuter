using SpotifyAdMuter.SendInput;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace SpotifyAdMuter.Blockers
{
    /// <summary>
    /// Blocks Spotify ads by killing the app, then relaunching.
    /// </summary>
    public class KillerBlocker : IAdvertBlocker
    {

        private long _lastKillEvent = 0;

        public void Block()
        {
            // We need to give time so that we aren't killing the application too often.
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastKillEvent < 15000)
            {
                return;
            }

            Process[] spotifyProcesses = Process.GetProcessesByName("Spotify");

            if (spotifyProcesses.Length == 0) { return; }

            for (int i = 0; i < spotifyProcesses.Length; i++)
            {
                if (!spotifyProcesses[i].HasExited)
                    spotifyProcesses[i].Kill();
            }

            // Restart the process.
            string? spotifyExePath = spotifyProcesses[0].MainModule?.FileName;
            if (spotifyExePath == null) { return; }

            Process.Start(spotifyExePath);

            // Give the application time to launch!
            Thread.Sleep(2000);

            InputSim.SendInputKeys(Keys.Space);

            _lastKillEvent = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Unblock()
        {

        }
    }
}
