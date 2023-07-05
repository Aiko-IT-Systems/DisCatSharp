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

using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Represents various lavalink filters.
/// </summary>
public sealed class LavalinkFilters
{
	/// <summary>
	/// Gets or sets the player volume from <c>0.0</c> to <c>5.0</c>, where <c>1.0</c> is 100%. Values ><c>1.0</c> may cause clipping.
	/// </summary>
	[JsonProperty("volume")]
	public Optional<float> Volume { get; set; }

	/// <summary>
	/// Gets or sets the equalizer.
	/// </summary>
	[JsonProperty("equalizer")]
	public Optional<List<LavalinkEqualizer>> Equalizers { get; set; }

	/// <summary>
	/// Gets or sets the karaoke.
	/// </summary>
	[JsonProperty("karaoke")]
	public Optional<LavalinkKaraoke> Karaoke { get; set; }

	/// <summary>
	/// Gets or sets the timescale.
	/// </summary>
	[JsonProperty("timescale")]
	public Optional<LavalinkTimescale> Timescale { get; set; }

	/// <summary>
	/// Gets or sets the tremolo.
	/// </summary>
	[JsonProperty("tremolo")]
	public Optional<LavalinkTremolo> Tremolo { get; set; }

	/// <summary>
	/// Gets or sets the vibrato.
	/// </summary>
	[JsonProperty("vibrato")]
	public Optional<LavalinkVibrato> Vibrato { get; set; }

	/// <summary>
	/// Gets or sets the rotation.
	/// </summary>
	[JsonProperty("rotation")]
	public Optional<LavalinkRotation> Rotation { get; set; }

	/// <summary>
	/// Gets or sets the disortion.
	/// </summary>
	[JsonProperty("disortion")]
	public Optional<LavalinkDisortion> Disortion { get; set; }

	/// <summary>
	/// Gets or sets the channel-mix.
	/// </summary>
	[JsonProperty("channelMix")]
	public Optional<LavalinkChannelMix> ChannelMix { get; set; }

	/// <summary>
	/// Gets or sets the low-pass.
	/// </summary>
	[JsonProperty("lowPass")]
	public Optional<LavalinkLowPass> LowPass { get; set; }

	/// <summary>
	/// Gets a dictionary of custom plugin filters.
	/// <para><c>Key</c> is plugin name.</para>
	/// <para><c>Value</c> is plugin object.</para>
	/// </summary>
	[JsonProperty("pluginFilters")]
	public Optional<Dictionary<string, IPluginFilter>> PluginFilters { get; set; }
}
