using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a selected sku option.
/// </summary>
public sealed class DiscordSkuSelectedOption
{
	/// <summary>
	///     Gets the option name.
	/// </summary>
	[JsonProperty("option_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? OptionName { get; internal set; }

	/// <summary>
	///     Gets the option value.
	/// </summary>
	[JsonProperty("option_value", NullValueHandling = NullValueHandling.Ignore)]
	public string? OptionValue { get; internal set; }
}
