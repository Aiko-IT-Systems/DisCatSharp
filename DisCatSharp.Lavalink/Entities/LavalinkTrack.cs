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
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink track.
/// </summary>
public sealed class LavalinkTrack
{
	/// <summary>
	/// Gets the encoded track.
	/// </summary>
	[JsonProperty("encoded")]
	public string Encoded { get; internal set; }

	/// <summary>
	/// Gets the lavalink track info.
	/// </summary>
	[JsonProperty("info")]
	public LavalinkTrackInfo Info { get; internal set; }

	// This can be dynamic
	/// <summary>
	/// Gets the lavalink plugin info.
	/// </summary>
	[JsonProperty("pluginInfo")]
	public LavalinkPluginInfo PluginInfo { get; internal set; }
}

/// <summary>
/// Represents lavalink plugin information.
/// </summary>
public sealed class LavalinkPluginInfo
{
	/// <summary>
	/// Gets additional json properties that are not known to the deserializing object.
	/// </summary>
	[JsonIgnore]
	internal IDictionary<string, object> UnknownProperties = new Dictionary<string, object>();

	/// <summary>
	/// Lets JsonConvert set the unknown properties.
	/// </summary>
	[JsonExtensionData(ReadData = true, WriteData = false)]
	public IDictionary<string, object> AdditionalProperties
	{
		get => this.UnknownProperties;
		set => this.UnknownProperties = value;
	}
}

/// <summary>
/// Represents track information.
/// </summary>
public sealed class LavalinkTrackInfo
{
	/// <summary>
	/// Gets the track identifier.
	/// </summary>
	[JsonProperty("identifier")]
	public string Identifier { get; internal set; }

	/// <summary>
	/// Gets whether the track is seekable.
	/// </summary>
	[JsonProperty("isSeekable")]
	public bool IsSeekable { get; internal set; }

	/// <summary>
	/// Gets the track author.
	/// </summary>
	[JsonProperty("author")]
	public string Author { get; internal set; }

	/// <summary>
	/// Gets the track length.
	/// </summary>
	[JsonIgnore]
	public TimeSpan Length => !this.IsStream ? TimeSpan.FromMilliseconds(this.LengthInternal) : TimeSpan.Zero;
	[JsonProperty("length")]
	internal long LengthInternal;

	/// <summary>
	/// Gets whether the track is a stream.
	/// </summary>
	[JsonProperty("isStream")]
	public bool IsStream { get; internal set; }

	/// <summary>
	/// Gets the current position.
	/// </summary>
	[JsonIgnore]
	public TimeSpan Position => TimeSpan.FromMilliseconds(this.PositionInternal);
	[JsonProperty("position")]
	internal long PositionInternal;

	/// <summary>
	/// Gets the track title.
	/// </summary>
	[JsonProperty("title")]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the track url.
	/// </summary>
	[JsonProperty("uri")]
	public Uri Uri { get; internal set; }

	/// <summary>
	/// Gets the tracks artwork url.
	/// </summary>
	[JsonProperty("artworkUrl", NullValueHandling = NullValueHandling.Include)]
	public Uri? ArtworkUrl { get; internal set; }

	/// <summary>
	/// Gets the tracks isrc.
	/// </summary>
	[JsonProperty("isrc")]
	public string? Isrc { get; internal set; }

	/// <summary>
	/// Gets the tracks source name.
	/// </summary>
	[JsonProperty("sourceName")]
	public string SourceName { get; internal set; }
}
