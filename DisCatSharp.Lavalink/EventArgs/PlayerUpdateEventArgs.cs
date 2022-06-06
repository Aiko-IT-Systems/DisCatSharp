// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents arguments for player update event.
/// </summary>
public sealed class PlayerUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the timestamp at which this event was emitted.
	/// </summary>
	public DateTimeOffset Timestamp { get; }

	/// <summary>
	/// Gets the position in the playback stream.
	/// </summary>
	public TimeSpan Position { get; }

	/// <summary>
	/// Gets the player that emitted this event.
	/// </summary>
	public LavalinkGuildConnection Player { get; }

	/// <summary>
	/// Gets whether the player is connected to the voice gateway.
	/// </summary>
	public bool IsConnected { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PlayerUpdateEventArgs"/> class.
	/// </summary>
	/// <param name="lvl">The lvl.</param>
	/// <param name="timestamp">The timestamp.</param>
	/// <param name="position">The position.</param>
	/// <param name="connected">Whether the player is connected.</param>
	internal PlayerUpdateEventArgs(LavalinkGuildConnection lvl, DateTimeOffset timestamp, TimeSpan position, bool connected)
		: base(lvl.Node.Discord.ServiceProvider)
	{
		this.Player = lvl;
		this.Timestamp = timestamp;
		this.Position = position;
		this.IsConnected = connected;
	}
}
