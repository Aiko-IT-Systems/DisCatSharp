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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a discord thread channel.
    /// </summary>
    public class DiscordThreadChannel : DiscordChannel, IEquatable<DiscordThreadChannel>
    {
        /// <summary>
        /// Gets ID of the owner that started this thread.
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerId { get; internal set; }

        /// <summary>
        /// Gets the name of this thread.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public new string Name { get; internal set; }

        /// <summary>
        /// Gets the type of this thread.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public new ChannelType Type { get; internal set; }

        /// <summary>
        /// Gets whether this thread is private.
        /// </summary>
        [JsonIgnore]
        public new bool IsPrivate
            => this.Type == ChannelType.PrivateThread;

        /// <summary>
        /// Gets the ID of the last message sent in this thread.
        /// </summary>
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public new ulong? LastMessageId { get; internal set; }

        /// <summary>
        /// <para>Gets the slowmode delay configured for this thread.</para>
        /// <para>All bots, as well as users with <see cref="Permissions.ManageChannels"/> or <see cref="Permissions.ManageMessages"/> permissions in the channel are exempt from slowmode.</para>
        /// </summary>
        [JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
        public new int? PerUserRateLimit { get; internal set; }

        /// <summary>
        /// Gets an approximate count of messages in a thread, stops counting at 50.
        /// </summary>
        [JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? MessageCount { get; internal set; }

        /// <summary>
        /// Gets an approximate count of users in a thread, stops counting at 50.
        /// </summary>
        [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? MemberCount { get; internal set; }

        /// <summary>
        /// Represents the current member for this thread. This will have a value if the user has joined the thread.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordThreadChannelMember CurrentMember { get; internal set; }


        /// <summary>
        /// Gets when the last pinned message was pinned in this thread.
        /// </summary>
        [JsonIgnore]
        public new DateTimeOffset? LastPinTimestamp
            => !string.IsNullOrWhiteSpace(this.LastPinTimestampRaw) && DateTimeOffset.TryParse(this.LastPinTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
                dto : null;

        /// <summary>
        /// Gets when the last pinned message was pinned in this thread as raw string.
        /// </summary>
        [JsonProperty("last_pin_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal new string LastPinTimestampRaw { get; set; }

        /// <summary>
        /// Gets the threads metadata.
        /// </summary>
        [JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordThreadChannelMetadata ThreadMetadata { get; internal set; }

        /// <summary>
        /// Gets the thread members object.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordThreadChannelMember> ThreadMembers => new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannelMember>(this._threadMembers);

        [JsonProperty("thread_member", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordThreadChannelMember> _threadMembers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordThreadChannel"/> class.
        /// </summary>
        internal DiscordThreadChannel() { }

        #region Methods

        /// <summary>
        /// Deletes a thread.
        /// </summary>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task Delete(string Reason = null)
            => this.Discord.ApiClient.DeleteThread(this.Id, Reason);


        /// <summary>
        /// Modifies the current thread.
        /// </summary>
        /// <param name="Action">Action to perform on this thread</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.NotSupportedException">Thrown when the <see cref="ThreadAutoArchiveDuration"/> cannot be modified. This happens, when the guild hasn't reached a certain boost <see cref="PremiumTier"/>.</exception>
        public Task Modify(Action<ThreadEditModel> Action)
        {
            var mdl = new ThreadEditModel();
            Action(mdl);

            var canContinue = !mdl.AutoArchiveDuration.HasValue || !mdl.AutoArchiveDuration.Value.HasValue || Utilities.CheckThreadAutoArchiveDurationFeature(this.Guild, mdl.AutoArchiveDuration.Value.Value);
            if (mdl.Invitable.HasValue)
            {
                canContinue = this.Guild.Features.CanCreatePrivateThreads;
            }
            return canContinue ? this.Discord.ApiClient.ModifyThread(this.Id, mdl.Name, mdl.Locked, mdl.Archived, mdl.AutoArchiveDuration, mdl.PerUserRateLimit, mdl.Invitable, mdl.AuditLogReason) : throw new NotSupportedException($"Cannot modify ThreadAutoArchiveDuration. Guild needs boost tier {(mdl.AutoArchiveDuration.Value.Value == ThreadAutoArchiveDuration.ThreeDays ? "one" : "two")}.");
        }

        /// <summary>
        /// Archives a thread.
        /// </summary>
        /// <param name="Locked">Whether the thread should be locked.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Archive(bool Locked = true, string Reason = null)
            => this.Discord.ApiClient.ModifyThread(this.Id, null, Locked, true, null, null, null, Reason: Reason);

        /// <summary>
        /// Unarchives a thread.
        /// </summary>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Unarchive(string Reason = null)
            => this.Discord.ApiClient.ModifyThread(this.Id, null, null, false, null, null, null, Reason: Reason);

        /// <summary>
        /// Gets the members of a thread. Needs the <see cref="DiscordIntents.GuildMembers"/> intent.
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyList<DiscordThreadChannelMember>> GetMembersAsync()
            => await this.Discord.ApiClient.GetThreadMembersAsync(this.Id);

        /// <summary>
        /// Adds a member to this thread.
        /// </summary>
        /// <param name="member_id">The member id to be added.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task AddMember(ulong MemberId)
            => this.Discord.ApiClient.AddThreadMemberAsync(this.Id, MemberId);

        /// <summary>
        /// Adds a member to this thread.
        /// </summary>
        /// <param name="Member">The member to be added.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task AddMember(DiscordMember Member)
            => this.AddMember(Member.Id);

        /// <summary>
        /// Gets a member in this thread.
        /// </summary>
        /// <param name="member_id">The member to be added.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member is not part of the thread.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordThreadChannelMember> GetMember(ulong MemberId)
            => this.Discord.ApiClient.GetThreadMemberAsync(this.Id, MemberId);

        /// <summary>
        /// Gets a member in this thread.
        /// </summary>
        /// <param name="Member">The member to be added.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member is not part of the thread.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordThreadChannelMember> GetMember(DiscordMember Member)
            => this.Discord.ApiClient.GetThreadMemberAsync(this.Id, Member.Id);

        /// <summary>
        /// Removes a member from this thread.
        /// </summary>
        /// <param name="member_id">The member id to be removed.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task RemoveMember(ulong MemberId)
            => this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, MemberId);

        /// <summary>
        /// Removes a member from this thread. Only applicable to private threads.
        /// </summary>
        /// <param name="Member">The member to be removed.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task RemoveMember(DiscordMember Member)
            => this.RemoveMember(Member.Id);

        /// <summary>
        /// Adds a role to this thread. Only applicable to private threads.
        /// </summary>
        /// <param name="role_id">The role id to be added.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task AddRoleAsync(ulong RoleId)
        {
            var role = this.Guild.GetRole(RoleId);
            var members = await this.Guild.GetAllMembersAsync();
            var roleMembers = members.Where(M => M.Roles.Contains(role));
            foreach (var member in roleMembers)
            {
                await this.Discord.ApiClient.AddThreadMemberAsync(this.Id, member.Id);
            }
        }

        /// <summary>
        /// Adds a role to this thread. Only applicable to private threads.
        /// </summary>
        /// <param name="Role">The role to be added.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task AddRole(DiscordRole Role)
            => this.AddRoleAsync(Role.Id);

        /// <summary>
        /// Removes a role from this thread. Only applicable to private threads.
        /// </summary>
        /// <param name="role_id">The role id to be removed.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task RemoveRoleAsync(ulong RoleId)
        {
            var role = this.Guild.GetRole(RoleId);
            var members = await this.Guild.GetAllMembersAsync();
            var roleMembers = members.Where(M => M.Roles.Contains(role));
            foreach (var member in roleMembers)
            {
                await this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, member.Id);
            }
        }

        /// <summary>
        /// Removes a role to from thread. Only applicable to private threads.
        /// </summary>
        /// <param name="Role">The role to be removed.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task RemoveRole(DiscordRole Role)
            => this.RemoveRoleAsync(Role.Id);

        /// <summary>
        /// Joins a thread.
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client has no access to this thread.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Join()
            => this.Discord.ApiClient.JoinThreadAsync(this.Id);

        /// <summary>
        /// Leaves a thread.
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client has no access to this thread.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Leave()
            => this.Discord.ApiClient.LeaveThreadAsync(this.Id);


        /// <summary>
        /// Sends a message to this thread.
        /// </summary>
        /// <param name="Content">Content of the message to send.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<DiscordMessage> SendMessage(string Content)
        {
            return !this.IsWriteable()
                ? throw new ArgumentException("Cannot send a text message to a non-thread channel.")
                : this.Discord.ApiClient.CreateMessageAsync(this.Id, Content, null, Sticker: null, ReplyMessageId: null, MentionReply: false, FailOnInvalidReply: false);
        }

        /// <summary>
        /// Sends a message to this thread.
        /// </summary>
        /// <param name="Embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<DiscordMessage> SendMessage(DiscordEmbed Embed)
        {
            return !this.IsWriteable()
                ? throw new ArgumentException("Cannot send a text message to a non-thread channel.")
                : this.Discord.ApiClient.CreateMessageAsync(this.Id, null, new[] { Embed }, Sticker: null, ReplyMessageId: null, MentionReply: false, FailOnInvalidReply: false);
        }

        /// <summary>
        /// Sends a message to this thread.
        /// </summary>
        /// <param name="Content">Content of the message to send.</param>
        /// <param name="Embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<DiscordMessage> SendMessage(string Content, DiscordEmbed Embed)
        {
            return !this.IsWriteable()
                ? throw new ArgumentException("Cannot send a text message to a non-thread channel.")
                : this.Discord.ApiClient.CreateMessageAsync(this.Id, Content, new[] { Embed }, Sticker: null, ReplyMessageId: null, MentionReply: false, FailOnInvalidReply: false);
        }

        /// <summary>
        /// Sends a message to this thread.
        /// </summary>
        /// <param name="Builder">The builder with all the items to thread.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<DiscordMessage> SendMessage(DiscordMessageBuilder Builder)
            => this.Discord.ApiClient.CreateMessageAsync(this.Id, Builder);

        /// <summary>
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="Action">The builder with all the items to send.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<DiscordMessage> SendMessage(Action<DiscordMessageBuilder> Action)
        {
            var builder = new DiscordMessageBuilder();
            Action(builder);

            return !this.IsWriteable()
                ? throw new ArgumentException("Cannot send a text message to a non-text channel.")
                : this.Discord.ApiClient.CreateMessageAsync(this.Id, builder);
        }

        /// <summary>
        /// Returns a specific message
        /// </summary>
        /// <param name="Id">The id of the message</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new async Task<DiscordMessage> GetMessageAsync(ulong Id)
        {
            return this.Discord.Configuration.MessageCacheSize > 0
                && this.Discord is DiscordClient dc
                && dc.MessageCache != null
                && dc.MessageCache.TryGet(Xm => Xm.Id == Id && Xm.ChannelId == this.Id, out var msg)
                ? msg
                : await this.Discord.ApiClient.GetMessageAsync(this.Id, Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a list of messages before a certain message.
        /// <param name="Limit">The amount of messages to fetch.</param>
        /// <param name="Before">Message to fetch before from.</param>
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<IReadOnlyList<DiscordMessage>> GetMessagesBefore(ulong Before, int Limit = 100)
            => this.GetMessagesInternal(Limit, Before, null, null);

        /// <summary>
        /// Returns a list of messages after a certain message.
        /// <param name="Limit">The amount of messages to fetch.</param>
        /// <param name="After">Message to fetch after from.</param>
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<IReadOnlyList<DiscordMessage>> GetMessagesAfter(ulong After, int Limit = 100)
            => this.GetMessagesInternal(Limit, null, After, null);

        /// <summary>
        /// Returns a list of messages around a certain message.
        /// <param name="Limit">The amount of messages to fetch.</param>
        /// <param name="Around">Message to fetch around from.</param>
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<IReadOnlyList<DiscordMessage>> GetMessagesAround(ulong Around, int Limit = 100)
            => this.GetMessagesInternal(Limit, null, null, Around);

        /// <summary>
        /// Returns a list of messages from the last message in the thread.
        /// <param name="Limit">The amount of messages to fetch.</param>
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<IReadOnlyList<DiscordMessage>> GetMessages(int Limit = 100) =>
            this.GetMessagesInternal(Limit, null, null, null);

        /// <summary>
        /// Returns a list of messages
        /// </summary>
        /// <param name="Limit">How many messages should be returned.</param>
        /// <param name="Before">Get messages before snowflake.</param>
        /// <param name="After">Get messages after snowflake.</param>
        /// <param name="Around">Get messages around snowflake.</param>
        private async Task<IReadOnlyList<DiscordMessage>> GetMessagesInternal(int Limit = 100, ulong? Before = null, ulong? After = null, ulong? Around = null)
        {
            if (this.Type != ChannelType.PublicThread && this.Type != ChannelType.PrivateThread && this.Type != ChannelType.NewsThread)
                throw new ArgumentException("Cannot get the messages of a non-thread channel.");

            if (Limit < 0)
                throw new ArgumentException("Cannot get a negative number of messages.");

            if (Limit == 0)
                return Array.Empty<DiscordMessage>();

            //return this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, limit, before, after, around);
            if (Limit > 100 && Around != null)
                throw new InvalidOperationException("Cannot get more than 100 messages around the specified ID.");

            var msgs = new List<DiscordMessage>(Limit);
            var remaining = Limit;
            ulong? last = null;
            var isAfter = After != null;

            int lastCount;
            do
            {
                var fetchSize = remaining > 100 ? 100 : remaining;
                var fetch = await this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, fetchSize, !isAfter ? last ?? Before : null, isAfter ? last ?? After : null, Around).ConfigureAwait(false);

                lastCount = fetch.Count;
                remaining -= lastCount;

                if (!isAfter)
                {
                    msgs.AddRange(fetch);
                    last = fetch.LastOrDefault()?.Id;
                }
                else
                {
                    msgs.InsertRange(0, fetch);
                    last = fetch.FirstOrDefault()?.Id;
                }
            }
            while (remaining > 0 && lastCount > 0);

            return new ReadOnlyCollection<DiscordMessage>(msgs);
        }

        /// <summary>
        /// Deletes multiple messages if they are less than 14 days old.  If they are older, none of the messages will be deleted and you will receive a <see cref="DisCatSharp.Exceptions.BadRequestException"/> error.
        /// </summary>
        /// <param name="Messages">A collection of messages to delete.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new async Task DeleteMessagesAsync(IEnumerable<DiscordMessage> Messages, string Reason = null)
        {
            // don't enumerate more than once
            var msgs = Messages.Where(X => X.Channel.Id == this.Id).Select(X => X.Id).ToArray();
            if (Messages == null || !msgs.Any())
                throw new ArgumentException("You need to specify at least one message to delete.");

            if (msgs.Count() < 2)
            {
                await this.Discord.ApiClient.DeleteMessage(this.Id, msgs.Single(), Reason).ConfigureAwait(false);
                return;
            }

            for (var i = 0; i < msgs.Count(); i += 100)
                await this.Discord.ApiClient.DeleteMessages(this.Id, msgs.Skip(i).Take(100), Reason).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="Message">The message to be deleted.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task DeleteMessage(DiscordMessage Message, string Reason = null)
            => this.Discord.ApiClient.DeleteMessage(this.Id, Message.Id, Reason);


        /// <summary>
        /// Post a typing indicator
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task TriggerTyping()
        {
            return this.Type != ChannelType.PublicThread && this.Type != ChannelType.PrivateThread && this.Type != ChannelType.NewsThread
                ? throw new ArgumentException("Cannot start typing in a non-text channel.")
                : this.Discord.ApiClient.TriggerTyping(this.Id);
        }

        /// <summary>
        /// Returns all pinned messages
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission or the client is missing <see cref="Permissions.ReadMessageHistory"/>.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public new Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages()
        {
            return this.Type != ChannelType.PublicThread && this.Type != ChannelType.PrivateThread && this.Type != ChannelType.News
                ? throw new ArgumentException("A non-thread channel does not have pinned messages.")
                : this.Discord.ApiClient.GetPinnedMessagesAsync(this.Id);
        }

        /// <summary>
        /// Returns a string representation of this thread.
        /// </summary>
        /// <returns>String representation of this thread.</returns>
        public override string ToString()
        {
            var threadchannel = (object)this.Type switch
            {
                ChannelType.NewsThread => $"News thread {this.Name} ({this.Id})",
                ChannelType.PublicThread => $"Thread {this.Name} ({this.Id})",
                ChannelType.PrivateThread => $"Private thread {this.Name} ({this.Id})",
                _ => $"Thread {this.Name} ({this.Id})",
            };
            return threadchannel;
        }
        #endregion

        /// <summary>
        /// Checks whether this <see cref="DiscordThreadChannel"/> is equal to another object.
        /// </summary>
        /// <param name="Obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordThreadChannel"/>.</returns>
        public override bool Equals(object Obj) => this.Equals(Obj as DiscordThreadChannel);

        /// <summary>
        /// Checks whether this <see cref="DiscordThreadChannel"/> is equal to another <see cref="DiscordThreadChannel"/>.
        /// </summary>
        /// <param name="E"><see cref="DiscordThreadChannel"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordThreadChannel"/> is equal to this <see cref="DiscordThreadChannel"/>.</returns>
        public bool Equals(DiscordThreadChannel E) => E is not null && (ReferenceEquals(this, E) || this.Id == E.Id);

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordThreadChannel"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordThreadChannel"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordThreadChannel"/> objects are equal.
        /// </summary>
        /// <param name="E1">First channel to compare.</param>
        /// <param name="E2">Second channel to compare.</param>
        /// <returns>Whether the two channels are equal.</returns>
        public static bool operator ==(DiscordThreadChannel E1, DiscordThreadChannel E2)
        {
            var o1 = E1 as object;
            var o2 = E2 as object;

            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || E1.Id == E2.Id);
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordThreadChannel"/> objects are not equal.
        /// </summary>
        /// <param name="E1">First channel to compare.</param>
        /// <param name="E2">Second channel to compare.</param>
        /// <returns>Whether the two channels are not equal.</returns>
        public static bool operator !=(DiscordThreadChannel E1, DiscordThreadChannel E2)
            => !(E1 == E2);
    }
}
