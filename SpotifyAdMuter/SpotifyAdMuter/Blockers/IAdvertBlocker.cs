namespace SpotifyAdMuter.Blockers
{
    /// <summary>
    /// Performs some sort of advert blocking logic so that Spotify can't play ads.
    /// </summary>
    public interface IAdvertBlocker
    {

        void Block();

        void Unblock();

    }
}
