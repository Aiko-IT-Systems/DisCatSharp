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

using System;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink player state.
/// </summary>
public sealed class LavalinkPlayerState
{
	/// <summary>
	/// Gets the current datetime offset.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Time => Utilities.GetDateTimeOffsetFromMilliseconds(this._time);

	/// <summary>
	/// Gets the unix timestamp in milliseconds.
	/// </summary>
	[JsonProperty("time")]
	private readonly long _time;

	/// <summary>
	/// Gets the position of the track as <see cref="TimeSpan"/>.
	/// </summary>
	[JsonIgnore]
	public TimeSpan Position => TimeSpan.FromMilliseconds(this._position);

	/// <summary>
	/// Gets the position of the track in milliseconds.
	/// </summary>
	[JsonProperty("position")]
	private readonly long _position;

	/// <summary>
	/// Gets whether Lavalink is connected to the voice gateway.
	/// </summary>
	[JsonProperty("connected")]
	public bool IsConnected { get; internal set; }

	/// <summary>
	/// Gets the ping of the node to the Discord voice server in milliseconds (<c>-1</c> if not connected).
	/// </summary>
	[JsonProperty("ping")]
	public int Ping { get; internal set; }
}
