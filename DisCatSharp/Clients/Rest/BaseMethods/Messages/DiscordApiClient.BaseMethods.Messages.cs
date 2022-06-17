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
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal async Task<DiscordMessage> GetMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	/// Creates the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="content">The content.</param>
	/// <param name="embeds">The embeds.</param>
	/// <param name="sticker">The sticker.</param>
	/// <param name="replyMessageId">The reply message id.</param>
	/// <param name="mentionReply">If true, mention reply.</param>
	/// <param name="failOnInvalidReply">If true, fail on invalid reply.</param>

	internal async Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content, IEnumerable<DiscordEmbed> embeds, DiscordSticker sticker, ulong? replyMessageId, bool mentionReply, bool failOnInvalidReply)
	{
		if (content != null && content.Length > 2000)
			throw new ArgumentException("Message content length cannot exceed 2000 characters.");

		if (!embeds?.Any() ?? true)
		{
			if (content == null && sticker == null)
				throw new ArgumentException("You must specify message content, a sticker or an embed.");
			if (content.Length == 0)
				throw new ArgumentException("Message content must not be empty.");
		}

		if (embeds != null)
			foreach (var embed in embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var pld = new RestChannelMessageCreatePayload
		{
			HasContent = content != null,
			Content = content,
			StickersIds = sticker is null ? Array.Empty<ulong>() : new[] {sticker.Id},
			IsTts = false,
			HasEmbed = embeds?.Any() ?? false,
			Embeds = embeds
		};

		if (replyMessageId != null)
			pld.MessageReference = new InternalDiscordMessageReference { MessageId = replyMessageId, FailIfNotExists = failOnInvalidReply };

		if (replyMessageId != null)
			pld.Mentions = new DiscordMentions(Mentions.All, true, mentionReply);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	/// Creates the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="builder">The builder.</param>

	internal async Task<DiscordMessage> CreateMessageAsync(ulong channelId, DiscordMessageBuilder builder)
	{
		builder.Validate();

		if (builder.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed?.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var pld = new RestChannelMessageCreatePayload
		{
			HasContent = builder.Content != null,
			Content = builder.Content,
			StickersIds = builder.Sticker is null ? Array.Empty<ulong>() : new[] {builder.Sticker.Id},
			IsTts = builder.IsTts,
			HasEmbed = builder.Embeds != null,
			Embeds = builder.Embeds,
			Components = builder.Components
		};

		if (builder.ReplyId != null)
			pld.MessageReference = new InternalDiscordMessageReference { MessageId = builder.ReplyId, FailIfNotExists = builder.FailOnInvalidReply };

		pld.Mentions = new DiscordMentions(builder.Mentions ?? Mentions.All, builder.Mentions?.Any() ?? false, builder.MentionOnReply);

		if (builder.Files.Count == 0)
		{
			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
			var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));
			return ret;
		}
		else
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = new(builder.Files.Count);
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName
				};
				attachments.Add(att);
				fileId++;
			}
			pld.Attachments = attachments;

			var values = new Dictionary<string, string>
			{
				["payload_json"] = DiscordJson.SerializeObject(pld)
			};

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
			var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.ExecuteMultipartRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			foreach (var file in builder.FilesInternal.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}

			return ret;
		}
	}

	/// <summary>
	/// Gets the channel messages async.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="limit">The limit.</param>
	/// <param name="before">The before.</param>
	/// <param name="after">The after.</param>
	/// <param name="around">The around.</param>

	internal async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channelId, int limit, ulong? before, ulong? after, ulong? around)
	{
		var urlParams = new Dictionary<string, string>();
		if (around != null)
			urlParams["around"] = around?.ToString(CultureInfo.InvariantCulture);
		if (before != null)
			urlParams["before"] = before?.ToString(CultureInfo.InvariantCulture);
		if (after != null)
			urlParams["after"] = after?.ToString(CultureInfo.InvariantCulture);
		if (limit > 0)
			urlParams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var msgsRaw = JArray.Parse(res.Response);
		var msgs = new List<DiscordMessage>();
		foreach (var xj in msgsRaw)
			msgs.Add(this.PrepareMessage(xj));

		return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
	}

	/// <summary>
	/// Gets the channel message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal async Task<DiscordMessage> GetChannelMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	/// Edits the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="content">The content.</param>
	/// <param name="embeds">The embeds.</param>
	/// <param name="mentions">The mentions.</param>
	/// <param name="components">The components.</param>
	/// <param name="suppressEmbed">The suppress_embed.</param>
	/// <param name="files">The files.</param>
	/// <param name="attachments">The attachments to keep.</param>

	internal async Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds, Optional<IEnumerable<IMention>> mentions, IReadOnlyList<DiscordActionRowComponent> components, Optional<bool> suppressEmbed, IReadOnlyCollection<DiscordMessageFile> files, Optional<IEnumerable<DiscordAttachment>> attachments)
	{
		if (embeds.HasValue && embeds.Value != null)
			foreach (var embed in embeds.Value)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var pld = new RestChannelMessageEditPayload
		{
			HasContent = content.HasValue,
			Content = content.ValueOrDefault(),
			HasEmbed = embeds.HasValue && (embeds.Value?.Any() ?? false),
			Embeds = embeds.HasValue && (embeds.Value?.Any() ?? false) ? embeds.Value : null,
			Components = components,
			Flags = suppressEmbed.HasValue && (bool)suppressEmbed ? MessageFlags.SuppressedEmbeds : null,
			Mentions = mentions
		.Map(m => new DiscordMentions(m ?? Mentions.None, false, mentions.Value?.OfType<RepliedUserMention>().Any() ?? false))
		.ValueOrDefault()
		};

		if (files?.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachmentsNew = new();
			foreach (var file in files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName
				};
				attachmentsNew.Add(att);
				fileId++;
			}
			if (attachments.HasValue && attachments.Value.Any())
				attachmentsNew.AddRange(attachments.Value);

			pld.Attachments = attachmentsNew;

			var values = new Dictionary<string, string>
			{
				["payload_json"] = DiscordJson.SerializeObject(pld)
			};

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {channel_id = channelId, message_id = messageId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.ExecuteMultipartRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: files).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			foreach (var file in files.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}

			return ret;
		}
		else
		{
			pld.Attachments = attachments.ValueOrDefault();

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {channel_id = channelId, message_id = messageId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			return ret;
		}
	}

	/// <summary>
	/// Deletes the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteMessageAsync(ulong channelId, ulong messageId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Deletes the messages async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageIds">The message_ids.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds, string reason)
	{
		var pld = new RestChannelMessageBulkDeletePayload
		{
			Messages = messageIds
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}{Endpoints.BULK_DELETE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Triggers the typing async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal Task TriggerTypingAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.TYPING}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route);
	}

	/// <summary>
	/// Gets the pinned messages async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var msgsRaw = JArray.Parse(res.Response);
		var msgs = new List<DiscordMessage>();
		foreach (var xj in msgsRaw)
			msgs.Add(this.PrepareMessage(xj));

		return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
	}

	/// <summary>
	/// Pins the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task PinMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PUT, route);
	}

	/// <summary>
	/// Unpins the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task UnpinMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}
}
