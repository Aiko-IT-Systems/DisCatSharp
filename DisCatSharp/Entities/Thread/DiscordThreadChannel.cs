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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

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
	public IReadOnlyDictionary<ulong, DiscordThreadChannelMember> ThreadMembers => new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannelMember>(this.ThreadMembersInternal);

	[JsonProperty("thread_member", NullValueHandling = NullValueHandling.Ignore)]
	[JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordThreadChannelMember> ThreadMembersInternal;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordThreadChannel"/> class.
	/// </summary>
	internal DiscordThreadChannel()
	{ }

	#region Methods

	/// <summary>
	/// Deletes a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task DeleteAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteThreadAsync(this.Id, reason);

	/// <summary>
	/// Modifies the current thread.
	/// </summary>
	/// <param name="action">Action to perform on this thread</param>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="System.NotSupportedException">Thrown when the <see cref="ThreadAutoArchiveDuration"/> cannot be modified. This happens, when the guild hasn't reached a certain boost <see cref="PremiumTier"/>.</exception>
	public Task ModifyAsync(Action<ThreadEditModel> action)
	{
		var mdl = new ThreadEditModel();
		action(mdl);

		var canContinue = !mdl.AutoArchiveDuration.HasValue || !mdl.AutoArchiveDuration.Value.HasValue || Utilities.CheckThreadAutoArchiveDurationFeature(this.Guild, mdl.AutoArchiveDuration.Value.Value);
		if (mdl.Invitable.HasValue)
		{
			canContinue = this.Guild.Features.CanCreatePrivateThreads;
		}
		return canContinue ? this.Discord.ApiClient.ModifyThreadAsync(this.Id, mdl.Name, mdl.Locked, mdl.Archived, mdl.PerUserRateLimit, mdl.AutoArchiveDuration, mdl.Invitable, mdl.AuditLogReason) : throw new NotSupportedException($"Cannot modify ThreadAutoArchiveDuration. Guild needs boost tier {(mdl.AutoArchiveDuration.Value.Value == ThreadAutoArchiveDuration.ThreeDays ? "one" : "two")}.");
	}

	/// <summary>
	/// Archives a thread.
	/// </summary>
	/// <param name="locked">Whether the thread should be locked.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ArchiveAsync(bool locked = true, string reason = null)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, null, locked, true, null, null, null, reason: reason);

	/// <summary>
	/// Unarchives a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnarchiveAsync(string reason = null)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, null, null, false, null, null, null, reason: reason);

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
	/// <param name="memberId">The member id to be added.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddMemberAsync(ulong memberId)
		=> this.Discord.ApiClient.AddThreadMemberAsync(this.Id, memberId);

	/// <summary>
	/// Adds a member to this thread.
	/// </summary>
	/// <param name="member">The member to be added.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddMemberAsync(DiscordMember member)
		=> this.AddMemberAsync(member.Id);

	/// <summary>
	/// Gets a member in this thread.
	/// </summary>
	/// <param name="memberId">The member to be added.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member is not part of the thread.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordThreadChannelMember> GetMemberAsync(ulong memberId)
		=> this.Discord.ApiClient.GetThreadMemberAsync(this.Id, memberId);

	/// <summary>
	/// Gets a member in this thread.
	/// </summary>
	/// <param name="member">The member to be added.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member is not part of the thread.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordThreadChannelMember> GetMemberAsync(DiscordMember member)
		=> this.Discord.ApiClient.GetThreadMemberAsync(this.Id, member.Id);

	/// <summary>
	/// Removes a member from this thread.
	/// </summary>
	/// <param name="memberId">The member id to be removed.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveMemberAsync(ulong memberId)
		=> this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, memberId);

	/// <summary>
	/// Removes a member from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="member">The member to be removed.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveMemberAsync(DiscordMember member)
		=> this.RemoveMemberAsync(member.Id);

	/// <summary>
	/// Adds a role to this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="roleId">The role id to be added.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task AddRoleAsync(ulong roleId)
	{
		var role = this.Guild.GetRole(roleId);
		var members = await this.Guild.GetAllMembersAsync();
		var roleMembers = members.Where(m => m.Roles.Contains(role));
		foreach (var member in roleMembers)
		{
			await this.Discord.ApiClient.AddThreadMemberAsync(this.Id, member.Id);
		}
	}

	/// <summary>
	/// Adds a role to this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="role">The role to be added.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddRoleAsync(DiscordRole role)
		=> this.AddRoleAsync(role.Id);

	/// <summary>
	/// Removes a role from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="roleId">The role id to be removed.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task RemoveRoleAsync(ulong roleId)
	{
		var role = this.Guild.GetRole(roleId);
		var members = await this.Guild.GetAllMembersAsync();
		var roleMembers = members.Where(m => m.Roles.Contains(role));
		foreach (var member in roleMembers)
		{
			await this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, member.Id);
		}
	}

	/// <summary>
	/// Removes a role to from thread. Only applicable to private threads.
	/// </summary>
	/// <param name="role">The role to be removed.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveRoleAsync(DiscordRole role)
		=> this.RemoveRoleAsync(role.Id);

	/// <summary>
	/// Joins a thread.
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client has no access to this thread.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task JoinAsync()
		=> this.Discord.ApiClient.JoinThreadAsync(this.Id);

	/// <summary>
	/// Leaves a thread.
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client has no access to this thread.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task LeaveAsync()
		=> this.Discord.ApiClient.LeaveThreadAsync(this.Id);

	/// <summary>
	/// Sends a message to this thread.
	/// </summary>
	/// <param name="content">Content of the message to send.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<DiscordMessage> SendMessageAsync(string content) =>
		!this.IsWritable()
			? throw new ArgumentException("Cannot send a text message to a non-thread channel.")
			: this.Discord.ApiClient.CreateMessageAsync(this.Id, content, null, sticker: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Sends a message to this thread.
	/// </summary>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<DiscordMessage> SendMessageAsync(DiscordEmbed embed) =>
		!this.IsWritable()
			? throw new ArgumentException("Cannot send a text message to a non-thread channel.")
			: this.Discord.ApiClient.CreateMessageAsync(this.Id, null, new[] { embed }, sticker: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Sends a message to this thread.
	/// </summary>
	/// <param name="content">Content of the message to send.</param>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed embed) =>
		!this.IsWritable()
			? throw new ArgumentException("Cannot send a text message to a non-thread channel.")
			: this.Discord.ApiClient.CreateMessageAsync(this.Id, content, new[] { embed }, sticker: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Sends a message to this thread.
	/// </summary>
	/// <param name="builder">The builder with all the items to thread.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<DiscordMessage> SendMessageAsync(DiscordMessageBuilder builder)
		=> this.Discord.ApiClient.CreateMessageAsync(this.Id, builder);

	/// <summary>
	/// Sends a message to this channel.
	/// </summary>
	/// <param name="action">The builder with all the items to send.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<DiscordMessage> SendMessageAsync(Action<DiscordMessageBuilder> action)
	{
		var builder = new DiscordMessageBuilder();
		action(builder);

		return !this.IsWritable()
			? throw new ArgumentException("Cannot send a text message to a non-text channel.")
			: this.Discord.ApiClient.CreateMessageAsync(this.Id, builder);
	}

	/// <summary>
	/// Returns a specific message
	/// </summary>
	/// <param name="id">The id of the message</param>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessagesInThreads"/> permission and <see cref="Permissions.SendTtsMessages"/> if TTS is true or the thread is locked.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new async Task<DiscordMessage> GetMessageAsync(ulong id) =>
		this.Discord.Configuration.MessageCacheSize > 0
		&& this.Discord is DiscordClient dc
		&& dc.MessageCache != null
		&& dc.MessageCache.TryGet(xm => xm.Id == id && xm.ChannelId == this.Id, out var msg)
			? msg
			: await this.Discord.ApiClient.GetMessageAsync(this.Id, id).ConfigureAwait(false);

	/// <summary>
	/// Returns a list of messages before a certain message.
	/// <param name="limit">The amount of messages to fetch.</param>
	/// <param name="before">Message to fetch before from.</param>
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<IReadOnlyList<DiscordMessage>> GetMessagesBeforeAsync(ulong before, int limit = 100)
		=> this.GetMessagesInternalAsync(limit, before, null, null);

	/// <summary>
	/// Returns a list of messages after a certain message.
	/// <param name="limit">The amount of messages to fetch.</param>
	/// <param name="after">Message to fetch after from.</param>
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<IReadOnlyList<DiscordMessage>> GetMessagesAfterAsync(ulong after, int limit = 100)
		=> this.GetMessagesInternalAsync(limit, null, after, null);

	/// <summary>
	/// Returns a list of messages around a certain message.
	/// <param name="limit">The amount of messages to fetch.</param>
	/// <param name="around">Message to fetch around from.</param>
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<IReadOnlyList<DiscordMessage>> GetMessagesAroundAsync(ulong around, int limit = 100)
		=> this.GetMessagesInternalAsync(limit, null, null, around);

	/// <summary>
	/// Returns a list of messages from the last message in the thread.
	/// <param name="limit">The amount of messages to fetch.</param>
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> or the <see cref="Permissions.ReadMessageHistory"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(int limit = 100) =>
		this.GetMessagesInternalAsync(limit, null, null, null);

	/// <summary>
	/// Returns a list of messages
	/// </summary>
	/// <param name="limit">How many messages should be returned.</param>
	/// <param name="before">Get messages before snowflake.</param>
	/// <param name="after">Get messages after snowflake.</param>
	/// <param name="around">Get messages around snowflake.</param>
	private async Task<IReadOnlyList<DiscordMessage>> GetMessagesInternalAsync(int limit = 100, ulong? before = null, ulong? after = null, ulong? around = null)
	{
		if (this.Type != ChannelType.PublicThread && this.Type != ChannelType.PrivateThread && this.Type != ChannelType.NewsThread)
			throw new ArgumentException("Cannot get the messages of a non-thread channel.");

		if (limit < 0)
			throw new ArgumentException("Cannot get a negative number of messages.");

		if (limit == 0)
			return Array.Empty<DiscordMessage>();

		//return this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, limit, before, after, around);
		if (limit > 100 && around != null)
			throw new InvalidOperationException("Cannot get more than 100 messages around the specified ID.");

		var msgs = new List<DiscordMessage>(limit);
		var remaining = limit;
		ulong? last = null;
		var isAfter = after != null;

		int lastCount;
		do
		{
			var fetchSize = remaining > 100 ? 100 : remaining;
			var fetch = await this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, fetchSize, !isAfter ? last ?? before : null, isAfter ? last ?? after : null, around).ConfigureAwait(false);

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
	/// <param name="messages">A collection of messages to delete.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new async Task DeleteMessagesAsync(IEnumerable<DiscordMessage> messages, string reason = null)
	{
		// don't enumerate more than once
		var msgs = messages.Where(x => x.Channel.Id == this.Id).Select(x => x.Id).ToArray();
		if (messages == null || !msgs.Any())
			throw new ArgumentException("You need to specify at least one message to delete.");

		if (msgs.Length < 2)
		{
			await this.Discord.ApiClient.DeleteMessageAsync(this.Id, msgs.Single(), reason).ConfigureAwait(false);
			return;
		}

		for (var i = 0; i < msgs.Length; i += 100)
			await this.Discord.ApiClient.DeleteMessagesAsync(this.Id, msgs.Skip(i).Take(100), reason).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes a message
	/// </summary>
	/// <param name="message">The message to be deleted.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task DeleteMessageAsync(DiscordMessage message, string reason = null)
		=> this.Discord.ApiClient.DeleteMessageAsync(this.Id, message.Id, reason);

	/// <summary>
	/// Post a typing indicator
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task TriggerTypingAsync() =>
		this.Type != ChannelType.PublicThread && this.Type != ChannelType.PrivateThread && this.Type != ChannelType.NewsThread
			? throw new ArgumentException("Cannot start typing in a non-text channel.")
			: this.Discord.ApiClient.TriggerTypingAsync(this.Id);

	/// <summary>
	/// Returns all pinned messages
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AccessChannels"/> permission or the client is missing <see cref="Permissions.ReadMessageHistory"/>.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public new Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync() =>
		this.Type != ChannelType.PublicThread && this.Type != ChannelType.PrivateThread && this.Type != ChannelType.News
			? throw new ArgumentException("A non-thread channel does not have pinned messages.")
			: this.Discord.ApiClient.GetPinnedMessagesAsync(this.Id);

	/// <summary>
	/// Returns a string representation of this thread.
	/// </summary>
	/// <returns>String representation of this thread.</returns>
	public override string ToString()
		=> this.Type switch
		{
			ChannelType.NewsThread => $"News thread {this.Name} ({this.Id})",
			ChannelType.PublicThread => $"Thread {this.Name} ({this.Id})",
			ChannelType.PrivateThread => $"Private thread {this.Name} ({this.Id})",
			_ => $"Thread {this.Name} ({this.Id})",
		};
	#endregion

	/// <summary>
	/// Checks whether this <see cref="DiscordThreadChannel"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordThreadChannel"/>.</returns>
	public override bool Equals(object obj) => this.Equals(obj as DiscordThreadChannel);

	/// <summary>
	/// Checks whether this <see cref="DiscordThreadChannel"/> is equal to another <see cref="DiscordThreadChannel"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordThreadChannel"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordThreadChannel"/> is equal to this <see cref="DiscordThreadChannel"/>.</returns>
	public bool Equals(DiscordThreadChannel e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordThreadChannel"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordThreadChannel"/>.</returns>
	public override int GetHashCode() => this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordThreadChannel"/> objects are equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are equal.</returns>
	public static bool operator ==(DiscordThreadChannel e1, DiscordThreadChannel e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordThreadChannel"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are not equal.</returns>
	public static bool operator !=(DiscordThreadChannel e1, DiscordThreadChannel e2)
		=> !(e1 == e2);
}
