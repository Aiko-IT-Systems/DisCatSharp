using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a rule's content type.
/// </summary>
public enum AutomodTriggerType
{
	/// <summary>
	/// Checks if content contains words from a user defined list of keywords.
	/// Max. 3 per guild.
	/// </summary>
	Keyword = 1,

	/// <summary>
	/// Checks if content contains a suspocopis link.
	/// </summary>
	[DiscordDeprecated]
	SuspiciousLinkFilter = 2,

	/// <summary>
	/// Checks if content represents generic spam.
	/// Max. 1 per guild.
	/// </summary>
	Spam = 3,

	/// <summary>
	/// Checks if content contains words from internal pre-defined wordsets.
	/// Max. 1 per guild.
	/// </summary>
	KeywordPreset = 4,

	/// <summary>
	/// Checks if content contains more unique mentions than allowed.
	/// Max. 1 per guild.
	/// </summary>
	MentionSpam = 5,

	/// <summary>
	/// Flag messages that may break server rules using OpenAI technology.
	/// </summary>
	[DiscordInExperiment, DiscordUnreleased]
	EnforceServerRules = 7
}
