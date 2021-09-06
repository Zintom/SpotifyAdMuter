using SpotifyAdMuter.Blockers;
using SpotifyAdMuter.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpotifyAdMuter
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new TrayApplicationContext());
        }
    }

    public class TrayApplicationContext : ApplicationContext
    {

        private readonly NotifyIcon _trayIcon;

        private readonly ToolStripMenuItem _muterModeBtn;
        private readonly ToolStripMenuItem _afkModeBtn;

        private readonly AdvertDetectorService _detectorService = new AdvertDetectorService();

        private enum BlockingMode
        {
            None,
            Muter,
            Killer
        }

        private BlockingMode _blockingMode = BlockingMode.None;

        public TrayApplicationContext()
        {
            _muterModeBtn = new ToolStripMenuItem("Muting Mode", null, (o, e) => { ChangeMode(BlockingMode.Muter); });
            _afkModeBtn = new ToolStripMenuItem("Kill Mode", null, (o, e) => { ChangeMode(BlockingMode.Killer); });

            _muterModeBtn.ToolTipText = Properties.Resources.BlockingModeMuter;
            _afkModeBtn.ToolTipText = Properties.Resources.BlockingModeAfk;

            var menuStrip = new ContextMenuStrip();
            menuStrip.Items.Add(_muterModeBtn);
            menuStrip.Items.Add(_afkModeBtn);
            menuStrip.Items.Add(new ToolStripSeparator());
            menuStrip.Items.Add(new ToolStripMenuItem("Exit", null, Exit));

            // Initialize Tray Icon
            _trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.AppIcon,
                ContextMenuStrip = menuStrip,
                Visible = true
            };

            _trayIcon.BalloonTipTitle = "Spotify Muter Service";
            _trayIcon.BalloonTipText = "I'm running in the system tray.";
            _trayIcon.ShowBalloonTip(3000);

            ChangeMode((BlockingMode)Properties.Settings.Default.BlockingMode);

            _detectorService.StatusChanged += MuterService_StatusChanged;
            _detectorService.StartService();
        }

        private void ChangeMode(BlockingMode mode)
        {
            if (_blockingMode == mode) { return; }

            _blockingMode = mode;
            Properties.Settings.Default.BlockingMode = (byte)_blockingMode;
            Properties.Settings.Default.Save();
            switch (mode)
            {
                case BlockingMode.Muter:
                    _muterModeBtn.Checked = true;
                    _afkModeBtn.Checked = false;

                    _detectorService.AdvertBlocker = new MuterBlocker(new SpotifyAudioController(new NAudioVolumeMixer()));
                    break;
                case BlockingMode.Killer:
                    _afkModeBtn.Checked = true;
                    _muterModeBtn.Checked = false;

                    _detectorService.AdvertBlocker = new KillerBlocker();
                    break;
            }
        }

        private void MuterService_StatusChanged(bool advertPlaying)
        {
            if (advertPlaying)
            {
                _trayIcon.Text = "Spotify muted, nothing is playing or an advert is playing.";
            }
            else
            {
                _trayIcon.Text = "No adverts detected.";
            }
        }

        void Exit(object? sender, EventArgs e)
        {
            _detectorService.StopService();

            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            Application.Exit();
        }
    }
}
