namespace SpotifyAdMuter.Helpers
{
    public interface IVolumeMixer
    {
        /// <summary>
        /// Gets the volume of the audio session associated with the given <paramref name="processID"/>.
        /// </summary>
        /// <param name="processID"></param>
        /// <returns>A floating value between 0 and 1 which represents the volume of the session.</returns>
        public float? GetApplicationVolume(int processID);

        /// <summary>
        /// Sets the volume of the audio session associated with the given <paramref name="processID"/>.
        /// </summary>
        /// <param name="level">The volume level to set it to, from 0 to 1</param>
        public void SetApplicationVolume(int processID, float level);

        /// <summary>
        /// Mutes the audio session associated with the given <paramref name="processID"/>.
        /// </summary>
        /// <param name="mute">Mute = <see langword="true"/>, Unmute = <see langword="false"/>.</param>
        public void SetApplicationMute(int processID, bool mute);
    }
}
