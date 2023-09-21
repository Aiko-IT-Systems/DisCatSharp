using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an interactions application command callback data.
/// </summary>
internal class DiscordInteractionApplicationCommandCallbackData : ObservableApiObject
{
	/// <summary>
	/// Whether this message is text to speech.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsTts { get; internal set; }

	/// <summary>
	/// Gets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; internal set; }

	/// <summary>
	/// Gets the embeds.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordEmbed> Embeds { get; internal set; }

	/// <summary>
	/// Gets the mentions.
	/// </summary>
	[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<IMention> Mentions { get; internal set; }

	/// <summary>
	/// Gets the flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	/// <summary>
	/// Gets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyCollection<DiscordActionRowComponent> Components { get; internal set; }

	/// <summary>
	/// Gets the autocomplete choices.
	/// </summary>
	[JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyCollection<DiscordApplicationCommandAutocompleteChoice> Choices { get; internal set; }

	/// <summary>
	/// Gets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAttachment> Attachments { get; set; }
}

/// <summary>
/// Represents an interactions application command callback data for modals.
/// </summary>
internal class DiscordInteractionApplicationCommandModalCallbackData
{
	/// <summary>
	/// Gets the custom id.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string CustomId { get; internal set; }

	/// <summary>
	/// Gets the content.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyCollection<DiscordComponent> ModalComponents { get; internal set; }
}

/// <summary>
/// Represents an interactions application command callback data for iFrames.
/// </summary>
internal class DiscordInteractionApplicationCommandIframeCallbackData
{
	/// <summary>
	/// Gets the custom id.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string CustomId { get; internal set; }

	/// <summary>
	/// Gets the content.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the iFrame modal size.
	/// </summary>
	[JsonProperty("modal_size", NullValueHandling = NullValueHandling.Ignore)]
	public IframeModalSize ModalSize { get; internal set; }

	/// <summary>
	/// Gets the iFrame path.
	/// </summary>
	[JsonProperty("iframe_path", NullValueHandling = NullValueHandling.Include)]
	public string? IframePath { get; internal set; }
}
