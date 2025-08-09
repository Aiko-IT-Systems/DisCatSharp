using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord guild member.
/// </summary>
public class DiscordMember : DiscordUser, IEquatable<DiscordMember>
{
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<ulong>> _roleIdsLazy;

	/// <summary>
	///     Gets the avatar hash.
	/// </summary>
	[JsonIgnore]
	internal string? AvatarHashInternal;

	/// <summary>
	///     Gets the guild id for this member.
	/// </summary>
	[JsonIgnore]
	public ulong GuildId = 0;

	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	internal List<ulong> RoleIdsInternal = [];

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordMember" /> class.
	/// </summary>
	internal DiscordMember()
	{
		this._roleIdsLazy = new(() => new ReadOnlyCollection<ulong>(this.RoleIdsInternal));
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordMember" /> class.
	/// </summary>
	/// <param name="user">The user.</param>
	internal DiscordMember(DiscordUser user)
	{
		this.Discord = user.Discord;

		this.Id = user.Id;

		this.ManualUser = user;

		this.RoleIdsInternal = [];
		this._roleIdsLazy = new(() => new ReadOnlyCollection<ulong>(this.RoleIdsInternal));
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordMember" /> class.
	/// </summary>
	/// <param name="mbr">The mbr.</param>
	internal DiscordMember(TransportMember mbr)
	{
		this.Id = mbr.User!.Id;
		this.IsDeafened = mbr.IsDeafened;
		this.IsMuted = mbr.IsMuted;
		this.JoinedAt = mbr.JoinedAt;
		this.Nickname = mbr.Nickname;
		this.PremiumSince = mbr.PremiumSince;
		this.IsPending = mbr.IsPending;
		this.GuildAvatarHash = mbr.GuildAvatarHash;
		this.GuildBannerHash = mbr.GuildBannerHash;
		this.GuildBio = mbr.GuildBio;
		this.GuildPronouns = mbr.GuildPronouns;
		this.CommunicationDisabledUntil = mbr.CommunicationDisabledUntil;
		this.UnusualDmActivityUntil = mbr.UnusualDmActivityUntil;
		this.AvatarHashInternal = mbr.AvatarHash;
		this.RoleIdsInternal = mbr.Roles ?? [];
		this._roleIdsLazy = new(() => new ReadOnlyCollection<ulong>(this.RoleIdsInternal));
		this.MemberFlags = mbr.MemberFlags;
		this.InteractionPermissions = mbr.Permissions;
		this.GuildAvatarDecorationData = mbr.GuildAvatarDecorationData;
		if (mbr.User is not null)
			this.ManualUser = new(mbr.User)
			{
				Discord = this.Discord
			};
	}

	/// <summary>
	///     Gets the members avatar hash.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	public string? GuildAvatarHash { get; internal set; }

	/// <summary>
	///     Gets the members avatar URL.
	/// </summary>
	[JsonIgnore]
	public string GuildAvatarUrl
		=> string.IsNullOrWhiteSpace(this.GuildAvatarHash) ? this.User.AvatarUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GUILDS}/{this.GuildId.ToString(CultureInfo.InvariantCulture)}{Endpoints.USERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.AVATARS}/{this.GuildAvatarHash}.{(this.GuildAvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	/// <summary>
	///     Gets the members banner hash.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public string? GuildBannerHash { get; internal set; }

	/// <summary>
	///     Gets the members banner URL.
	/// </summary>
	[JsonIgnore]
	public string? GuildBannerUrl
		=> string.IsNullOrWhiteSpace(this.GuildBannerHash) ? this.User.BannerUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GUILDS}/{this.GuildId.ToString(CultureInfo.InvariantCulture)}{Endpoints.USERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.BANNERS}/{this.GuildBannerHash}.{(this.GuildBannerHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	/// <summary>
	///     Gets the members guild avatar decoration data.
	/// </summary>
	[JsonProperty("avatar_decoration_data", NullValueHandling = NullValueHandling.Ignore)]
	public AvatarDecorationData GuildAvatarDecorationData { get; internal set; }

	/// <summary>
	///     The color of this member's banner. Mutually exclusive with <see cref="GuildBannerHash" />.
	/// </summary>
	[JsonIgnore]
	public override DiscordColor? BannerColor
		=> this.User.BannerColor;

	/// <summary>
	///     Gets this member's nickname.
	/// </summary>
	[JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
	public string? Nickname { get; internal set; }

	/// <summary>
	///     Gets the members guild bio.
	///     This is not available to bots tho.
	/// </summary>
	[JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
	public string? GuildBio { get; internal set; }

	/// <summary>
	///     Gets the members's pronouns.
	/// </summary>
	[JsonProperty("pronouns", NullValueHandling = NullValueHandling.Ignore)]
	public string? GuildPronouns { get; internal set; }

	/// <summary>
	///     Gets the members flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MemberFlags MemberFlags { get; internal set; }

	/// <summary>
	///     Gets this member's display name.
	/// </summary>
	[JsonIgnore]
	public string DisplayName
		=> this.Nickname ?? (this.IsMigrated ? this.GlobalName ?? this.Username : this.Username);

	/// <summary>
	///     List of role ids
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<ulong> RoleIds
		=> this._roleIdsLazy.Value;

	/// <summary>
	///     Gets the list of roles associated with this member.
	///     <note type="warning">This will throw if accessed for an oauth2 constructed member.</note>
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordRole> Roles
		=> this.RoleIds.Select(id => this.Guild.GetRole(id)).Where(x => x is not null).ToList()!;

	/// <summary>
	///     Gets the color associated with this user's top color-giving role, otherwise 0 (no color).
	///     <note type="warning">This will throw if accessed for an oauth2 constructed member.</note>
	/// </summary>
	[JsonIgnore]
	public DiscordColor Color
	{
		get
		{
			var role = this.Roles.OrderByDescending(xr => xr.Position).FirstOrDefault(xr => xr.Color.Value is not 0);
			return role?.Color ?? new();
		}
	}

	/// <summary>
	///     Date the user joined the guild
	/// </summary>
	[JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset JoinedAt { get; internal set; }

	/// <summary>
	///     Date the user started boosting this server
	/// </summary>
	[JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? PremiumSince { get; internal set; }

	/// <summary>
	///     Date until the can communicate again.
	/// </summary>
	[JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Include)]
	public DateTime? CommunicationDisabledUntil { get; internal set; }

	/// <summary>
	///     Datetime until unusual dm activity time happened.
	/// </summary>
	[JsonProperty("unusual_dm_activity_until", NullValueHandling = NullValueHandling.Include)]
	public DateTime? UnusualDmActivityUntil { get; internal set; }

	/// <summary>
	///     Whether the user has their communication disabled.
	/// </summary>
	[JsonIgnore]
	public bool IsCommunicationDisabled
		=> this.CommunicationDisabledUntil is not null && this.CommunicationDisabledUntil.Value.ToUniversalTime() > DateTime.UtcNow;

	/// <summary>
	///     Whether the user has their communication disabled.
	/// </summary>
	[JsonIgnore]
	public bool HasUnusualDmActivity
		=> this.UnusualDmActivityUntil is not null && this.UnusualDmActivityUntil.Value.ToUniversalTime() > DateTime.UtcNow;

	/// <summary>
	///     If the user is deafened
	/// </summary>
	[JsonProperty("is_deafened", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsDeafened { get; internal set; }

	/// <summary>
	///     If the user is muted
	/// </summary>
	[JsonProperty("is_muted", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsMuted { get; internal set; }

	/// <summary>
	///     Whether the user has not passed the guild's Membership Screening requirements yet.
	/// </summary>
	[JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsPending { get; internal set; }

	/// <summary>
	///     Gets this member's voice state.
	///     <note type="warning">This will throw if accessed for an oauth2 constructed member.</note>
	/// </summary>
	[JsonIgnore]
	public DiscordVoiceState? VoiceState
		=> this.Discord.Guilds[this.GuildId].VoiceStates.GetValueOrDefault(this.Id);

	/// <summary>
	///     Gets the guild of which this member is a part of.
	///     <note type="warning">This will throw if accessed for an oauth2 constructed member.</note>
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds[this.GuildId];

	/// <summary>
	///     Gets whether this member is the Guild owner.
	///     <note type="warning">This will throw if accessed for an oauth2 constructed member.</note>
	/// </summary>
	[JsonIgnore]
	public bool IsOwner
		=> this.Id == this.Guild.OwnerId;

	/// <summary>
	///     Gets the member's position in the role hierarchy, which is the member's highest role's position. Returns
	///     <see cref="int.MaxValue" /> for the guild's owner.
	/// </summary>
	[JsonIgnore]
	public int Hierarchy
		=> this.IsOwner
			? int.MaxValue
			: this.RoleIds.Count is 0
				? 0
				: this.Roles.Max(x => x.Position);

	/// <summary>
	///     Gets the permissions for the current member.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? InteractionPermissions { get; internal set; }

	/// <summary>
	///     Gets the permissions for the current member.
	///     <note type="warning">This will throw if accessed for an oauth2 constructed member.</note>
	/// </summary>
	[JsonIgnore]
	public Permissions Permissions
		=> this.InteractionPermissions ?? this.GetPermissions();

	/// <inheritdoc />
	public override DisplayNameStyles? DisplayNameStyles
		=> this.User.DisplayNameStyles;

	/// <summary>
	///     Checks whether this <see cref="DiscordMember" /> is equal to another <see cref="DiscordMember" />.
	/// </summary>
	/// <param name="e"><see cref="DiscordMember" /> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordMember" /> is equal to this <see cref="DiscordMember" />.</returns>
	public bool Equals(DiscordMember e)
		=> e is not null && (ReferenceEquals(this, e) || (this.Id == e.Id && this.GuildId == e.GuildId));

	/// <summary>
	///     Sets this member's voice mute status.
	/// </summary>
	/// <param name="mute">Whether the member is to be muted.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.MuteMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task SetMuteAsync(bool mute, string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildMemberAsync(this.GuildId, this.Id, default, default, mute, default, default, default, this.MemberFlags, reason);

	/// <summary>
	///     Manually verifies the member to bypass the Membership Screening requirements.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	public Task VerifyAsync(string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildMemberAsync(this.GuildId, this.Id, default, default, default, default, default, true, this.MemberFlags, reason);

	/// <summary>
	///     Manually unverifies the member to not bypass the Membership Screening requirements anymore.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	public Task UnverifyAsync(string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildMemberAsync(this.GuildId, this.Id, default, default, default, default, default, false, this.MemberFlags, reason);

	/// <summary>
	///     Sets this member's voice deaf status.
	/// </summary>
	/// <param name="deaf">Whether the member is to be deafened.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.DeafenMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task SetDeafAsync(bool deaf, string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildMemberAsync(this.GuildId, this.Id, default, default, default, deaf, default, default, this.MemberFlags, reason);

	/// <summary>
	///     Modifies this member.
	/// </summary>
	/// <param name="action">Action to perform on this member.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageNicknames" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task ModifyAsync(Action<MemberEditModel> action)
	{
		var mdl = new MemberEditModel();
		action(mdl);

		if (mdl.VoiceChannel.HasValue && mdl.VoiceChannel.Value != null! && mdl.VoiceChannel.Value.Type is not ChannelType.Voice && mdl.VoiceChannel.Value.Type is not ChannelType.Stage)
			throw new ArgumentException("Given channel is not a voice or stage channel.", nameof(action));

		if (mdl.Nickname.HasValue && this.Discord.CurrentUser.Id == this.Id)
		{
			await this.Discord.ApiClient.ModifyCurrentMemberNicknameAsync(this.Guild.Id, mdl.Nickname.Value,
				mdl.AuditLogReason).ConfigureAwait(false);

			await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, Optional.None,
				mdl.Roles.Map(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
				mdl.VoiceChannel.Map(e => e?.Id), default, this.MemberFlags, mdl.AuditLogReason).ConfigureAwait(false);
		}
		else
			await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, mdl.Nickname,
				mdl.Roles.Map(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
				mdl.VoiceChannel.Map(e => e?.Id), default, this.MemberFlags, mdl.AuditLogReason).ConfigureAwait(false);
	}

	/// <summary>
	///     Disconnects the member from their current voice channel.
	/// </summary>
	public async Task DisconnectFromVoiceAsync()
		=> await this.ModifyAsync(x => x.VoiceChannel = null!).ConfigureAwait(false);

	/// <summary>
	///     Adds a timeout to a member.
	/// </summary>
	/// <param name="until">The datetime offset to time out the user. Up to 28 days.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task TimeoutAsync(DateTimeOffset until, string? reason = null)
		=> until.Subtract(DateTimeOffset.UtcNow).Days > 28 ? throw new ArgumentException("Timeout can not be longer than 28 days") : this.Discord.ApiClient.ModifyTimeoutAsync(this.Guild.Id, this.Id, until, reason);

	/// <summary>
	///     Adds a timeout to a member.
	/// </summary>
	/// <param name="until">The timespan to time out the user. Up to 28 days.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task TimeoutAsync(TimeSpan until, string? reason = null)
		=> this.TimeoutAsync(DateTimeOffset.UtcNow + until, reason);

	/// <summary>
	///     Adds a timeout to a member.
	/// </summary>
	/// <param name="until">The datetime to time out the user. Up to 28 days.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task TimeoutAsync(DateTime until, string? reason = null)
		=> this.TimeoutAsync(until.ToUniversalTime() - DateTime.UtcNow, reason);

	/// <summary>
	///     Removes the timeout from a member.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveTimeoutAsync(string reason = null)
		=> this.Discord.ApiClient.ModifyTimeoutAsync(this.Guild.Id, this.Id, null, reason);

	/// <summary>
	///     Grants a role to the member.
	/// </summary>
	/// <param name="role">Role to grant.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageRoles" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task GrantRoleAsync(DiscordRole role, string? reason = null)
		=> this.Discord.ApiClient.AddGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

	/// <summary>
	///     Revokes a role from a member.
	/// </summary>
	/// <param name="role">Role to revoke.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageRoles" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RevokeRoleAsync(DiscordRole role, string? reason = null)
		=> this.Discord.ApiClient.RemoveGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

	/// <summary>
	///     Sets the member's roles to ones specified.
	/// </summary>
	/// <param name="roles">Roles to set.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageRoles" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ReplaceRolesAsync(IEnumerable<DiscordRole> roles, string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, default,
			Optional.Some(roles.Select(xr => xr.Id)), default, default, default, default, this.MemberFlags, reason);

	/// <summary>
	///     Bans this member from their guild.
	/// </summary>
	/// <param name="deleteMessageDays">How many days to remove messages from.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task BanAsync(int deleteMessageDays = 0, string? reason = null)
		=> this.Guild.BanMemberAsync(this, deleteMessageDays, reason);

	/// <summary>
	///     Unbans this member from their guild.
	/// </summary>
	/// <exception cref="Exceptions.UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.BanMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnbanAsync(string? reason = null)
		=> this.Guild.UnbanMemberAsync(this, reason);

	/// <summary>
	///     Kicks this member from their guild.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns></returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.KickMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveAsync(string? reason = null)
		=> this.Discord.ApiClient.RemoveGuildMemberAsync(this.GuildId, this.Id, reason);

	/// <summary>
	///     Moves this member to the specified voice channel
	/// </summary>
	/// <param name="channel"></param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.MoveMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task PlaceInAsync(DiscordChannel channel)
		=> channel.PlaceMemberAsync(this);

	/// <summary>
	///     Gets the member's voice state.
	/// </summary>
	/// <returns></returns>
	public async Task<DiscordVoiceState?> GetVoiceStateAsync()
		=> await this.Discord.ApiClient.GetMemberVoiceStateAsync(this.Guild.Id, this.Id);

	/// <summary>
	///     Updates the member's suppress state in a stage channel.
	/// </summary>
	/// <param name="channel">The channel the member is currently in.</param>
	/// <param name="suppress">Toggles the member's suppress state.</param>
	/// <exception cref="ArgumentException">Thrown when the channel in not a voice channel.</exception>
	public async Task UpdateVoiceStateAsync(DiscordChannel channel, bool? suppress)
	{
		if (channel.Type != ChannelType.Stage)
			throw new ArgumentException("Voice state can only be updated in a stage channel.");

		await this.Discord.ApiClient.UpdateUserVoiceStateAsync(this.Guild.Id, this.Id, channel.Id, suppress).ConfigureAwait(false);
	}

	/// <summary>
	///     Makes the user a speaker.
	/// </summary>
	/// <exception cref="ArgumentException">Thrown when the user is not inside an stage channel.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.MuteMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task MakeSpeakerAsync()
	{
		var vs = this.VoiceState;
		if (vs?.Channel?.Type is not ChannelType.Stage)
			throw new ArgumentException("Voice state can only be updated when the user is inside an stage channel.");

		await this.Discord.ApiClient.UpdateUserVoiceStateAsync(this.Guild.Id, this.Id, vs.Channel.Id, false).ConfigureAwait(false);
	}

	/// <summary>
	///     Moves the user to audience.
	/// </summary>
	/// <exception cref="ArgumentException">Thrown when the user is not inside an stage channel.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.MuteMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task MoveToAudienceAsync()
	{
		var vs = this.VoiceState;
		if (vs?.Channel?.Type is not ChannelType.Stage)
			throw new ArgumentException("Voice state can only be updated when the user is inside an stage channel.");

		await this.Discord.ApiClient.UpdateUserVoiceStateAsync(this.Guild.Id, this.Id, vs.Channel.Id, true).ConfigureAwait(false);
	}

	/// <summary>
	///     Calculates permissions in a given channel for this member.
	/// </summary>
	/// <param name="channel">Channel to calculate permissions for.</param>
	/// <returns>Calculated permissions for this member in the channel.</returns>
	public Permissions PermissionsIn(DiscordChannel channel)
		=> channel.PermissionsFor(this);

	/// <summary>
	///     Get's the current member's roles based on the sum of the permissions of their given roles.
	/// </summary>
	private Permissions GetPermissions()
	{
		if (this.Guild.OwnerId == this.Id)
			return PermissionMethods.FullPerms;

		// assign @everyone permissions
		var everyoneRole = this.Guild.EveryoneRole!;
		var perms = everyoneRole.Permissions;

		// assign permissions from member's roles (in order)
		perms |= this.Roles.Aggregate(Permissions.None, (c, role) => c | role.Permissions);

		// Administrator grants all permissions and cannot be overridden
		return (perms & Permissions.Administrator) == Permissions.Administrator ? PermissionMethods.FullPerms : perms;
	}

	/// <summary>
	///     Returns a string representation of this member.
	/// </summary>
	/// <returns>String representation of this member.</returns>
	public override string ToString()
		=> this.IsMigrated ? $"Member {this.Id}; {this.UsernameWithGlobalName}" : $"Member {this.Id}; {this.UsernameWithDiscriminator}";

	/// <summary>
	///     Checks whether this <see cref="DiscordMember" /> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordMember" />.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordMember);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordMember" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordMember" />.</returns>
	public override int GetHashCode()
	{
		var hash = 13;

		hash = (hash * 7) + this.Id.GetHashCode();
		hash = (hash * 7) + this.GuildId.GetHashCode();

		return hash;
	}

	/// <summary>
	///     Gets whether the two <see cref="DiscordMember" /> objects are equal.
	/// </summary>
	/// <param name="e1">First member to compare.</param>
	/// <param name="e2">Second member to compare.</param>
	/// <returns>Whether the two members are equal.</returns>
	public static bool operator ==(DiscordMember e1, DiscordMember e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 is not null || o2 is null) && (o1 is null || o2 is not null) && ((o1 is null && o2 is null) || (e1.Id == e2.Id && e1.GuildId == e2.GuildId));
	}

	/// <summary>
	///     Gets whether the two <see cref="DiscordMember" /> objects are not equal.
	/// </summary>
	/// <param name="e1">First member to compare.</param>
	/// <param name="e2">Second member to compare.</param>
	/// <returns>Whether the two members are not equal.</returns>
	public static bool operator !=(DiscordMember e1, DiscordMember e2)
		=> !(e1 == e2);

#region Overridden user properties

	/// <summary>
	///     Gets the user.
	/// </summary>
	[JsonIgnore]
	internal DiscordUser User
		=> this.Discord?.UserCache.TryGetValue(this.Id, out var user) ?? false ? user : this.ManualUser;

	/// <summary>
	///     Sets the user when converting a json object to <see cref="DiscordMember" /> while not having access to the user
	///     cache.
	/// </summary>
	[JsonIgnore]
	public DiscordUser ManualUser { get; set; }

	/// <summary>
	///     Gets this member's username.
	/// </summary>
	[JsonIgnore]
	public override string Username
	{
		get => this.User.Username;
		internal set => this.User.Username = value;
	}

	/// <summary>
	///     Gets the member's 4-digit discriminator.
	/// </summary>
	[JsonIgnore, DiscordDeprecated]
	public override string Discriminator
	{
		get => this.User.Discriminator;
		internal set => this.User.Discriminator = value;
	}

	/// <summary>
	///     Gets the member's username with discriminator.
	/// </summary>
	[JsonIgnore]
	public override string UsernameWithDiscriminator
		=> this.User.UsernameWithDiscriminator;

	/// <summary>
	///     Gets the member's username with global name.
	/// </summary>
	[JsonIgnore]
	public override string UsernameWithGlobalName
		=> this.User.UsernameWithGlobalName;

	/// <summary>
	///     Gets the member's global name.
	/// </summary>
	[JsonIgnore]
	public override string? GlobalName
	{
		get => this.User.GlobalName;
		internal set => this.User.GlobalName = value;
	}

	/// <summary>
	///     Gets the member's user avatar hash.
	/// </summary>
	[JsonIgnore]
	public override string AvatarHash
	{
		get => this.User.AvatarHash;
		internal set => this.User.AvatarHash = value;
	}

	/// <summary>
	///     Gets the member's user banner hash.
	/// </summary>
	[JsonIgnore]
	public override string BannerHash
	{
		get => this.User.BannerHash;
		internal set => this.User.BannerHash = value;
	}

	/// <summary>
	///     Gets the member's user avatar decoration data.
	/// </summary>
	[JsonIgnore]
	public override AvatarDecorationData? AvatarDecorationData
	{
		get => this.User.AvatarDecorationData;
		internal set => this.User.AvatarDecorationData = value;
	}

	/// <summary>
	///     Gets the member's user avatar decoration data.
	/// </summary>
	[JsonIgnore]
	public override DiscordCollectibles? Collectibles
	{
		get => this.User.Collectibles;
		internal set => this.User.Collectibles = value;
	}

	/// <summary>
	///     Gets the member's primary guild.
	/// </summary>
	[JsonIgnore]
	public override DiscordPrimaryGuild? PrimaryGuild
	{
		get => this.User.PrimaryGuild;
		internal set => this.User.PrimaryGuild = value;
	}

	/// <summary>
	///     Gets whether the member is a bot.
	/// </summary>
	[JsonIgnore]
	public override bool IsBot
	{
		get => this.User.IsBot;
		internal set => this.User.IsBot = value;
	}

	/// <summary>
	///     <para>Whether this user account is migrated to the new username system.</para>
	///     <para>Learn more at <see href="https://dis.gd/usernames">dis.gd/usernames</see>.</para>
	/// </summary>
	[JsonIgnore]
	public override bool IsMigrated
		=> this.User.IsMigrated;

	/// <summary>
	///     Gets whether the member is an official Discord system user.
	/// </summary>
	[JsonIgnore]
	public override bool? IsSystem
	{
		get => this.User.IsSystem;
		internal set => this.User.IsSystem = value;
	}

	/// <summary>
	///     Gets the member's email address.
	///     <para>This is only present in OAuth.</para>
	/// </summary>
	[JsonIgnore]
	public override string Email
	{
		get => this.User.Email;
		internal set => this.User.Email = value;
	}

	/// <summary>
	///     Gets whether the member has multi-factor authentication enabled.
	/// </summary>
	[JsonIgnore]
	public override bool? MfaEnabled
	{
		get => this.User.MfaEnabled;
		internal set => this.User.MfaEnabled = value;
	}

	/// <summary>
	///     Gets whether the member is verified.
	///     <para>This is only present in OAuth.</para>
	/// </summary>
	[JsonIgnore]
	public override bool? Verified
	{
		get => this.User.Verified;
		internal set => this.User.Verified = value;
	}

	/// <summary>
	///     Gets the member's chosen language
	/// </summary>
	[JsonIgnore]
	public override string Locale
	{
		get => this.User.Locale;
		internal set => this.User.Locale = value;
	}

	/// <summary>
	///     Gets the user's flags.
	/// </summary>
	[JsonIgnore]
	public override UserFlags? OAuthFlags
	{
		get => this.User.OAuthFlags;
		internal set => this.User.OAuthFlags = value;
	}

	/// <summary>
	///     Gets the member's flags for OAuth.
	/// </summary>
	[JsonIgnore]
	public override UserFlags? Flags
	{
		get => this.User.Flags;
		internal set => this.User.Flags = value;
	}

#endregion
}
