// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a application command create payload.
/// </summary>
internal class RestApplicationCommandCreatePayload
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
	/*
	/// <summary>
	/// Whether the command is marked as NSFW.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool Nsfw { get; set; }*/
}

/// <summary>
/// Represents a application command edit payload.
/// </summary>
internal class RestApplicationCommandEditPayload
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
	[JsonProperty("options")]
	public Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> Options { get; set; }

	/// <summary>
	/// Gets the default permission.
	/// </summary>
	[JsonProperty("default_permission", NullValueHandling = NullValueHandling.Include)]
	public bool? DefaultPermission { get; set; } = null;

	/// <summary>
	/// The command needed permissions.
	/// </summary>
	[JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Include)]
	public Optional<Permissions> DefaultMemberPermission { get; set; }

	/// <summary>
	/// Whether the command is allowed for dms.
	/// </summary>
	[JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool> DmPermission { get; set; }
	/*
	/// <summary>
	/// Whether the command is marked as NSFW.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Nsfw { get; set; }*/
}

/// <summary>
/// Represents a interaction response payload.
/// </summary>
internal class RestInteractionResponsePayload
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
}

/// <summary>
/// Represents a interaction response payload.
/// </summary>
internal class RestInteractionModalResponsePayload
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
}

/// <summary>
/// Represents a followup message create payload.
/// </summary>
internal class RestFollowupMessageCreatePayload
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
	public int? Flags { get; set; }

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
