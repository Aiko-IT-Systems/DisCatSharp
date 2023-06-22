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

using System.Threading.Tasks;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents an interface for using the built-in DisCatSharp Lavalink queue.
/// </summary>
public interface IQueueEntry
{
	/// <summary>
	/// The lavalink track to play.
	/// </summary>
	LavalinkTrack Track { get; internal set; }

	/// <summary>
	/// Adds a track.
	/// </summary>
	/// <param name="track">The track to add.</param>
	/// <returns>The queue entry.</returns>
	public IQueueEntry AddTrack(LavalinkTrack track)
	{
		this.Track = track;
		return this;
	}

	/// <summary>
	/// Actions to execute before this queue entry gets played.
	/// Return <see langword="false"/> if entry shouldn't be played.
	/// </summary>
	abstract Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player);

	/// <summary>
	/// Actions to execute after this queue entry was played.
	/// </summary>
	abstract Task AfterPlayingAsync(LavalinkGuildPlayer player);
}
