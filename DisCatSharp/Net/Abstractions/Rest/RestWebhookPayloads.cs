using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a webhook payload.
/// </summary>
internal sealed class RestWebhookPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the avatar base64.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
	public string AvatarBase64 { get; set; }

	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public ulong ChannelId { get; set; }

	/// <summary>
	/// Gets whether an avatar is set.
	/// </summary>
	[JsonProperty]
	public bool AvatarSet { get; set; }

	/// <summary>
	/// Gets whether the avatar should be serialized.
	/// </summary>
	public bool ShouldSerializeAvatarBase64()
		=> this.AvatarSet;
}

/// <summary>
/// Represents a webhook execute payload.
/// </summary>
internal sealed class RestWebhookExecutePayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; set; }

	/// <summary>
	/// Gets or sets the username.
	/// </summary>
	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public string Username { get; set; }

	/// <summary>
	/// Gets or sets the avatar url.
	/// </summary>
	[JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
	public string AvatarUrl { get; set; }

	/// <summary>
	/// Whether this message is tts.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsTts { get; set; }

	/// <summary>
	/// Gets or sets the embeds.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordEmbed> Embeds { get; set; }

	/// <summary>
	/// Gets or sets the mentions.
	/// </summary>
	[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMentions Mentions { get; set; }

	/// <summary>
	/// Gets or sets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordActionRowComponent> Components { get; set; }

	/// <summary>
	/// Gets or sets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAttachment> Attachments { get; set; }

	///<summary>
	/// Gets or sets the thread name.
	/// </summary>
	[JsonProperty("thread_name", NullValueHandling = NullValueHandling.Ignore)]
	public string ThreadName { get; set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags Flags { get; set; }

	[JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<ulong>? AppliedTags { get; set; }
}

/// <summary>
/// Represents a webhook message edit payload.
/// </summary>
internal sealed class RestWebhookMessageEditPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Content { get; set; }

	/// <summary>
	/// Gets or sets the embeds.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordEmbed> Embeds { get; set; }

	/// <summary>
	/// Gets or sets the mentions.
	/// </summary>
	[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<IMention> Mentions { get; set; }

	/// <summary>
	/// Gets or sets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordAttachment> Attachments { get; set; }

	/// <summary>
	/// Gets or sets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordActionRowComponent> Components { get; set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; set; }
}
