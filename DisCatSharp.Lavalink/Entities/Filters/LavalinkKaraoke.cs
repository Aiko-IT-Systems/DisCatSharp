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
/// Uses equalization to eliminate part of a band, usually targeting vocals.
/// </summary>
public sealed class LavalinkKaraoke
{
	/// <summary>
	/// Gets or sets the level (<c>0.0</c> to <c>1.0</c> where <c>0.0</c> is no effect and <c>1.0</c> is full effect).
	/// </summary>
	[JsonProperty("level")]
	public Optional<float> Level { get; set; }

	/// <summary>
	/// Gets or sets the mono level (<c>0.0</c> to <c>1.0</c> where <c>0.0</c> is no effect and <c>1.0</c> is full effect).
	/// </summary>
	[JsonProperty("monoLevel")]
	public Optional<float> MonoLevel { get; set; }

	/// <summary>
	/// Gets or sets the filter band in Hz.
	/// </summary>
	[JsonProperty("filterBand")]
	public Optional<float> FilterBand { get; set; }

	/// <summary>
	/// Gets or sets the filter width.
	/// </summary>
	[JsonProperty("filterWidth")]
	public Optional<float> FilterWidth { get; set; }

	/// <inheritdoc cref="LavalinkKaraoke"/>
	/// <param name="level">The level</param>
	/// <param name="monoLevel">The mono level</param>
	/// <param name="filterBand">The filter band in Hz.</param>
	/// <param name="filterWidth">The filter width.</param>
	public LavalinkKaraoke(Optional<float> level, Optional<float> monoLevel, Optional<float> filterBand, Optional<float> filterWidth)
	{
		this.Level = level;
		this.MonoLevel = monoLevel;
		this.FilterBand = filterBand;
		this.FilterWidth = filterWidth;
	}
}
