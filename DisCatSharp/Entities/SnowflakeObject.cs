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

using System;
using System.Collections.Generic;

using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an object in the Discord API.
/// </summary>
public abstract class SnowflakeObject : ObservableApiObject, IEquatable<SnowflakeObject>, ICloneable
{
	/// <summary>
	/// Gets the ID of this object.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; internal set; }

	/// <summary>
	/// Gets the date and time this object was created.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset CreationTimestamp
		=> this.Id.GetSnowflakeTime();

	/// <summary>
	/// Initializes a new instance of the <see cref="SnowflakeObject"/> class.
	/// </summary>
	/// <param name="ignored">List of property names to ignore during JSON serialization.</param>
	internal SnowflakeObject(List<string>? ignored = null)
		: base(ignored)
	{ }

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> this.Equals(obj as SnowflakeObject);

	/// <inheritdoc />
	public virtual bool Equals(SnowflakeObject? other)
		=> other is not null && this.GetHashCode() == other.GetHashCode();

	/// <inheritdoc />
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Determines whether two <see cref="SnowflakeObject"/> instances are equal.
	/// </summary>
	/// <param name="left">The first <see cref="SnowflakeObject"/>.</param>
	/// <param name="right">The second <see cref="SnowflakeObject"/>.</param>
	/// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
	public static bool operator ==(SnowflakeObject? left, SnowflakeObject? right)
		=> left is not null && left.Equals(right);

	/// <summary>
	/// Determines whether two <see cref="SnowflakeObject"/> instances are not equal.
	/// </summary>
	/// <param name="left">The first <see cref="SnowflakeObject"/>.</param>
	/// <param name="right">The second <see cref="SnowflakeObject"/>.</param>
	/// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
	public static bool operator !=(SnowflakeObject? left, SnowflakeObject? right)
		=> !(left == right);

	/// <summary>
	/// Determines whether a <see cref="SnowflakeObject"/> is null.
	/// </summary>
	/// <param name="target">The <see cref="SnowflakeObject"/>.</param>
	/// <returns>Returns whether the current <see cref="NullableSnowflakeObject"/> is null.</returns>
	public static bool operator !(SnowflakeObject? target)
		=> target is null;

	/// <summary>
	/// Returns a <see langword="string"/> which represents the <see cref="SnowflakeObject"/>.
	/// </summary>
	/// <returns>A <see langword="string"/> which represents the current <see cref="SnowflakeObject"/>.</returns>
	public override string ToString()
		=> $"{this.GetType().Name} (ID: {this.Id})";

	/// <inheritdoc />
	public object Clone()
		=> this.MemberwiseClone();

	/// <summary>
	/// Serializes the <see cref="SnowflakeObject"/> to a JSON string.
	/// </summary>
	/// <returns>A JSON string representation of the object.</returns>
	public string ToJson()
		=> DiscordJson.SerializeObject(this);

	/// <summary>
	/// Deserializes a JSON string to a <typeparamref name="T"/> instance of <see cref="SnowflakeObject"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize.</typeparam>
	/// <param name="json">The JSON string to deserialize from.</param>
	/// <param name="discord">The Discord client instance associated with the deserialization.</param>
	/// <returns>An instance of type <typeparamref name="T"/>.</returns>
	public static T FromJson<T>(string json, BaseDiscordClient? discord = null) where T : SnowflakeObject
		=> DiscordJson.DeserializeObject<T>(json, discord);
}
