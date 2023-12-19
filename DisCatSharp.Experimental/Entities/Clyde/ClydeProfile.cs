using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a clyde profile.
/// </summary>
[DiscordDeprecated]
public sealed class ClydeProfile : ObservableApiObject
{
	/// <summary>
	/// Gets clyde's profile name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	/// <summary>
	/// Gets clyde's profile personality.
	/// </summary>
	[JsonProperty("personality", NullValueHandling = NullValueHandling.Ignore)]
	public string Personality { get; internal set; }

	/// <summary>
	/// Gets clyde's profile avatar hash.
	/// </summary>
	[JsonProperty("avatar_hash", NullValueHandling = NullValueHandling.Ignore)]
	public string? AvatarHash { get; internal set; }

	/// <summary>
	/// Gets clyde's profile banner hash.
	/// </summary>
	[JsonProperty("banner_hash", NullValueHandling = NullValueHandling.Ignore)]
	public string? BannerHash { get; internal set; }

	/// <summary>
	/// Gets clyde's profile bio.
	/// </summary>
	[JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
	public string Bio { get; internal set; }

	/// <summary>
	/// Gets the user who authored this clyde profile.
	/// </summary>
	[JsonProperty("author_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong AuthorId { get; internal set; }

	/// <summary>
	/// Gets clyde's profile theme color ints.
	/// </summary>
	[JsonProperty("theme_colors", NullValueHandling = NullValueHandling.Ignore)]
	internal List<int>? ThemeColorsInternal { get; set; }

	/// <summary>
	/// Gets the clyde's profile theme colors, if set.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordColor>? ThemeColors
		=> !(this.ThemeColorsInternal is not null && this.ThemeColorsInternal.Count != 0)
			? null
			: this.ThemeColorsInternal.Select(x => new DiscordColor(x)).ToList();

	/// <summary>
	/// Gets clyde's profile id.
	/// </summary>
	[JsonProperty("clyde_profile_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ClydeProfileId { get; internal set; }
}
