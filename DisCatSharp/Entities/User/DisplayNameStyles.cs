using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///		Represents the display name styles for a user.
/// </summary>
public class DisplayNameStyles
{
	/// <summary>
	///		Represents the font style for the display name.
	/// </summary>
	[JsonProperty("font_id")]
	public DisplayNameFont FontId { get; set; }

	/// <summary>
	///		Represents the effect style for the display name.
	/// </summary>
	[JsonProperty("effect_id")]
	public DisplayNameEffect EffectId { get; set; }

	/// <summary>
	///		Represents the raw color styles for the display name.
	/// </summary>
	[JsonProperty("colors")]
	public IReadOnlyList<int> ColorsInternal { get; set; }

	/// <summary>
	///		Represents the color styles for the display name.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordColor> Colors
		=> [.. this.ColorsInternal.Select(c => new DiscordColor(c))];

}
