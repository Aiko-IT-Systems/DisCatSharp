using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord thread channel.
/// </summary>
public class DiscordThreadChannel : DiscordChannel
{
	/// <summary>
	/// Gets ID of the owner that started this thread.
	/// </summary>
	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong OwnerId { get; internal set; }

	/// <summary>
	/// Gets whether this thread is newly created.
	/// <para>Why? We don't know.</para>
	/// </summary>
	[JsonProperty("newly_created", NullValueHandling = NullValueHandling.Ignore)]
	public bool NewlyCreated { get; internal set; }

	[JsonProperty("total_message_sent", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int TotalMessagesSent { get; internal set; }

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
	/// Gets the threads metadata.
	/// </summary>
	[JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordThreadChannelMetadata ThreadMetadata { get; internal set; }

	/// <summary>
	/// Gets the thread members object.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordThreadChannelMember> ThreadMembers => new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannelMember>(this.ThreadMembersInternal);

	[JsonProperty("thread_member", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordThreadChannelMember> ThreadMembersInternal;

	/// <summary>
	/// List of applied tag ids.
	/// </summary>
	[JsonIgnore]
	internal IReadOnlyList<ulong> AppliedTagIds
		=> this.AppliedTagIdsInternal;

	/// <summary>
	/// List of applied tag ids.
	/// </summary>
	[JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
	internal List<ulong> AppliedTagIdsInternal;

	/// <summary>
	/// Gets the list of applied tags.
	/// Only applicable for forum channel posts.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<ForumPostTag> AppliedTags
		=> this.AppliedTagIds?.Select(id => this.Parent.GetForumPostTag(id)).Where(x => x != null).ToList();

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordThreadChannel"/> class.
	/// </summary>
	internal DiscordThreadChannel()
		: base(["hashes", "guild_hashes"])
	{ }

#region Methods

	/// <summary>
	/// Modifies the current thread.
	/// </summary>
	/// <param name="action">Action to perform on this thread</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="NotSupportedException">Thrown when the <see cref="ThreadAutoArchiveDuration"/> cannot be modified. This happens, when the guild hasn't reached a certain boost <see cref="PremiumTier"/>.</exception>
	public Task ModifyAsync(Action<ThreadEditModel> action)
	{
		var mdl = new ThreadEditModel();
		action(mdl);

		return this.Parent.Type == ChannelType.Forum && mdl.AppliedTags.HasValue && mdl.AppliedTags.Value.Count() > 5
			? throw new NotSupportedException("Cannot have more than 5 applied tags.")
			: this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, mdl.Name, mdl.Locked, mdl.Archived, mdl.PerUserRateLimit, mdl.AutoArchiveDuration, mdl.Invitable, mdl.AppliedTags, mdl.Pinned, mdl.AuditLogReason);
	}

	/// <summary>
	/// Add a tag to the current thread.
	/// </summary>
	/// <param name="tag">The tag to add.</param>
	/// <param name="reason">The reason for the audit logs.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddTagAsync(ForumPostTag tag, string reason = null)
		=> this.AppliedTagIds.Count == 5
			? throw new NotSupportedException("Cannot have more than 5 applied tags.")
			: this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, null, null, null, null, new List<ForumPostTag>(this.AppliedTags)
			{
				tag
			}, null, reason);

	/// <summary>
	/// Remove a tag from the current thread.
	/// </summary>
	/// <param name="tag">The tag to remove.</param>
	/// <param name="reason">The reason for the audit logs.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task RemoveTagAsync(ForumPostTag tag, string reason = null)
		=> await this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, null, null, null, null, new List<ForumPostTag>(this.AppliedTags).Where(x => x != tag).ToList(), null, reason).ConfigureAwait(false);

	/// <summary>
	/// Archives a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ArchiveAsync(string reason = null)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, true, null, null, null, null, null, reason);

	/// <summary>
	/// Unarchives a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnarchiveAsync(string reason = null)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, false, null, null, null, null, null, reason);

	/// <summary>
	/// Locks a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task LockAsync(string reason = null)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, true, null, null, null, null, null, null, reason);

	/// <summary>
	/// Unlocks a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnlockAsync(string reason = null)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, false, true, null, null, null, null, null, reason);

	/// <summary>
	/// Gets the members of a thread. Needs the <see cref="DiscordIntents.GuildMembers"/> intent.
	/// </summary>
	/// <param name="withMember">Whether to request the member object. (If set to true, will paginate the result)</param>
	/// <param name="after">Request all members after the specified. (Currently only utilized if withMember is set to true)</param>
	/// <param name="limit">The amount of members to fetch. (Currently only utilized if withMember is set to true)</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyList<DiscordThreadChannelMember>> GetMembersAsync(bool withMember = false, ulong? after = null, int? limit = null)
		=> await this.Discord.ApiClient.GetThreadMembersAsync(this.Id).ConfigureAwait(false);

	/// <summary>
	/// Adds a member to this thread.
	/// </summary>
	/// <param name="memberId">The member id to be added.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddMemberAsync(ulong memberId)
		=> this.Discord.ApiClient.AddThreadMemberAsync(this.Id, memberId);

	/// <summary>
	/// Adds a member to this thread.
	/// </summary>
	/// <param name="member">The member to be added.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddMemberAsync(DiscordMember member)
		=> this.AddMemberAsync(member.Id);

	/// <summary>
	/// Gets a member in this thread.
	/// </summary>
	/// <param name="memberId">The id of the member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <exception cref="NotFoundException">Thrown when the member is not part of the thread.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordThreadChannelMember> GetMemberAsync(ulong memberId, bool withMember = false)
		=> this.Discord.ApiClient.GetThreadMemberAsync(this.Id, memberId, withMember);

	/// <summary>
	/// Tries to get a member in this thread.
	/// </summary>
	/// <param name="memberId">The id of the member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordThreadChannelMember?> TryGetMemberAsync(ulong memberId, bool withMember = false)
	{
		try
		{
			return await this.GetMemberAsync(memberId, withMember).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	/// Gets a member in this thread.
	/// </summary>
	/// <param name="member">The member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <exception cref="NotFoundException">Thrown when the member is not part of the thread.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordThreadChannelMember> GetMemberAsync(DiscordMember member, bool withMember = false)
		=> this.Discord.ApiClient.GetThreadMemberAsync(this.Id, member.Id, withMember);

	/// <summary>
	/// Tries to get a member in this thread.
	/// </summary>
	/// <param name="member">The member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordThreadChannelMember?> TryGetMemberAsync(DiscordMember member, bool withMember = false)
	{
		try
		{
			return await this.GetMemberAsync(member, withMember).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	/// Removes a member from this thread.
	/// </summary>
	/// <param name="memberId">The member id to be removed.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveMemberAsync(ulong memberId)
		=> this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, memberId);

	/// <summary>
	/// Removes a member from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="member">The member to be removed.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveMemberAsync(DiscordMember member)
		=> this.RemoveMemberAsync(member.Id);

	/// <summary>
	/// Adds a role to this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="roleId">The role id to be added.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task AddRoleAsync(ulong roleId)
	{
		var role = this.Guild.GetRole(roleId);
		var members = await this.Guild.GetAllMembersAsync().ConfigureAwait(false);
		var roleMembers = members.Where(m => m.Roles.Contains(role));
		foreach (var member in roleMembers)
			await this.Discord.ApiClient.AddThreadMemberAsync(this.Id, member.Id).ConfigureAwait(false);
	}

	/// <summary>
	/// Adds a role to this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="role">The role to be added.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddRoleAsync(DiscordRole role)
		=> this.AddRoleAsync(role.Id);

	/// <summary>
	/// Removes a role from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="roleId">The role id to be removed.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task RemoveRoleAsync(ulong roleId)
	{
		var role = this.Guild.GetRole(roleId);
		var members = await this.Guild.GetAllMembersAsync().ConfigureAwait(false);
		var roleMembers = members.Where(m => m.Roles.Contains(role));
		foreach (var member in roleMembers)
			await this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, member.Id).ConfigureAwait(false);
	}

	/// <summary>
	/// Removes a role from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="role">The role to be removed.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveRoleAsync(DiscordRole role)
		=> this.RemoveRoleAsync(role.Id);

	/// <summary>
	/// Joins a thread.
	/// </summary>
	/// <exception cref="UnauthorizedException">Thrown when the client has no access to this thread.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task JoinAsync()
		=> this.Discord.ApiClient.JoinThreadAsync(this.Id);

	/// <summary>
	/// Leaves a thread.
	/// </summary>
	/// <exception cref="UnauthorizedException">Thrown when the client has no access to this thread.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task LeaveAsync()
		=> this.Discord.ApiClient.LeaveThreadAsync(this.Id);

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
			_ => $"Thread {this.Name} ({this.Id})"
		};

#endregion
}
