using System;
using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a primary guild.
/// </summary>
public sealed class DiscordPrimaryGuild
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPrimaryGuild" /> class.
	/// </summary>
	internal DiscordPrimaryGuild()
	{ }

	/// <summary>
	///     Gets the identity guild id.
	/// </summary>
	[JsonProperty("identity_guild_id")]
	public ulong? IdentityGuildId { get; internal set; }

	/// <summary>
	///     Gets whether the identity is enabled and shown to everyone.
	/// </summary>
	[JsonProperty("identity_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool IdentityEnabled { get; internal set; }

	/// <summary>
	///     Gets the primary guild's tag.
	/// </summary>
	[JsonProperty("tag")]
	public string Tag { get; internal set; }

	/// <summary>
	///     Gets the cprimary guild's badge url
	/// </summary>
	[JsonIgnore]
	public string? BadgeUrl
		=> string.IsNullOrWhiteSpace(this.BadgeHash) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.CLAN_BADGES}/{this.IdentityGuildId?.ToString(CultureInfo.InvariantCulture)}/{this.BadgeHash}.{(this.BadgeHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	/// <summary>
	///     Gets the primary guild's badge hash.
	/// </summary>
	[JsonProperty("badge", NullValueHandling = NullValueHandling.Ignore)]
	public string BadgeHash { get; internal set; }
}
