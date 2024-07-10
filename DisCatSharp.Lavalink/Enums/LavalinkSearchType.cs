namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// The lavalink search type.
/// </summary>
public enum LavalinkSearchType
{
	/// <summary>
	/// Search on SoundCloud
	/// </summary>
	SoundCloud,

	/// <summary>
	/// Search on Youtube.
	/// </summary>
	Youtube,

	/// <summary>
	/// Provide Lavalink with a plain URL.
	/// </summary>
	Plain,

	/*
	// This does not support search. Keeping it for reference tho.
	/// <summary>
	/// Search on Band Camp.
	/// </summary>
	BandCamp,

	/// <summary>
	/// Search on Twitch.
	/// </summary>
	Twitch,

	/// <summary>
	/// Search on Vimeo.
	/// </summary>
	Vimeo,*/

	// Requires: https://github.com/topiSenpai/LavaSrc

	/// <summary>
	/// Search on Apple Music.
	/// </summary>
	AppleMusic,

	/// <summary>
	/// Search on Deezer.
	/// </summary>
	Deezer,

	/// <summary>
	/// Search on Deezer with ISRC.
	/// </summary>
	DeezerISrc,

	/// <summary>
	/// Search on Yandex Music.
	/// </summary>
	YandexMusic,

	/// <summary>
	/// Search on Spotify.
	/// </summary>
	Spotify,

	/// <summary>
	/// Search on Spotify with recommendation seed.
	/// </summary>
	SpotifyRec
}
