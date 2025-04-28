using System;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a user's avatar decoration data.
/// </summary>
public sealed class AvatarDecorationData
{
	/// <summary>
	///     Gets the decoration's asset hash.
	/// </summary>
	[JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
	public string Asset { get; internal set; }

	/// <summary>
	///     Gets the user's avatar decoration url.
	/// </summary>
	[JsonIgnore]
	public string AssetUrl
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS_DECORATION_PRESETS}/{this.Asset}.png?size=1024";

	/// <summary>
	///     Gets the decoration's sku id.
	/// </summary>
	[JsonProperty("sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong SkuId { get; internal set; }

	/// <summary>
	///     Gets whether and when the decoration expires.
	/// </summary>
	[JsonProperty("expires_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? ExpiresAt { get; internal set; }
}
