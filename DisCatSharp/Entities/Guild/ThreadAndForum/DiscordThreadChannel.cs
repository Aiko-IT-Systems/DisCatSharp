using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a discord thread channel.
/// </summary>
public class DiscordThreadChannel : DiscordChannel
{
	/// <summary>
	///     List of applied tag ids.
	/// </summary>
	[JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
	internal List<ulong> AppliedTagIdsInternal;

	[JsonProperty("thread_member", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordThreadChannelMember> ThreadMembersInternal;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordThreadChannel" /> class.
	/// </summary>
	internal DiscordThreadChannel()
		: base(["hashes", "guild_hashes"])
	{ }

	/// <summary>
	///     Gets ID of the owner that started this thread.
	/// </summary>
	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong OwnerId { get; internal set; }

	/// <summary>
	///     Gets whether this thread is newly created.
	///     <para>Why? We don't know.</para>
	/// </summary>
	[JsonProperty("newly_created", NullValueHandling = NullValueHandling.Ignore)]
	public bool NewlyCreated { get; internal set; }

	[JsonProperty("total_message_sent", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int TotalMessagesSent { get; internal set; }

	/// <summary>
	///     Gets an approximate count of messages in a thread, stops counting at 50.
	/// </summary>
	[JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? MessageCount { get; internal set; }

	/// <summary>
	///     Gets an approximate count of users in a thread, stops counting at 50.
	/// </summary>
	[JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? MemberCount { get; internal set; }

	/// <summary>
	///     Represents the current member for this thread. This will have a value if the user has joined the thread.
	/// </summary>
	[JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordThreadChannelMember CurrentMember { get; internal set; }

	/// <summary>
	///     Gets the threads metadata.
	/// </summary>
	[JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordThreadChannelMetadata ThreadMetadata { get; internal set; }

	/// <summary>
	///     Gets the thread members object.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordThreadChannelMember> ThreadMembers => new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannelMember>(this.ThreadMembersInternal);

	/// <summary>
	///     List of applied tag ids.
	/// </summary>
	[JsonIgnore]
	internal IReadOnlyList<ulong> AppliedTagIds
		=> this.AppliedTagIdsInternal;

	/// <summary>
	///     Gets the list of applied tags.
	///     Only applicable for forum channel posts.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<ForumPostTag> AppliedTags
		=> this.AppliedTagIds?.Select(id => this.Parent.GetForumPostTag(id)).Where(x => x != null).ToList();

	#region Methods

	/// <summary>
	///     Modifies the current thread.
	/// </summary>
	/// <param name="action">Action to perform on this thread</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageThreads" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="NotSupportedException">
	///     Thrown when the <see cref="ThreadAutoArchiveDuration" /> cannot be modified.
	///     This happens, when the guild hasn't reached a certain boost <see cref="PremiumTier" />.
	/// </exception>
	public Task ModifyAsync(Action<ThreadEditModel> action, CancellationToken cancellationToken = default)
	{
		var mdl = new ThreadEditModel();
		action(mdl);

		return this.Parent.Type == ChannelType.Forum && mdl.AppliedTags.HasValue && mdl.AppliedTags.Value.Count() > 5
			? throw new NotSupportedException("Cannot have more than 5 applied tags.")
			: this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, mdl.Name, mdl.Locked, mdl.Archived, mdl.PerUserRateLimit, mdl.AutoArchiveDuration, mdl.Invitable, mdl.AppliedTags, mdl.Pinned, mdl.AuditLogReason, cancellationToken: cancellationToken);
	}

	/// <summary>
	///     Add a tag to the current thread.
	/// </summary>
	/// <param name="tag">The tag to add.</param>
	/// <param name="reason">The reason for the audit logs.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageThreads" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddTagAsync(ForumPostTag tag, string reason = null, CancellationToken cancellationToken = default)
		=> this.AppliedTagIds.Count == 5
			? throw new NotSupportedException("Cannot have more than 5 applied tags.")
			: this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, null, null, null, null, new List<ForumPostTag>(this.AppliedTags)
			{
				tag
			}, null, reason, cancellationToken: cancellationToken);

	/// <summary>
	///     Remove a tag from the current thread.
	/// </summary>
	/// <param name="tag">The tag to remove.</param>
	/// <param name="reason">The reason for the audit logs.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageThreads" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task RemoveTagAsync(ForumPostTag tag, string reason = null, CancellationToken cancellationToken = default)
		=> await this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, null, null, null, null, new List<ForumPostTag>(this.AppliedTags).Where(x => x != tag).ToList(), null, reason, cancellationToken: cancellationToken).ConfigureAwait(false);

	/// <summary>
	///     Archives a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageThreads" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ArchiveAsync(string reason = null, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, true, null, null, null, null, null, reason, cancellationToken: cancellationToken);

	/// <summary>
	///     Unarchives a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnarchiveAsync(string reason = null, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, null, false, null, null, null, null, null, reason, cancellationToken: cancellationToken);

	/// <summary>
	///     Locks a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageThreads" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task LockAsync(string reason = null, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, true, null, null, null, null, null, null, reason, cancellationToken: cancellationToken);

	/// <summary>
	///     Unlocks a thread.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnlockAsync(string reason = null, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.ModifyThreadAsync(this.Id, this.Parent.Type, null, false, true, null, null, null, null, null, reason, cancellationToken: cancellationToken);

	/// <summary>
	///     Gets the members of a thread. Needs the <see cref="DiscordIntents.GuildMembers" /> intent.
	/// </summary>
	/// <param name="withMember">Whether to request the member object. (If set to true, will paginate the result)</param>
	/// <param name="after">Request all members after the specified. (Currently only utilized if withMember is set to true)</param>
	/// <param name="limit">The amount of members to fetch. (Currently only utilized if withMember is set to true)</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyList<DiscordThreadChannelMember>> GetMembersAsync(bool withMember = false, ulong? after = null, int? limit = null, CancellationToken cancellationToken = default)
		=> await this.Discord.ApiClient.GetThreadMembersAsync(this.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

	/// <summary>
	///     Adds a member to this thread.
	/// </summary>
	/// <param name="memberId">The member id to be added.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddMemberAsync(ulong memberId, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.AddThreadMemberAsync(this.Id, memberId, cancellationToken: cancellationToken);

	/// <summary>
	///     Adds a member to this thread.
	/// </summary>
	/// <param name="member">The member to be added.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddMemberAsync(DiscordMember member, CancellationToken cancellationToken = default)
		=> this.AddMemberAsync(member.Id, cancellationToken);

	/// <summary>
	///     Gets a member in this thread.
	/// </summary>
	/// <param name="memberId">The id of the member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the member is not part of the thread.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordThreadChannelMember> GetMemberAsync(ulong memberId, bool withMember = false, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.GetThreadMemberAsync(this.Id, memberId, withMember, cancellationToken: cancellationToken);

	/// <summary>
	///     Tries to get a member in this thread.
	/// </summary>
	/// <param name="memberId">The id of the member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordThreadChannelMember?> TryGetMemberAsync(ulong memberId, bool withMember = false, CancellationToken cancellationToken = default)
	{
		try
		{
			return await this.GetMemberAsync(memberId, withMember, cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Gets a member in this thread.
	/// </summary>
	/// <param name="member">The member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the member is not part of the thread.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordThreadChannelMember> GetMemberAsync(DiscordMember member, bool withMember = false, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.GetThreadMemberAsync(this.Id, member.Id, withMember, cancellationToken: cancellationToken);

	/// <summary>
	///     Tries to get a member in this thread.
	/// </summary>
	/// <param name="member">The member to get.</param>
	/// <param name="withMember">Whether to request the member object.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordThreadChannelMember?> TryGetMemberAsync(DiscordMember member, bool withMember = false, CancellationToken cancellationToken = default)
	{
		try
		{
			return await this.GetMemberAsync(member, withMember, cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Removes a member from this thread.
	/// </summary>
	/// <param name="memberId">The member id to be removed.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveMemberAsync(ulong memberId, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, memberId, cancellationToken: cancellationToken);

	/// <summary>
	///     Removes a member from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="member">The member to be removed.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveMemberAsync(DiscordMember member, CancellationToken cancellationToken = default)
		=> this.RemoveMemberAsync(member.Id, cancellationToken);

	/// <summary>
	///     Adds a role to this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="roleId">The role id to be added.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task AddRoleAsync(ulong roleId, CancellationToken cancellationToken = default)
	{
		var role = this.Guild.GetRole(roleId);
		var members = await this.Guild.GetAllMembersAsync(cancellationToken).ConfigureAwait(false);
		var roleMembers = members.Where(m => m.Roles.Contains(role));
		foreach (var member in roleMembers)
			await this.Discord.ApiClient.AddThreadMemberAsync(this.Id, member.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Adds a role to this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="role">The role to be added.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddRoleAsync(DiscordRole role, CancellationToken cancellationToken = default)
		=> this.AddRoleAsync(role.Id, cancellationToken);

	/// <summary>
	///     Removes a role from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="roleId">The role id to be removed.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task RemoveRoleAsync(ulong roleId, CancellationToken cancellationToken = default)
	{
		var role = this.Guild.GetRole(roleId);
		var members = await this.Guild.GetAllMembersAsync(cancellationToken).ConfigureAwait(false);
		var roleMembers = members.Where(m => m.Roles.Contains(role));
		foreach (var member in roleMembers)
			await this.Discord.ApiClient.RemoveThreadMemberAsync(this.Id, member.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Removes a role from this thread. Only applicable to private threads.
	/// </summary>
	/// <param name="role">The role to be removed.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveRoleAsync(DiscordRole role, CancellationToken cancellationToken = default)
		=> this.RemoveRoleAsync(role.Id, cancellationToken);

	/// <summary>
	///     Joins a thread.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client has no access to this thread.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task JoinAsync(CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.JoinThreadAsync(this.Id, cancellationToken: cancellationToken);

	/// <summary>
	///     Leaves a thread.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client has no access to this thread.</exception>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task LeaveAsync(CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.LeaveThreadAsync(this.Id, cancellationToken: cancellationToken);

	/// <summary>
	///     Returns a string representation of this thread.
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
