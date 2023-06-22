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

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Changes the speed, pitch, and rate.
/// </summary>
public sealed class LavalinkTimescale
{
	/// <summary>
	/// The playback speed. (<c>>0.0</c> where <c>1.0</c> is default)
	/// </summary>
	[JsonProperty("speed")]
	public Optional<float> Speed { get; set; }

	/// <summary>
	/// The pitch. (<c>>0.0</c> where <c>1.0</c> is default)
	/// </summary>
	[JsonProperty("pitch")]
	public Optional<float> Pitch { get; set; }

	/// <summary>
	/// The rate. (<c>>0.0</c> where <c>1.0</c> is default)
	/// </summary>
	[JsonProperty("rate")]
	public Optional<float> Rate { get; set; }

	/// <inheritdoc cref="LavalinkTimescale"/>
	/// <param name="speed">The playback speed. (<c>>0.0</c> where <c>1.0</c> is default)</param>
	/// <param name="pitch">The pitch. (<c>>0.0</c> where <c>1.0</c> is default)</param>
	/// <param name="rate">The rate. (<c>>0.0</c> where <c>1.0</c> is default)</param>
	public LavalinkTimescale(Optional<float> speed, Optional<float> pitch, Optional<float> rate)
	{
		this.Speed = speed;
		this.Pitch = pitch;
		this.Rate = rate;
	}
}
