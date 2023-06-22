// This file is part of the DisCatSharp project.
//
// Copyright (c) 2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
