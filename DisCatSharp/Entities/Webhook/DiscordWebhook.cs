// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.IO;
using System.Threading.Tasks;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents information about a Discord webhook.
    /// </summary>
    public class DiscordWebhook : SnowflakeObject, IEquatable<DiscordWebhook>
    {
        /// <summary>
        /// Gets the api client.
        /// </summary>
        internal DiscordApiClient ApiClient { get; set; }

        /// <summary>
        /// Gets the ID of the guild this webhook belongs to.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the ID of the channel this webhook belongs to.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the user this webhook was created by.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the default name of this webhook.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets hash of the default avatar for this webhook.
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        internal string AvatarHash { get; set; }

        /// <summary>
        /// Gets the partial source guild for this webhook (For Channel Follower Webhooks).
        /// </summary>
        [JsonProperty("source_guild", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuild SourceGuild { get; set; }

        /// <summary>
        /// Gets the partial source channel for this webhook (For Channel Follower Webhooks).
        /// </summary>
        [JsonProperty("source_channel", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordChannel SourceChannel { get; set; }

        /// <summary>
        /// Gets the url used for executing the webhook.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        /// <summary>
        /// Gets the default avatar url for this webhook.
        /// </summary>
        public string AvatarUrl
            => !string.IsNullOrWhiteSpace(this.AvatarHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Avatars}/{this.Id}/{this.AvatarHash}.png?size=1024" : null;

        /// <summary>
        /// Gets the secure token of this webhook.
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordWebhook"/> class.
        /// </summary>
        internal DiscordWebhook() { }

        /// <summary>
        /// Modifies this webhook.
        /// </summary>
        /// <param name="Name">New default name for this webhook.</param>
        /// <param name="Avatar">New avatar for this webhook.</param>
        /// <param name="ChannelId">The new channel id to move the webhook to.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>The modified webhook.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordWebhook> Modify(string Name = null, Optional<Stream> Avatar = default, ulong? ChannelId = null, string Reason = null)
        {
            var avatarb64 = Optional.FromNoValue<string>();
            if (Avatar.HasValue && Avatar.Value != null)
                using (var imgtool = new ImageTool(Avatar.Value))
                    avatarb64 = imgtool.GetBase64();
            else if (Avatar.HasValue)
                avatarb64 = null;

            var newChannelId = ChannelId ?? this.ChannelId;

            return this.Discord.ApiClient.ModifyWebhookAsync(this.Id, newChannelId, Name, avatarb64, Reason);
        }

        /// <summary>
        /// Gets a previously-sent webhook message.
        /// </summary>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> GetMessageAsync(ulong MessageId)
            => await (this.Discord?.ApiClient ?? this.ApiClient).GetWebhookMessage(this.Id, this.Token, MessageId).ConfigureAwait(false);

        /// <summary>
        /// Gets a previously-sent webhook message.
        /// </summary>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> GetMessageAsync(ulong MessageId, ulong ThreadId)
            => await (this.Discord?.ApiClient ?? this.ApiClient).GetWebhookMessage(this.Id, this.Token, MessageId, ThreadId).ConfigureAwait(false);

        /// <summary>
        /// Permanently deletes this webhook.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Delete()
            => this.Discord.ApiClient.DeleteWebhook(this.Id, this.Token);

        /// <summary>
        /// Executes this webhook with the given <see cref="DiscordWebhookBuilder"/>.
        /// </summary>
        /// <param name="Builder">Webhook builder filled with data to send.</param>
        /// <param name="thread_id">Target thread id (Optional). Defaults to null.</param>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordMessage> Execute(DiscordWebhookBuilder Builder, string ThreadId = null)
            => (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookAsync(this.Id, this.Token, Builder, ThreadId);

        /// <summary>
        /// Executes this webhook in Slack compatibility mode.
        /// </summary>
        /// <param name="Json">JSON containing Slack-compatible payload for this webhook.</param>
        /// <param name="thread_id">Target thread id (Optional). Defaults to null.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task ExecuteSlack(string Json, string ThreadId = null)
            => (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookSlackAsync(this.Id, this.Token, Json, ThreadId);

        /// <summary>
        /// Executes this webhook in GitHub compatibility mode.
        /// </summary>
        /// <param name="Json">JSON containing GitHub-compatible payload for this webhook.</param>
        /// <param name="thread_id">Target thread id (Optional). Defaults to null.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task ExecuteGithub(string Json, string ThreadId = null)
            => (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookGithubAsync(this.Id, this.Token, Json, ThreadId);

        /// <summary>
        /// Edits a previously-sent webhook message.
        /// </summary>
        /// <param name="MessageId">The id of the message to edit.</param>
        /// <param name="Builder">The builder of the message to edit.</param>
        /// <param name="thread_id">Target thread id (Optional). Defaults to null.</param>
        /// <returns>The modified <see cref="DiscordMessage"/></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> EditMessageAsync(ulong MessageId, DiscordWebhookBuilder Builder, string ThreadId = null)
        {
            Builder.Validate(true);
            if (Builder._keepAttachments.HasValue && Builder._keepAttachments.Value)
            {
                Builder._attachments.AddRange(this.ApiClient.GetWebhookMessageAsync(this.Id, this.Token, MessageId.ToString(), ThreadId).Result.Attachments);
            }
            else if (Builder._keepAttachments.HasValue)
            {
                Builder._attachments.Clear();
            }
            return await (this.Discord?.ApiClient ?? this.ApiClient).EditWebhookMessageAsync(this.Id, this.Token, MessageId.ToString(), Builder, ThreadId).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a message that was created by the webhook.
        /// </summary>
        /// <param name="MessageId">The id of the message to delete</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteMessage(ulong MessageId)
            => (this.Discord?.ApiClient ?? this.ApiClient).DeleteWebhookMessage(this.Id, this.Token, MessageId);

        /// <summary>
        /// Deletes a message that was created by the webhook.
        /// </summary>
        /// <param name="MessageId">The id of the message to delete</param>
        /// <param name="ThreadId">Target thread id (Optional). Defaults to null.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteMessage(ulong MessageId, ulong ThreadId)
            => (this.Discord?.ApiClient ?? this.ApiClient).DeleteWebhookMessage(this.Id, this.Token, MessageId, ThreadId);

        /// <summary>
        /// Checks whether this <see cref="DiscordWebhook"/> is equal to another object.
        /// </summary>
        /// <param name="Obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordWebhook"/>.</returns>
        public override bool Equals(object Obj) => this.Equals(Obj as DiscordWebhook);

        /// <summary>
        /// Checks whether this <see cref="DiscordWebhook"/> is equal to another <see cref="DiscordWebhook"/>.
        /// </summary>
        /// <param name="E"><see cref="DiscordWebhook"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordWebhook"/> is equal to this <see cref="DiscordWebhook"/>.</returns>
        public bool Equals(DiscordWebhook E) => E is not null && (ReferenceEquals(this, E) || this.Id == E.Id);

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordWebhook"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordWebhook"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordWebhook"/> objects are equal.
        /// </summary>
        /// <param name="E1">First webhook to compare.</param>
        /// <param name="E2">Second webhook to compare.</param>
        /// <returns>Whether the two webhooks are equal.</returns>
        public static bool operator ==(DiscordWebhook E1, DiscordWebhook E2)
        {
            var o1 = E1 as object;
            var o2 = E2 as object;

            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || E1.Id == E2.Id);
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordWebhook"/> objects are not equal.
        /// </summary>
        /// <param name="E1">First webhook to compare.</param>
        /// <param name="E2">Second webhook to compare.</param>
        /// <returns>Whether the two webhooks are not equal.</returns>
        public static bool operator !=(DiscordWebhook E1, DiscordWebhook E2)
            => !(E1 == E2);
    }
}
