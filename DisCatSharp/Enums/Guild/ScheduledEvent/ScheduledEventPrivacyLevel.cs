
namespace DisCatSharp.Enums;

/// <summary>
/// Represents the privacy level for a guild scheduled event.
/// </summary>
public enum ScheduledEventPrivacyLevel : int
{
	/// <summary>
	/// Indicates that the guild scheduled event is public.
	/// </summary>
	Public = 1,

	/// <summary>
	/// Indicates that the the guild scheduled event is only accessible to guild members.
	/// </summary>
	GuildOnly = 2
}
