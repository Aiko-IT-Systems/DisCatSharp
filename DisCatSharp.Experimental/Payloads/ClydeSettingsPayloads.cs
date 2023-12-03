using System.Collections.Generic;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Payloads;

/// <summary>
/// Represents a clyde settings update payload with a profile id.
/// </summary>
[DiscordDeprecated]
internal sealed class ClydeSettingsProfileIdOnlyUpdatePayload : ObservableApiObject
{
	/// <summary>
	/// Sets clyde's profile id.
	/// </summary>
	[JsonProperty("clyde_profile_id")]
	internal ulong ClydeProfileId { get; set; }

	/// <summary>
	/// Sets clyde's personality preset.
	/// </summary>
	[JsonProperty("personality_preset")]
	internal string PersonalityPreset { get; } = "custom";
}

/// <summary>
/// Represents a clyde settings update payload.
/// </summary>
[DiscordDeprecated]
internal sealed class ClydeSettingsProfileUpdatePayload : ObservableApiObject
{
	/// <summary>
	/// Sets clyde's avatar.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
	internal Optional<string?> Avatar { get; set; }

	/// <summary>
	/// Sets clyde's banner.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Include)]
	internal Optional<string?> Banner { get; set; }

	/// <summary>
	/// Sets clyde's nick.
	/// </summary>
	[JsonProperty("nick", NullValueHandling = NullValueHandling.Include)]
	internal Optional<string?> Nick { get; set; }

	/// <summary>
	/// Sets clyde's personality.
	/// </summary>
	[JsonProperty("personality", NullValueHandling = NullValueHandling.Ignore)]
	internal Optional<string> Personality { get; set; }

	/// <summary>
	/// Sets clyde's personality preset.
	/// </summary>
	[JsonProperty("personality_preset", NullValueHandling = NullValueHandling.Ignore)]
	internal string PersonalityPreset { get; } = "custom";

	/// <summary>
	/// Sets clyde's theme colors.
	/// </summary>
	[JsonProperty("theme_colors", NullValueHandling = NullValueHandling.Include)]
	internal Optional<List<int>?> ThemeColors { get; set; }
}

/// <summary>
/// Represents a personality generation payload.
/// </summary>
[DiscordDeprecated]
internal sealed class PersonalityGenerationPayload : ObservableApiObject
{
	/// <summary>
	/// Sets the base personality to generate a new one from.
	/// </summary>
	[JsonProperty("personality", NullValueHandling = NullValueHandling.Ignore)]
	internal string Personality { get; set; } = string.Empty;
}
