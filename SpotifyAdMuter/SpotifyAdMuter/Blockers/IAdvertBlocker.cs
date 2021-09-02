namespace SpotifyAdMuter.Blockers
{
    /// <summary>
    /// Performs some sort of advert blocking logic so that Spotify can't play ads.
    /// </summary>
    public interface IAdvertBlocker
    {

        /// <summary>
        /// Block adverts.
        /// </summary>
        void Block();

        /// <summary>
        /// Unblock adverts.
        /// </summary>
        void Unblock();

    }
}
