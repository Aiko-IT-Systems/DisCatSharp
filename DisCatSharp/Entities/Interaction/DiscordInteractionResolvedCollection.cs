using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a collection of Discord snowflake objects resolved from interaction arguments.
/// </summary>
public sealed class DiscordInteractionResolvedCollection : ObservableApiObject
{
	/// <summary>
	/// Gets the resolved user objects, if any.
	/// </summary>
	[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyDictionary<ulong, DiscordUser> Users { get; internal set; }

	/// <summary>
	/// Gets the resolved member objects, if any.
	/// </summary>
	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyDictionary<ulong, DiscordMember> Members { get; internal set; }

	/// <summary>
	/// Gets the resolved channel objects, if any.
	/// </summary>
	[JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyDictionary<ulong, DiscordChannel> Channels { get; internal set; }

	/// <summary>
	/// Gets the resolved role objects, if any.
	/// </summary>
	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyDictionary<ulong, DiscordRole> Roles { get; internal set; }

	/// <summary>
	/// Gets the resolved message objects, if any.
	/// <para>Not applicable for messages with resolved data for auto-populated select menus.</para>
	/// </summary>
	[JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyDictionary<ulong, DiscordMessage> Messages { get; internal set; }

	/// <summary>
	/// Gets the resolved attachments objects, if any.
	/// <para>Not applicable for messages with resolved data for auto-populated select menus.</para>
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyDictionary<ulong, DiscordAttachment> Attachments { get; internal set; }
}
