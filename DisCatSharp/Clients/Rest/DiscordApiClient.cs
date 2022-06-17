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
using System.Net;
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

/// <summary>
/// Represents a discord api client.
/// </summary>
public sealed partial class DiscordApiClient
{
	/// <summary>
	/// The audit log reason header name.
	/// </summary>
	private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

	/// <summary>
	/// Gets the discord client.
	/// </summary>
	internal BaseDiscordClient Discord { get; }

	/// <summary>
	/// Gets the rest client.
	/// </summary>
	internal RestClient Rest { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApiClient"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal DiscordApiClient(BaseDiscordClient client)
	{
		this.Discord = client;
		this.Rest = new RestClient(client);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApiClient"/> class.
	/// </summary>
	/// <param name="proxy">The proxy.</param>
	/// <param name="timeout">The timeout.</param>
	/// <param name="useRelativeRateLimit">If true, use relative rate limit.</param>
	/// <param name="logger">The logger.</param>
	internal DiscordApiClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRateLimit, ILogger logger) // This is for meta-clients, such as the webhook client
	{
		this.Rest = new RestClient(proxy, timeout, useRelativeRateLimit, logger);
	}

	/// <summary>
	/// Builds the query string.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <param name="post">Whether this query will be transmitted via POST.</param>
	private static string BuildQueryString(IDictionary<string, string> values, bool post = false)
	{
		if (values == null || values.Count == 0)
			return string.Empty;

		var valsCollection = values.Select(xkvp =>
			$"{WebUtility.UrlEncode(xkvp.Key)}={WebUtility.UrlEncode(xkvp.Value)}");
		var vals = string.Join("&", valsCollection);

		return !post ? $"?{vals}" : vals;
	}

	/// <summary>
	/// Prepares the message.
	/// </summary>
	/// <param name="msgRaw">The msg_raw.</param>
	/// <returns>A DiscordMessage.</returns>
	private DiscordMessage PrepareMessage(JToken msgRaw)
	{
		var author = msgRaw["author"].ToObject<TransportUser>();
		var ret = msgRaw.ToDiscordObject<DiscordMessage>();
		ret.Discord = this.Discord;

		this.PopulateMessage(author, ret);

		var referencedMsg = msgRaw["referenced_message"];
		if (ret.MessageType == MessageType.Reply && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
		{
			author = referencedMsg["author"].ToObject<TransportUser>();
			ret.ReferencedMessage.Discord = this.Discord;
			this.PopulateMessage(author, ret.ReferencedMessage);
		}

		if (ret.Channel != null)
			return ret;

		var channel = !ret.GuildId.HasValue
			? new DiscordDmChannel
			{
				Id = ret.ChannelId,
				Discord = this.Discord,
				Type = ChannelType.Private
			}
			: new DiscordChannel
			{
				Id = ret.ChannelId,
				GuildId = ret.GuildId,
				Discord = this.Discord
			};

		ret.Channel = channel;

		return ret;
	}

	/// <summary>
	/// Populates the message.
	/// </summary>
	/// <param name="author">The author.</param>
	/// <param name="ret">The message.</param>
	private void PopulateMessage(TransportUser author, DiscordMessage ret)
	{
		var guild = ret.Channel?.Guild;

		//If this is a webhook, it shouldn't be in the user cache.
		if (author.IsBot && int.Parse(author.Discriminator) == 0)
		{
			ret.Author = new DiscordUser(author) { Discord = this.Discord };
		}
		else
		{
			if (!this.Discord.UserCache.TryGetValue(author.Id, out var usr))
			{
				this.Discord.UserCache[author.Id] = usr = new DiscordUser(author) { Discord = this.Discord };
			}

			if (guild != null)
			{
				if (!guild.Members.TryGetValue(author.Id, out var mbr))
					mbr = new DiscordMember(usr) { Discord = this.Discord, GuildId = guild.Id };
				ret.Author = mbr;
			}
			else
			{
				ret.Author = usr;
			}
		}

		ret.PopulateMentions();

		if (ret.ReactionsInternal == null)
			ret.ReactionsInternal = new List<DiscordReaction>();
		foreach (var xr in ret.ReactionsInternal)
			xr.Emoji.Discord = this.Discord;
	}

	/// <summary>
	/// Executes a rest request.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="payload">The payload.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	internal Task<RestResponse> DoRequestAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, string payload = null, double? ratelimitWaitOverride = null)
	{
		var req = new RestRequest(client, bucket, url, method, route, headers, payload, ratelimitWaitOverride);

		if (this.Discord != null)
			this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req);

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	/// Executes a multipart rest request for stickers.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="file">The file.</param>
	/// <param name="name">The sticker name.</param>
	/// <param name="tags">The sticker tag.</param>
	/// <param name="description">The sticker description.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	private Task<RestResponse> DoStickerMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null,
		DiscordMessageFile file = null, string name = "", string tags = "", string description = "", double? ratelimitWaitOverride = null)
	{
		var req = new MultipartStickerWebRequest(client, bucket, url, method, route, headers, file, name, tags, description, ratelimitWaitOverride);

		if (this.Discord != null)
			this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req);

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	/// Executes a multipart request.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="values">The values.</param>
	/// <param name="files">The files.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	private Task<RestResponse> DoMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
		IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimitWaitOverride = null)
	{
		var req = new MultipartWebRequest(client, bucket, url, method, route, headers, values, files, ratelimitWaitOverride);

		if (this.Discord != null)
			this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req);

