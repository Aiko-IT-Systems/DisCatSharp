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

using DisCatSharp.Lavalink.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Represents reasons why a <see cref="LavalinkTrack"/> ended.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LavalinkTrackEndReason
{
	/// <summary>
	/// The track was finished.
	/// </summary>
	[EnumMember(Value = "finished")]
	Finished,

	/// <summary>
	/// The track was failed to load.
	/// </summary>
	[EnumMember(Value = "loadFailed")]
	LoadFailed,

	/// <summary>
	/// The track was stopped.
	/// </summary>
	[EnumMember(Value = "stopped")]
	Stopped,

	/// <summary>
	/// The track was replaced.
	/// </summary>
	[EnumMember(Value = "replaced")]
	Replaced,

	/// <summary>
	/// The track was cleaned up.
	/// </summary>
	[EnumMember(Value = "cleanup")]
	Cleanup
}
