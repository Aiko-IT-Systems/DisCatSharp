using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the privacy level for a stage.
/// </summary>
[DiscordDeprecated("Discord removed the feature for stage discovery. Option is defaulting to GuildOnly.")]
public enum StagePrivacyLevel : int
{
	/// <summary>
	/// Indicates that the stage is public visible.
	/// </summary>
	Public = 1,

	/// <summary>
	/// Indicates that the stage is only visible to guild members.
	/// </summary>
	GuildOnly = 2
}
