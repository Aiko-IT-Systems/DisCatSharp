using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a localized sku name payload.
/// </summary>
public sealed class DiscordSkuName
{
	/// <summary>
	///     Gets the default sku name.
	/// </summary>
	[JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
	public string? Default { get; internal set; }

	/// <summary>
	///     Gets the localized sku names.
	/// </summary>
	[JsonProperty("localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> Localizations { get; internal set; } = [];
}
