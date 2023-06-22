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

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents lavalink server statistics received via websocket.
/// </summary>
internal sealed class StatsOp : LavalinkOp
{
	/// <summary>
	/// Gets the total count of players.
	/// </summary>
	[JsonProperty("players")]
	internal int Players { get; set; }

	/// <summary>
	/// Gets the count of playing (active) players.
	/// </summary>
	[JsonProperty("playingPlayers")]
	internal int PlayingPlayers { get; set; }

	/// <summary>
	/// Gets or sets the uptime.
	/// </summary>
	[JsonProperty("uptime")]
	internal readonly long UptimeInt;

	/// <summary>
	/// Gets or sets the memory stats.
	/// </summary>
	[JsonProperty("memory")]
	internal MemoryStats Memory { get; set; }

	/// <summary>
	/// Gets or sets the cpu stats.
	/// </summary>
	[JsonProperty("cpu")]
	internal CpuStats Cpu { get; set; }

	/// <summary>
	/// Gets or sets the frame stats.
	/// </summary>
	[JsonProperty("frameStats")]
	internal FrameStats Frames { get; set; }
}