		return req.WaitForCompletionAsync();
	}

	#region Webhooks
	/// <summary>
	/// Creates the webhook async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, Optional<string> base64Avatar, string reason)
	{
		var pld = new RestWebhookPayload
		{
			Name = name,
			AvatarBase64 = base64Avatar.ValueOrDefault(),
			AvatarSet = base64Avatar.HasValue
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Gets the channel webhooks async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

		return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
	}

	/// <summary>
	/// Gets the guild webhooks async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

		return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
	}

	/// <summary>
	/// Gets the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>

	internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhookId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {webhook_id = webhookId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
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

		var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
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
			var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: builder.Files);

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
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

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
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
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
	#endregion

	#region Reactions
	/// <summary>
	/// Creates the reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>

	internal Task CreateReactionAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Deletes the own reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>

	internal Task DeleteOwnReactionAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Deletes the user reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteUserReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId, emoji, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Gets the reactions async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="afterId">The after_id.</param>
	/// <param name="limit">The limit.</param>

	internal async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, string emoji, ulong? afterId = null, int limit = 25)
	{
		var urlParams = new Dictionary<string, string>();
		if (afterId.HasValue)
			urlParams["after"] = afterId.Value.ToString(CultureInfo.InvariantCulture);

		urlParams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, BuildQueryString(urlParams), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var reactersRaw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response);
		var reacters = new List<DiscordUser>();
		foreach (var xr in reactersRaw)
		{
			var usr = new DiscordUser(xr) { Discord = this.Discord };
			usr = this.Discord.UserCache.AddOrUpdate(xr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				return old;
			});

			reacters.Add(usr);
		}

		return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters));
	}

	/// <summary>
	/// Deletes the all reactions async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteAllReactionsAsync(ulong channelId, ulong messageId, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Deletes the reactions emoji async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>

	internal Task DeleteReactionsEmojiAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}
	#endregion

	#region Emoji
	/// <summary>
	/// Gets the guild emojis async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);
		var users = new Dictionary<ulong, DiscordUser>();
		var emojis = new List<DiscordGuildEmoji>();
		foreach (var rawEmoji in emojisRaw)
		{
			var xge = rawEmoji.ToObject<DiscordGuildEmoji>();
			xge.Guild = gld;

			var xtu = rawEmoji["user"]?.ToObject<TransportUser>();
			if (xtu != null)
			{
				if (!users.ContainsKey(xtu.Id))
				{
					var user = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);
					users[user.Id] = user;
				}

				xge.User = users[xtu.Id];
			}

			emojis.Add(xge);
		}

		return new ReadOnlyCollection<DiscordGuildEmoji>(emojis);
	}

	/// <summary>
	/// Gets the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>

	internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guildId, ulong emojiId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu != null)
			emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

		return emoji;
	}

	/// <summary>
	/// Creates the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="imageb64">The imageb64.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guildId, string name, string imageb64, IEnumerable<ulong> roles, string reason)
	{
		var pld = new RestGuildEmojiCreatePayload
		{
			Name = name,
			ImageB64 = imageb64,
			Roles = roles?.ToArray()
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		emoji.User = xtu != null
			? gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu)
			: this.Discord.CurrentUser;

		return emoji;
	}

	/// <summary>
	/// Modifies the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guildId, ulong emojiId, string name, IEnumerable<ulong> roles, string reason)
	{
		var pld = new RestGuildEmojiModifyPayload
		{
			Name = name,
			Roles = roles?.ToArray()
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu != null)
			emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

		return emoji;
	}

	/// <summary>
	/// Deletes the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteGuildEmojiAsync(ulong guildId, ulong emojiId, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
	#endregion

	#region Stickers

	/// <summary>
	/// Gets a sticker.
	/// </summary>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetStickerAsync(ulong stickerId)
	{
		var route = $"{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Gets the sticker packs.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
	{
		var route = $"{Endpoints.STICKERPACKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response)["sticker_packs"] as JArray;
		var ret = json.ToDiscordObject<DiscordStickerPack[]>();

		return ret.ToList();
	}

	/// <summary>
	/// Gets the guild stickers.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<IReadOnlyList<DiscordSticker>> GetGuildStickersAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var json = JArray.Parse(res.Response);
		var ret = json.ToDiscordObject<DiscordSticker[]>();

		for (var i = 0; i < ret.Length; i++)
		{
			var stkr = ret[i];
			stkr.Discord = this.Discord;

			if (json[i]["user"] is JObject obj) // Null = Missing stickers perm //
			{
				var tsr = obj.ToDiscordObject<TransportUser>();
				var usr = new DiscordUser(tsr) {Discord = this.Discord};
				usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					return old;
				});
				stkr.User = usr;
			}
		}

		return ret.ToList();
	}

	/// <summary>
	/// Gets a guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetGuildStickerAsync(ulong guildId, ulong stickerId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var ret = json.ToDiscordObject<DiscordSticker>();
		if (json["user"] is not null) // Null = Missing stickers perm //
		{
			var tsr = json["user"].ToDiscordObject<TransportUser>();
			var usr = new DiscordUser(tsr) {Discord = this.Discord};
			usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				return old;
			});
			ret.User = usr;
		}
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Creates the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="file">The file.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> CreateGuildStickerAsync(ulong guildId, string name, string description, string tags, DiscordMessageFile file, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var res = await this.DoStickerMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, file, name, tags, description);

		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Modifies the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> ModifyGuildStickerAsync(ulong guildId, ulong stickerId, Optional<string> name, Optional<string> description, Optional<string> tags, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var pld = new RestStickerModifyPayload()
		{
			Name = name,
			Description = description,
			Tags = tags
		};

		var values = new Dictionary<string, string>
		{
			["payload_json"] = DiscordJson.SerializeObject(pld)
		};

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route);
		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();
		ret.Discord = this.Discord;

		return null;
	}

	/// <summary>
	/// Deletes the guild sticker async.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task DeleteGuildStickerAsync(ulong guildId, ulong stickerId, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, sticker_id = stickerId }, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
	#endregion

	#region Application Commands

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

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
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Gets the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, bool withLocalizations = false)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var querydict = new Dictionary<string, string>
		{
			["with_localizations"] = withLocalizations.ToString().ToLower()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Bulk overwrites the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commands">The commands.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, IEnumerable<DiscordApplicationCommand> commands)
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

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Creates a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="command">The command.</param>
	internal async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong applicationId, ulong guildId, DiscordApplicationCommand command)
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

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Gets a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Edits a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="options">The options.</param>
	/// <param name="nameLocalization">The localizations of the name.</param>
	/// <param name="descriptionLocalization">The localizations of the description.</param>
	/// <param name="defaultMemberPermission">The default member permissions.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	internal async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId,
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

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task DeleteGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	#region Permissions 2.1

	/// <summary>
	/// Gets the guild application command permissions.
	/// </summary>
	/// <param name="applicationId">The target application id.</param>
	/// <param name="guildId">The target guild id.</param>
	internal async Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> GetGuildApplicationCommandPermissionsAsync(ulong applicationId, ulong guildId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}{Endpoints.PERMISSIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermission>>(res.Response);

		foreach (var app in ret)
			app.Discord = this.Discord;

		return ret.ToList();
	}

	/// <summary>
	/// Gets a guild application command permission.
	/// </summary>
	/// <param name="applicationId">The target application id.</param>
	/// <param name="guildId">The target guild id.</param>
	/// <param name="commandId">The target command id.</param>
	internal async Task<DiscordGuildApplicationCommandPermission> GetGuildApplicationCommandPermissionAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id{Endpoints.PERMISSIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermission>(res.Response);

		ret.Discord = this.Discord;

		return ret;
	}

	#endregion

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
			await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files);

			foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}
		}
		else
		{
			await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
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
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
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
		var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
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
	#endregion

	#region Misc
	/// <summary>
	/// Gets the current application info async.
	/// </summary>

	internal Task<TransportApplication> GetCurrentApplicationInfoAsync()
		=> this.GetApplicationInfoAsync("@me");

	/// <summary>
	/// Gets the application info async.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>

	internal Task<TransportApplication> GetApplicationInfoAsync(ulong applicationId)
		=> this.GetApplicationInfoAsync(applicationId.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	/// Gets the application info async.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>

	private async Task<TransportApplication> GetApplicationInfoAsync(string applicationId)
	{
		var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return JsonConvert.DeserializeObject<TransportApplication>(res.Response);
	}

	/// <summary>
	/// Gets the application assets async.
	/// </summary>
	/// <param name="application">The application.</param>

	internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
	{
		var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id{Endpoints.ASSETS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id = application.Id }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
		foreach (var asset in assets)
		{
			asset.Discord = application.Discord;
			asset.Application = application;
		}

		return new ReadOnlyCollection<DiscordApplicationAsset>(new List<DiscordApplicationAsset>(assets));
	}

	/// <summary>
	/// Gets the gateway info async.
	/// </summary>

	internal async Task<GatewayInfo> GetGatewayInfoAsync()
	{
		var headers = Utilities.GetBaseHeaders();
		var route = Endpoints.GATEWAY;
		if (this.Discord.Configuration.TokenType == TokenType.Bot)
			route += Endpoints.BOT;
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, headers).ConfigureAwait(false);

		var info = JObject.Parse(res.Response).ToObject<GatewayInfo>();
		info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.ResetAfterInternal);
		return info;
	}
	#endregion
}
