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

using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Represents Lavalink track loading results.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LavalinkLoadResultType
{
	/// <summary>
	/// Specifies that track was loaded successfully.
	/// </summary>
	[EnumMember(Value = "track")]
	Track,

	/// <summary>
	/// Specifies that playlist was loaded successfully.
	/// </summary>
	[EnumMember(Value = "playlist")]
	Playlist,

	/// <summary>
	/// Specifies that the result set contains search results.
	/// </summary>
	[EnumMember(Value = "search")]
	Search,

	/// <summary>
	/// Specifies that the search yielded no results.
	/// </summary>
	[EnumMember(Value = "empty")]
	Empty,

	/// <summary>
	/// Specifies that the track failed to load.
	/// </summary>
	[EnumMember(Value = "error")]
	Error
}
