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
		=> new(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$",
		       RegexOptions.ECMAScript | RegexOptions.Compiled);

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
		=> new(@"http(s)?:\/\/(www\.)?youtu(\.be|be\.com)\/(watch\?v=|playlist)?(?<id>\w{1,})?((\?|\&)list=(?<list>[\w-_]{1,}))(&index=(?<index>\d{1,}))?",
		       RegexOptions.ECMAScript | RegexOptions.Compiled);
}
