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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{

	/// <summary>
	/// Gets the global application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(ulong applicationId, bool withLocalizations = false)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId }, out var path);

		var querydict = new Dictionary<string, string>
		{
			["with_localizations"] = withLocalizations.ToString().ToLower()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Bulk overwrites the global application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commands">The commands.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ulong applicationId, IEnumerable<DiscordApplicationCommand> commands)
	{
		var pld = new List<RestApplicationCommandCreatePayload>();
		foreach (var command in commands)
		{
			pld.Add(new RestApplicationCommandCreatePayload
			{
				Type = command.Type,
				Name = command.Name,
				Description = command.Description,
				Options = command.Options,
				NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
				DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
				DefaultMemberPermission = command.DefaultMemberPermissions,
				DmPermission = command.DmPermission/*,
				Nsfw = command.IsNsfw*/
			});
		}

		this.Discord.Logger.LogDebug(DiscordJson.SerializeObject(pld));

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {application_id = applicationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Creates a global application command.
	/// </summary>
	/// <param name="applicationId">The applicationid.</param>
	/// <param name="command">The command.</param>
	internal async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(ulong applicationId, DiscordApplicationCommand command)
	{
		var pld = new RestApplicationCommandCreatePayload
		{
			Type = command.Type,
			Name = command.Name,
			Description = command.Description,
			Options = command.Options,
			NameLocalizations = command.NameLocalizations.GetKeyValuePairs(),
			DescriptionLocalizations = command.DescriptionLocalizations.GetKeyValuePairs(),
			DefaultMemberPermission = command.DefaultMemberPermissions,
			DmPermission = command.DmPermission/*,
			Nsfw = command.IsNsfw*/
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {application_id = applicationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Gets a global application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong applicationId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Edits a global application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commandId">The command id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="options">The options.</param>
	/// <param name="nameLocalization">The localizations of the name.</param>
	/// <param name="descriptionLocalization">The localizations of the description.</param>
	/// <param name="defaultMemberPermission">The default member permissions.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	internal async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong applicationId, ulong commandId,
		Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options,
		Optional<DiscordApplicationCommandLocalization> nameLocalization, Optional<DiscordApplicationCommandLocalization> descriptionLocalization,
		Optional<Permissions> defaultMemberPermission, Optional<bool> dmPermission, Optional<bool> isNsfw)
	{
		var pld = new RestApplicationCommandEditPayload
		{
			Name = name,
			Description = description,
			Options = options,
			DefaultMemberPermission = defaultMemberPermission,
			DmPermission = dmPermission,
			NameLocalizations = nameLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault(),
			DescriptionLocalizations = descriptionLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault()/*,
			Nsfw = isNsfw*/
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {application_id = applicationId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes a global application command.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>
	/// <param name="commandId">The command_id.</param>

	internal async Task DeleteGlobalApplicationCommandAsync(ulong applicationId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {application_id = applicationId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}


	/// <summary>
	/// Creates the interaction response.
	/// </summary>
	/// <param name="interactionId">The interaction id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="type">The type.</param>
	/// <param name="builder">The builder.</param>
	internal async Task CreateInteractionResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionResponseBuilder builder)
	{
		if (builder?.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		RestInteractionResponsePayload pld;

		if (type != InteractionResponseType.AutoCompleteResult)
		{
			var data = builder != null ? new DiscordInteractionApplicationCommandCallbackData
			{
				Content = builder.Content ?? null,
				Embeds = builder.Embeds ?? null,
				IsTts = builder.IsTts,
				Mentions = builder.Mentions ?? null,
				Flags = builder.IsEphemeral ? MessageFlags.Ephemeral : null,
				Components = builder.Components ?? null,
				Choices = null
			} : null;


			pld = new RestInteractionResponsePayload
			{
				Type = type,
				Data = data
			};


			if (builder != null && builder.Files != null && builder.Files.Count > 0)
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = new();
				foreach (var file in builder.Files)
				{
					DiscordAttachment att = new()
					{
						Id = fileId,
						Discord = this.Discord,
						Description = file.Description,
						FileName = file.FileName,
						FileSize = null
					};
					attachments.Add(att);
					fileId++;
				}
				pld.Attachments = attachments;
				pld.Data.Attachments = attachments;
			}
		}
		else
		{
			pld = new RestInteractionResponsePayload
			{
				Type = type,
				Data = new DiscordInteractionApplicationCommandCallbackData
				{
					Content = null,
					Embeds = null,
					IsTts = null,
					Mentions = null,
					Flags = null,
					Components = null,
					Choices = builder.Choices,
					Attachments = null
				},
				Attachments = null
			};
		}

		var values = new Dictionary<string, string>();

		if (builder != null)
			if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTts == true || builder.Mentions != null || builder.Files?.Count > 0)
				values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {interaction_id = interactionId, interaction_token = interactionToken }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "false").Build();
		if (builder != null)
		{
			await this.ExecuteMultipartRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files);

			foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}
		}
		else
		{
			await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
		}
	}

	/// <summary>
	/// Creates the interaction response.
	/// </summary>
	/// <param name="interactionId">The interaction id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="type">The type.</param>
	/// <param name="builder">The builder.</param>
	internal async Task CreateInteractionModalResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionModalBuilder builder)
	{
		if (builder.ModalComponents.Any(mc => mc.Components.Any(c => c.Type != ComponentType.InputText && c.Type != ComponentType.Select)))
			throw new NotSupportedException("Can't send any other type then Input Text or Select as Modal Component.");

		var pld = new RestInteractionModalResponsePayload
		{
			Type = type,
			Data = new DiscordInteractionApplicationCommandModalCallbackData
			{
				Title = builder.Title,
				CustomId = builder.CustomId,
				ModalComponents = builder.ModalComponents
			}
		};

		var values = new Dictionary<string, string>();

		var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {interaction_id = interactionId, interaction_token = interactionToken }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Gets the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	internal Task<DiscordMessage> GetOriginalInteractionResponseAsync(ulong applicationId, string interactionToken) =>
		this.GetWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, null);

	/// <summary>
	/// Edits the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="builder">The builder.</param>
	internal Task<DiscordMessage> EditOriginalInteractionResponseAsync(ulong applicationId, string interactionToken, DiscordWebhookBuilder builder) =>
		this.EditWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, builder, null);

	/// <summary>
	/// Deletes the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	internal Task DeleteOriginalInteractionResponseAsync(ulong applicationId, string interactionToken) =>
		this.DeleteWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, null);

	/// <summary>
	/// Creates the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="builder">The builder.</param>
	internal async Task<DiscordMessage> CreateFollowupMessageAsync(ulong applicationId, string interactionToken, DiscordFollowupMessageBuilder builder)
	{
		builder.Validate();

		if (builder.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var values = new Dictionary<string, string>();
		var pld = new RestFollowupMessageCreatePayload
		{
			Content = builder.Content,
			IsTts = builder.IsTts,
			Embeds = builder.Embeds,
			Flags = builder.Flags,
			Components = builder.Components
		};


		if (builder.Files != null && builder.Files.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = new();
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName,
					FileSize = null
				};
				attachments.Add(att);
				fileId++;
			}
			pld.Attachments = attachments;
		}

		if (builder.Mentions != null)
			pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

		if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTts == true || builder.Mentions != null || builder.Files?.Count > 0)
			values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.WEBHOOKS}/:application_id/:interaction_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {application_id = applicationId, interaction_token = interactionToken }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
		var res = await this.ExecuteMultipartRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

		foreach (var att in ret.AttachmentsInternal)
		{
			att.Discord = this.Discord;
		}

		foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
		{
			file.Stream.Position = file.ResetPositionTo.Value;
		}

		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Gets the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	internal Task<DiscordMessage> GetFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId) =>
		this.GetWebhookMessageAsync(applicationId, interactionToken, messageId);

	/// <summary>
	/// Edits the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	/// <param name="builder">The builder.</param>
	internal Task<DiscordMessage> EditFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId, DiscordWebhookBuilder builder) =>
		this.EditWebhookMessageAsync(applicationId, interactionToken, messageId.ToString(), builder, null);

	/// <summary>
	/// Deletes the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	internal Task DeleteFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId) =>
		this.DeleteWebhookMessageAsync(applicationId, interactionToken, messageId);
}
