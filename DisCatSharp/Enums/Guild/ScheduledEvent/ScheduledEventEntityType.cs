namespace DisCatSharp.Enums;

/// <summary>
/// Represents the entity type for a scheduled event.
/// </summary>
public enum ScheduledEventEntityType
{
	/// <summary>
	/// Indicates that the events is hold in a stage instance.
	/// </summary>
	StageInstance = 1,

	/// <summary>
	/// Indicates that the events is hold in a voice channel.
	/// </summary>
	Voice = 2,

	/// <summary>
	/// Indicates that the events is hold external.
	/// </summary>
	External = 3
}
