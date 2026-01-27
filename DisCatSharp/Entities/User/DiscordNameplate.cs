using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a user's nameplate.
/// </summary>
public sealed class DiscordNameplate
{
	/// <summary>
	///     Gets the user's nameplate url.
	/// </summary>
	[JsonIgnore]
	public string AssetUrl
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ASSETS}{Endpoints.COLLECTIBLES}/{this.Asset}asset.webm";

	/// <summary>
	///     Gets the nameplate's asset hash.
	/// </summary>
	/// <remarks>
	/// 	This property will return something like <c>nameplates/tarot/towers_strike/</c>.
	/// </remarks>
	[JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
	public string Asset { get; internal set; }

	/// <summary>
	///     Gets the nameplate's label.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	/// <summary>
	///     Gets the nameplate's sku id.
	/// </summary>
	[JsonProperty("sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong SkuId { get; internal set; }

	/// <summary>
	///     Gets the nameplate's dominant color.
	/// </summary>
	[JsonProperty("palette", NullValueHandling = NullValueHandling.Ignore)]
	public string Palette { get; internal set; }
}
