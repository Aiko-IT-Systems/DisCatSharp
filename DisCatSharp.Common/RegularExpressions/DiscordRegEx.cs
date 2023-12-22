using System.Text.RegularExpressions;

namespace DisCatSharp.Common.RegularExpressions;

/// <summary>
/// Provides common regex for discord related things.
/// </summary>
public static partial class DiscordRegEx
{
	[GeneratedRegex(@"^((https?:\/\/)?(www\.)?discord\.gg(\/.*)*|(https?:\/\/)?(www\.|canary\.|ptb\.)?(discord|discordapp)\.com\/invite)\/(?<code>[a-zA-Z0-9]*)(\?event=(?<event>\d+))?$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex InviteRegex();

	[GeneratedRegex(@"^(https?:\/\/)?(www\.|canary\.|ptb\.)?(discord|discordapp)\.com\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex MessageLinkRegex();

	[GeneratedRegex("^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex EmojiRegex();

	[GeneratedRegex(@"^<(?<animated>a):(?<name>\w{2,32}):(?<id>\d{17,20})>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex AnimatedEmojiRegex();

	[GeneratedRegex(@"^<:(?<name>\w{2,32}):(?<id>\d{17,20})>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex StaticEmojiRegex();

	[GeneratedRegex("^<t:(?<timestamp>-?\\d{1,13})(:(?<style>[tTdDfFR]))?>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex TimestampRegex();

	[GeneratedRegex("^<t:(?<timestamp>-?\\d{1,13})$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex DefaultStyledTimestampRegex();

	[GeneratedRegex("^<t:(?<timestamp>-?\\d{1,13}):(?<style>[tTdDfFR])>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex StyledTimestampRegex();

	[GeneratedRegex("^<@&(\\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex RoleRegex();

	[GeneratedRegex("^<#(\\d+)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex ChannelRegex();

	[GeneratedRegex(@"^<@\!?(\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex UserRegex();

	[GeneratedRegex(@"^<@\!(?<id>\d{17,20})>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex UserWithNicknameRegex();

	[GeneratedRegex(@"^<@\!?(?<id>\d{17,20})>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex UserWithOptionalNicknameRegex();

	[GeneratedRegex("^<@(?<id>\\d{17,20})>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex UserWithoutNicknameRegex();

	[GeneratedRegex(@"^(https?:\/\/)?(www\.|canary\.|ptb\.)?(discord|discordapp)\.com\/events\/(?<guild>\d+)\/(?<event>\d+)$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex EventRegex();
}
