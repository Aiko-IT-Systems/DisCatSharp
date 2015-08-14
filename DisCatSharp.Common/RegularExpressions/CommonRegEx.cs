using System.Text.RegularExpressions;

namespace DisCatSharp.Common.RegularExpressions;

/// <summary>
/// Provides common regex.
/// </summary>
public static partial class CommonRegEx
{
	[GeneratedRegex("^#?([a-fA-F0-9]{6})$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex HexColorStringRegex();

	[GeneratedRegex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex RgbColorStringRegex();

	[GeneratedRegex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex TimeSpanRegex();

	[GeneratedRegex(@"http(s)?:\/\/(www\.)?youtu(\.be|be\.com)\/(watch\?v=|playlist)?(?<id>\w{1,})?((\?|\&)list=(?<list>[\w-_]{1,}))(&index=(?<index>\d{1,}))?", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex AdvancedYoutubeRegex();

	[GeneratedRegex(@"([`\*_~\[\]\(\)""\|]|<@\!?\d+>|<#\d+>|<@\&\d+>|<:[a-zA-Z0-9_\-]:\d+>|#{1,3} |> |>>> |\* )", RegexOptions.ECMAScript | RegexOptions.Compiled)]
	public static partial Regex MdStripRegex();

	[GeneratedRegex("([`\\*_~<>\\[\\]\\(\\)\"@\\!\\&#:\\|])", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex MdSanitizeRegex();

	[GeneratedRegex(":([a-z_]+)", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex HttpRouteRegex();
}
