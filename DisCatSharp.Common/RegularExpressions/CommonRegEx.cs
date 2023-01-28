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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Text.RegularExpressions;

namespace DisCatSharp.Common.RegularExpressions;

/// <summary>
/// Provides common regex.
/// </summary>
public static class CommonRegEx
{
	/// <summary>
	/// Represents a hex color string.
	/// </summary>
	public static Regex HexColorString
		=> new(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a rgb color string.
	/// </summary>
	public static Regex RgbColorString
		=> new(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a timespan.
	/// </summary>
	public static Regex TimeSpan
		=> new(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a advanced youtube regex.
	/// Named groups: 
	/// <list type="table">
	///   <listheader>
	///      <term>group</term>
	///      <description>description</description>
	///   </listheader>
	///   <item>
	///      <term>id</term>
	///      <description>Video ID</description>
	///   </item>
	///   <item>
	///      <term>list</term>
	///      <description>List ID</description>
	///   </item>
	///   <item>
	///      <term>index</term>
	///      <description>List index</description>
	///   </item>
	/// </list>
	/// </summary>
	public static Regex AdvancedYoutubeRegex
		=> new(@"http(s)?:\/\/(www\.)?youtu(\.be|be\.com)\/(watch\?v=|playlist)?(?<id>\w{1,})?((\?|\&)list=(?<list>\w{1,}))(&index=(?<index>\d{1,}))?", RegexOptions.ECMAScript | RegexOptions.Compiled);
}
