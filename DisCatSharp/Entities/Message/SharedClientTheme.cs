using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the shared client theme for a message.
/// </summary>
public class SharedClientTheme
{
	/// <summary>
	///     Gets or sets the colors for the theme.
	/// </summary>
	[JsonProperty("colors")]
	public List<string> Colors { get; set; } = [];

	/// <summary>
	///     Gets or sets the gradient angle (direction in degrees).
	/// </summary>
	[JsonProperty("gradient_angle")]
	public int GradientAngle { get; set; }

	/// <summary>
	///     Gets or sets the color intensity percentage (base mix). <c>0</c> is no mix, <c>100</c> is full mix.
	/// </summary>
	[JsonProperty("base_mix")]
	public int BaseMix { get; set; }

	/// <summary>
	///     Gets or sets the appearance.
	/// </summary>
	[JsonProperty("base_theme")]
	public BaseTheme BaseTheme { get; set; }
}
