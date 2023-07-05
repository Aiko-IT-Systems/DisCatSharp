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
/// Represents the severity for exceptions.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum Severity
{
	/// <summary>
	/// The cause is known and expected, indicates that there is nothing wrong with the library itself.
	/// </summary>
	[EnumMember(Value = "common")]
	Common,

	/// <summary>
	/// <para>The cause might not be exactly known, but is possibly caused by outside factors.</para>
	/// <para>For example when an outside service responds in a format that we do not expect.</para>
	/// </summary>
	[EnumMember(Value = "suspicious")]
	Suspicious,

	/// <summary>
	/// <para>The probable cause is an issue with the library or there is no way to tell what the cause might be.</para>
	/// <para>This is the default level and other levels are used in cases where the thrower has more in-depth knowledge about the error.</para>
	/// </summary>
	[EnumMember(Value = "fault")]
	Fault
}
