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

using DisCatSharp.Lavalink.Enums.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// There are 15 bands (0-14) that can be changed. "gain" is the multiplier for the given band. The default value is 0. Valid values range from -0.25 to 1.0, where -0.25 means the given band is completely muted, and 0.25 means it is doubled. Modifying the gain could also change the volume of the output.
/// </summary>
public sealed class LavalinkEqualizer
{
	/// <summary>
	/// The band.
	/// </summary>
	[JsonProperty("band")]
	public LavalinkFilterBand Band { get; set; }

	/// <summary>
	/// The gain (<c>-0.25</c> to <c>1.0</c>)
	/// </summary>
	[JsonProperty("band")]
	public float Gain { get; set; }

	/// <inheritdoc cref="LavalinkEqualizer"/>
	/// <param name="band">The band</param>
	/// <param name="gain">The gain (<c>-0.25</c> to <c>1.0</c>)</param>
	public LavalinkEqualizer(LavalinkFilterBand band, float gain)
	{
		this.Band = band;
		this.Gain = gain;
	}
}
