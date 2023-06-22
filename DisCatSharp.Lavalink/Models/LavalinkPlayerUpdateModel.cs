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
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Entities.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Models;

/// <summary>
/// Represents an easy-to-use model to update a <see cref="LavalinkPlayer"/>.
/// </summary>
public sealed class LavalinkPlayerUpdateModel
{
	/// <summary>
	/// Sets whether to replace the current track.
	/// </summary>
	[JsonIgnore]
	public bool Replace { internal get; set; }

	/// <summary>
	/// Sets the encoded track.
	/// </summary>
	[JsonProperty("encodedTrack")]
	public Optional<string?> EncodedTrack { internal get; set; }

	/// <summary>
	/// Sets the identifier.
	/// </summary>
	[JsonProperty("identifier")]
	public Optional<string> Identifier { internal get; set; }

	/// <summary>
	/// Sets the start or seek position.
	/// </summary>
	[JsonProperty("position")]
	public Optional<int> Position { internal get; set; }

	/// <summary>
	/// Sets the end time.
	/// </summary>
	[JsonProperty("endTime")]
	public Optional<int?> EndTime { internal get; set; }

	/// <summary>
	/// Sets the volume.
	/// </summary>
	[JsonProperty("volume")]
	public Optional<int> Volume { internal get; set; }

	/// <summary>
	/// Sets whether the player is paused.
	/// </summary>
	[JsonProperty("paused")]
	public Optional<bool> Paused { internal get; set; }

	/// <summary>
	/// Gets or sets the filters.
	/// </summary>
	[JsonProperty("filters")]
	public Optional<LavalinkFilters> Filters { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkPlayerUpdateModel"/> class.
	/// </summary>
	internal LavalinkPlayerUpdateModel()
	{ }
}
