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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>

	internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhookId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {webhook_id = webhookId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Gets the webhook with token async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>

	internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhookId, string webhookToken)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Token = webhookToken;
		ret.Id = webhookId;
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Modifies the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, ulong channelId, string name, Optional<string> base64Avatar, string reason)
	{
		var pld = new RestWebhookPayload
		{
			Name = name,
			AvatarBase64 = base64Avatar.ValueOrDefault(),
			AvatarSet = base64Avatar.HasValue,
			ChannelId = channelId
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Modifies the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string webhookToken, string reason)
	{
		var pld = new RestWebhookPayload
		{
			Name = name,
			AvatarBase64 = base64Avatar
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Deletes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteWebhookAsync(ulong webhookId, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {webhook_id = webhookId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Deletes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteWebhookAsync(ulong webhookId, string webhookToken, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Executes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> ExecuteWebhookAsync(ulong webhookId, string webhookToken, DiscordWebhookBuilder builder, string threadId)
	{
		builder.Validate();

		if (builder.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var values = new Dictionary<string, string>();
		var pld = new RestWebhookExecutePayload
		{
			Content = builder.Content,
			Username = builder.Username.ValueOrDefault(),
			AvatarUrl = builder.AvatarUrl.ValueOrDefault(),
			IsTts = builder.IsTts,
			Embeds = builder.Embeds,
			Components = builder.Components
		};

		if (builder.Mentions != null)
			pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

		if (builder.Files?.Count > 0)
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

		if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.Files?.Count > 0 || builder.IsTts == true || builder.Mentions != null)
			values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);

		var url = qub.Build();

		var res = await this.ExecuteMultipartRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

		foreach (var att in ret.Attachments)
			att.Discord = this.Discord;

		foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
		{
			file.Stream.Position = file.ResetPositionTo.Value;
		}

		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Executes the webhook slack async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="jsonPayload">The json_payload.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> ExecuteWebhookSlackAsync(ulong webhookId, string webhookToken, string jsonPayload, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.SLACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Executes the webhook github async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="jsonPayload">The json_payload.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> ExecuteWebhookGithubAsync(ulong webhookId, string webhookToken, string jsonPayload, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.GITHUB}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Edits the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, DiscordWebhookBuilder builder, string threadId)
	{
		builder.Validate(true);

		var pld = new RestWebhookMessageEditPayload
		{
			Content = builder.Content,
			Embeds = builder.Embeds,
			Mentions = builder.Mentions,
			Components = builder.Components,
		};

		if (builder.Files?.Count > 0)
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
			if (builder.Attachments != null && builder.Attachments?.Count() > 0)
				attachments.AddRange(builder.Attachments);

			pld.Attachments = attachments;

			var values = new Dictionary<string, string>
			{
				["payload_json"] = DiscordJson.SerializeObject(pld)
			};
			var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

			var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
			if (threadId != null)
				qub.AddParameter("thread_id", threadId);

			var url = qub.Build();
			var res = await this.ExecuteMultipartRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: builder.Files);

			var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

			ret.Discord = this.Discord;

			foreach (var att in ret.AttachmentsInternal)
				att.Discord = this.Discord;

			foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}

			return ret;
		}
		else
		{
			pld.Attachments = builder.Attachments;

			var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

			var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
			if (threadId != null)
				qub.AddParameter("thread_id", threadId);

			var url = qub.Build();
			var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

			var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

			ret.Discord = this.Discord;

			foreach (var att in ret.AttachmentsInternal)
				att.Discord = this.Discord;

			return ret;
		}
	}

	/// <summary>
	/// Edits the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>

	internal Task<DiscordMessage> EditWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, DiscordWebhookBuilder builder, ulong threadId) =>
		this.EditWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), builder, threadId.ToString());

	/// <summary>
	/// Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId) =>
		this.GetWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), null);

	/// <summary>
	/// Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, ulong threadId) =>
		this.GetWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), threadId.ToString());

	/// <summary>
	/// Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId) =>
		this.DeleteWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), null);

	/// <summary>
	/// Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, ulong threadId) =>
		this.DeleteWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), threadId.ToString());
}
