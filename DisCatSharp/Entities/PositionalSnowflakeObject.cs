// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an object in the Discord API.
/// </summary>
public abstract class PositionalSnowflakeObject : SnowflakeObject
{
	/// <summary>
	/// Gets the position
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; internal set; }

	/// <summary>
	/// High position value equals a lower position.
	/// </summary>
	[JsonIgnore]
	internal virtual bool HighIsLow { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PositionalSnowflakeObject"/> class.
	/// </summary>
	/// <param name="ignored">List of property names to ignore during JSON serialization.</param>
	internal PositionalSnowflakeObject(List<string>? ignored = null)
		: base(ignored)
	{ }

	/// <summary>
	/// Determines whether the left <see cref="PositionalSnowflakeObject"/> is higher positioned than the right <see cref="PositionalSnowflakeObject"/>.
	/// </summary>
	/// <param name="left">The first <see cref="PositionalSnowflakeObject"/>.</param>
	/// <param name="right">The second <see cref="PositionalSnowflakeObject"/>.</param>
	/// <returns><see langword="true"/> if the left one is higher positioned; otherwise, <see langword="false"/>.</returns>
	public static bool operator >(PositionalSnowflakeObject? left, PositionalSnowflakeObject? right)
		=> left is not null && right is not null &&
		   (left.HighIsLow ? left.Position < right.Position : left.Position > right.Position);

	/// <summary>
	/// Determines whether the left <see cref="PositionalSnowflakeObject"/> is lower positioned than the right <see cref="PositionalSnowflakeObject"/>.
	/// </summary>
	/// <param name="left">The first <see cref="PositionalSnowflakeObject"/>.</param>
	/// <param name="right">The second <see cref="PositionalSnowflakeObject"/>.</param>
	/// <returns><see langword="true"/> if the left one is lower positioned; otherwise, <see langword="false"/>.</returns>
	public static bool operator <(PositionalSnowflakeObject? left, PositionalSnowflakeObject? right)
		=> left is not null && right is not null &&
		   (left.HighIsLow ? left.Position > right.Position : left.Position < right.Position);

	/// <summary>
	/// Returns a <see langword="string"/> which represents the <see cref="PositionalSnowflakeObject"/>.
	/// </summary>
	/// <returns>A <see langword="string"/> which represents the current <see cref="PositionalSnowflakeObject"/>.</returns>
	public override string ToString()
		=> $"{this.GetType().Name} (ID: {this.Id}, Position: {this.Position})";
}
