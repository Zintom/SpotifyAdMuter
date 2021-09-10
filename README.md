# SpotifyAdMuter
Blocks adverts from playing on Spotify.

## Modes
In the current release, there are two modes of blocking.

* **Killing**: When an advert is detected, we kill the Spotify process, re-launch it, and send the relevant `APPCOMMAND`'s to get the next track playing. This is seemless and is what I believe to be the most practical.
* **Muting**: When an advert is detected, we mute the volume of the Spotify process, once the advert(s) are finished, we un-mute.
