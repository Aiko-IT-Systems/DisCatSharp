// ReSharper disable InconsistentNaming

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a guild's content level.
/// </summary>
public enum NsfwLevel
{
	/// <summary>
	/// Indicates the guild has no special NSFW level.
	/// </summary>
	Default = 0,

	/// <summary>
	/// Indicates the guild has extremely suggestive or mature content that would only be suitable for users over 18
	/// </summary>
	Explicit = 1,

	/// <summary>
	/// Indicates the guild has no content that could be deemed NSFW. It is SFW.
	/// </summary>
	Safe = 2,

	/// <summary>
	/// Indicates the guild has mildly NSFW content that may not be suitable for users under 18.
	/// </summary>
	Age_Restricted = 3
}
