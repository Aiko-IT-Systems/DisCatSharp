using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents the Social SDK channel context embedded in webhook message payloads.
/// </summary>
public sealed class DiscordWebhookSocialChannel : ObservableApiObject
{
	/// <summary>
	///     Gets the channel identifier.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; internal set; }

	/// <summary>
	///     Gets the channel type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ChannelType? Type { get; internal set; }

	/// <summary>
	///     Gets the channel name when Discord provided it.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	/// <summary>
	///     Gets the channel recipients when Discord provided them.
	/// </summary>
	[JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordUser> Recipients { get; internal set; } = [];
}
