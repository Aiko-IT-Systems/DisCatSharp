using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a application command create payload.
/// </summary>
internal sealed class RestApplicationCommandCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets the type.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandType Type { get; set; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the name localizations.
	/// </summary>
	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<Dictionary<string, string>> NameLocalizations { get; set; }

	/// <summary>
	/// Gets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; set; }

	/// <summary>
	/// Gets the description localizations.
	/// </summary>
	[JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<Dictionary<string, string>> DescriptionLocalizations { get; set; }

	/// <summary>
	/// Gets the options.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordApplicationCommandOption> Options { get; set; }

	/// <summary>
	/// Whether the command is allowed for everyone.
	/// </summary>
	[JsonProperty("default_permission", NullValueHandling = NullValueHandling.Include)]
	public bool? DefaultPermission { get; set; } = null;

	/// <summary>
	/// The command needed permissions.
	/// </summary>
	[JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Include)]
	public Permissions? DefaultMemberPermission { get; set; }

	/// <summary>
	/// Whether the command is allowed for dms.
	/// </summary>
	[JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Include)]
	public bool? DmPermission { get; set; }

	/// <summary>
	/// Whether the command is marked as NSFW.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool Nsfw { get; set; }

	/// <summary>
	/// Gets where the command is allowed at.
	/// </summary>
	[JsonProperty("contexts", NullValueHandling = NullValueHandling.Include)]
	public List<ApplicationCommandContexts>? AllowedContexts { get; set; }

	/// <summary>
	/// Gets the allowed integration types.
	/// </summary>
	[JsonProperty("integration_types", NullValueHandling = NullValueHandling.Ignore)]
	public List<ApplicationCommandIntegrationTypes>? IntegrationTypes { get; set; }

}

/// <summary>
/// Represents a application command edit payload.
/// </summary>
internal sealed class RestApplicationCommandEditPayload : ObservableApiObject
{
	/// <summary>
	/// Gets the name.
	/// </summary>
	[JsonProperty("name")]
	public Optional<string> Name { get; set; }

	/// <summary>
	/// Gets the name localizations.
	/// </summary>
	[JsonProperty("name_localizations")]
	public Optional<Dictionary<string, string>> NameLocalizations { get; set; }

	/// <summary>
	/// Gets the description.
	/// </summary>
	[JsonProperty("description")]
	public Optional<string> Description { get; set; }

	/// <summary>
	/// Gets the description localizations.
	/// </summary>
	[JsonProperty("description_localizations")]
	public Optional<Dictionary<string, string>> DescriptionLocalizations { get; set; }

	/// <summary>
	/// Gets the options.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<List<DiscordApplicationCommandOption>> Options { get; set; }

	/// <summary>
	/// The command needed permissions.
	/// </summary>
	[JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Include)]
	public Optional<Permissions?> DefaultMemberPermission { get; set; }

	/// <summary>
	/// Whether the command is allowed for dms.
	/// </summary>
	[JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool> DmPermission { get; set; }

	/// <summary>
	/// Whether the command is marked as NSFW.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Nsfw { get; set; }

	/// <summary>
	/// Gets where the command is allowed at.
	/// </summary>
	[JsonProperty("contexts", NullValueHandling = NullValueHandling.Include)]
	public Optional<List<ApplicationCommandContexts>?> AllowedContexts { get; set; }

	/// <summary>
	/// Gets the allowed integration types.
	/// </summary>
	[JsonProperty("integration_types", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<List<ApplicationCommandIntegrationTypes>?> IntegrationTypes { get; set; }
}

/// <summary>
/// Represents an interaction response payload.
/// </summary>
internal sealed class RestInteractionResponsePayload : ObservableApiObject
{
	/// <summary>
	/// Gets the type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionResponseType Type { get; set; }

	/// <summary>
	/// Gets the data.
	/// </summary>
	[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionApplicationCommandCallbackData Data { get; set; }

	/// <summary>
	/// Gets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAttachment> Attachments { get; set; }

	// TODO: Implement if it gets added to the api
	/// <summary>
	/// Gets the callback hints.
	/// </summary>
	[JsonProperty("hints", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordInteractionCallbackHint>? CallbackHints { get; set; }
}

/// <summary>
/// Represents an interaction response modal payload.
/// </summary>
internal sealed class RestInteractionModalResponsePayload : ObservableApiObject
{
	/// <summary>
	/// Gets the type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionResponseType Type { get; set; }

	/// <summary>
	/// Gets the data.
	/// </summary>
	[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionApplicationCommandModalCallbackData Data { get; set; }

	// TODO: Implement if it gets added to the api
	/// <summary>
	/// Gets the callback hints.
	/// </summary>
	[JsonProperty("hints", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordInteractionCallbackHint>? CallbackHints { get; set; }
}

/// <summary>
/// Represents an interaction response iFrame payload.
/// </summary>
internal sealed class RestInteractionIframeResponsePayload : ObservableApiObject
{
	/// <summary>
	/// Gets the type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionResponseType Type { get; set; }

	/// <summary>
	/// Gets the data.
	/// </summary>
	[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionApplicationCommandIframeCallbackData Data { get; set; }

	// TODO: Implement if it gets added to the api
	/// <summary>
	/// Gets the callback hints.
	/// </summary>
	[JsonProperty("hints", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordInteractionCallbackHint>? CallbackHints { get; set; }
}

/// <summary>
/// Represents a followup message create payload.
/// </summary>
internal sealed class RestFollowupMessageCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; set; }

	/// <summary>
	/// Get whether the message is tts.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsTts { get; set; }

	/// <summary>
	/// Gets the embeds.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordEmbed> Embeds { get; set; }

	/// <summary>
	/// Gets the mentions.
	/// </summary>
	[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMentions Mentions { get; set; }

	/// <summary>
	/// Gets the flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; set; }

	/// <summary>
	/// Gets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyCollection<DiscordActionRowComponent> Components { get; set; }

	/// <summary>
	/// Gets attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAttachment> Attachments { get; set; }
}

/// <summary>
/// Represents a role connection metadata payload.
/// </summary>
internal sealed class RestApplicationRoleConnectionMetadataPayload : ObservableApiObject
{
	/// <summary>
	/// Gets the metadata type.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationRoleConnectionMetadataType Type { get; set; }

	/// <summary>
	/// Gets the metadata key.
	/// </summary>
	[JsonProperty("key")]
	public string Key { get; set; }

	/// <summary>
	/// Gets the metadata name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the metadata description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; }

	/// <summary>
	/// Gets the metadata name translations.
	/// </summary>
	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> NameLocalizations { get; set; }

	/// <summary>
	/// Gets the metadata description localizations.
	/// </summary>
	[JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> DescriptionLocalizations { get; set; }
}
