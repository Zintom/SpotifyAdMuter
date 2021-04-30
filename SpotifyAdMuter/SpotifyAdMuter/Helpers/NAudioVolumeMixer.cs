using NAudio.CoreAudioApi;

namespace SpotifyAdMuter.Helpers
{
    public class NAudioVolumeMixer : IVolumeMixer
    {
        public float? GetApplicationVolume(int processID)
        {
            AudioSessionControl? asc = GetAudioSession(processID);

            return asc?.SimpleAudioVolume?.Volume ?? null;
        }

        public void SetApplicationMute(int processID, bool mute)
        {
            AudioSessionControl? asc = GetAudioSession(processID);
            if (asc == null) return;

            asc.SimpleAudioVolume.Mute = mute;
        }

        public void SetApplicationVolume(int processID, float level)
        {
            AudioSessionControl? asc = GetAudioSession(processID);
            if (asc == null) return;

            asc.SimpleAudioVolume.Volume = level;
        }

        /// <summary>
        /// Gets the audio session for the given process, if that process exists and has an audio session.
        /// </summary>
        private static AudioSessionControl? GetAudioSession(int processID)
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    var sessions = device.AudioSessionManager.Sessions;

                    for (int i = 0; i < sessions.Count; i++)
                    {
                        if (sessions[i].GetProcessID == processID)
                        {
                            return sessions[i];
                        }
                    }
                }

                return null;
            }
        }
    }
}
