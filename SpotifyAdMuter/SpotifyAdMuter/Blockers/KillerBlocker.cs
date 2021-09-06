using SpotifyAdMuter.SendInput;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SpotifyAdMuter.Blockers
{
    /// <summary>
    /// Blocks Spotify ads by killing the app, then relaunching.
    /// </summary>
    public class KillerBlocker : IAdvertBlocker
    {

        const uint WM_APPCOMMAND = 0x0319;
        const uint APPCOMMAND_MEDIA_PLAY = 46;

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The timestamp when <see cref="Block"/> was last run.
        /// </summary>
        private long _lastBlockEvent = 0;

        /// <summary>
        /// Kills Spotify and fills <paramref name="mainModuleFileName"/> with the process executable file path.
        /// </summary>
        /// <returns></returns>
        private static void KillSpotifyProcesses(out string? mainModuleFileName)
        {
            Process[] spotifyProcesses = Process.GetProcessesByName("Spotify");

            if (spotifyProcesses.Length == 0)
            {
                mainModuleFileName = null;
                return;
            }

            mainModuleFileName = spotifyProcesses[0]?.MainModule?.FileName;

            for (int i = 0; i < spotifyProcesses.Length; i++)
            {
                if (!spotifyProcesses[i].HasExited)
                    spotifyProcesses[i].Kill();
            }
        }

        public void Block()
        {
            // We need to give time so that we aren't killing the application too often.
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastBlockEvent < 15000)
            {
                return;
            }

            KillSpotifyProcesses(out string? spotifyExePath);

            if (spotifyExePath == null) { return; }

            // Restart the process minimized.
            ProcessStartInfo startInfo = new ProcessStartInfo(spotifyExePath, "--minimized");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            Process? spotifyProcess = Process.Start(startInfo);

            if (spotifyProcess == null) { return; }

            // Give the application time to launch!
            Thread.Sleep(2500);

            IntPtr spotifyWindowHwnd = spotifyProcess.MainWindowHandle;

            if (spotifyWindowHwnd == IntPtr.Zero) { return; }

            // Send an APPCOMMAND to the Spotify process which instructs it to begin playback.
            PostMessage(spotifyWindowHwnd, WM_APPCOMMAND, IntPtr.Zero, (IntPtr)MAKELPARAM(0, (short)APPCOMMAND_MEDIA_PLAY));

            _lastBlockEvent = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// An LPARAM (32 bits) can be split into a high and low word of 16 bits each.
        /// <para/>
        /// This method combines two 16 bit values into a single 32 bit value.
        /// </summary>
        private static int MAKELPARAM(short lowWord, short highWord)
        {
            // Push the low order bytes of the highWord to the high end of the output, mask the high order bytes from the LoWord.
            return (highWord << 16) | (lowWord & ushort.MaxValue);
        }

        public void Unblock()
        {

        }

    }
}
