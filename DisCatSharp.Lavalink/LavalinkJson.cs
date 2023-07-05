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

using System.Diagnostics;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink;

/// <summary>
/// Represents a custom json (de-)serializer.
/// </summary>
internal static class LavalinkJson
{
	/// <summary>
	/// The <see cref="JsonSerializerSettings"/> with an <see cref="Newtonsoft.Json.Serialization.IContractResolver"/> for <see cref="Optional"/> types.
	/// </summary>
	private static readonly JsonSerializerSettings s_setting = new()
	{
		ContractResolver = new OptionalJsonContractResolver()
	};

	/// <summary>
	/// Deserializes the JSON to the specified .NET type using <see cref="s_setting"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
	/// <param name="value">The object to deserialize.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeObject<T>(string value)
		=> JsonConvert.DeserializeObject<T>(value, s_setting);

	/// <summary>
	/// Serializes the specified object to a JSON string using formatting and <see cref="s_setting"/>.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted. Defaults to <see cref="Formatting.Indented"/>.</param>
	/// <returns>A JSON string representation of the object.</returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, Formatting formatting = Formatting.None)
		=> JsonConvert.SerializeObject(value, formatting, s_setting);
}
