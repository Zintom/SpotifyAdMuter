using System;
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
        private readonly MuterService muterService = new MuterService();

        private readonly NotifyIcon _trayIcon;

        public TrayApplicationContext()
        {
            ContextMenuStrip menuStrip = new ContextMenuStrip();
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

            muterService.StatusChanged += MuterService_StatusChanged;
            muterService.BeginMuting();
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
            muterService.EndMuting();

            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            Application.Exit();
        }
    }
}
