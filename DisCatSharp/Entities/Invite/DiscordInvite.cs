using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord invite.
/// </summary>
public class DiscordInvite : SnowflakeObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordInvite" /> class.
	/// </summary>
	internal DiscordInvite()
	{ }

	/// <summary>
	///     Gets the invite's code.
	/// </summary>
	[JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
	public string Code { get; internal set; }

	/// <summary>
	///     Gets the invite's url.
	/// </summary>
	[JsonIgnore]
	public string Url
		=> DiscordDomain.GetDomain(CoreDomain.DiscordShortlink).Url + "/" + this.Code;

	/// <summary>
	///     Gets the invite's url as Uri.
	/// </summary>
	[JsonIgnore]
	public Uri Uri
		=> new(this.Url);

	/// <summary>
	///     Gets the guild this invite is for.
	/// </summary>
	[JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInviteGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the guild id this invite is for.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	///     Gets the channel this invite is for.
	/// </summary>
	[JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInviteChannel Channel { get; internal set; }

	/// <summary>
	///     Gets the channel id this invite is for.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	///     Gets the target type for the voice channel this invite is for.
	/// </summary>
	[JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
	public TargetType? TargetType { get; internal set; }

	/// <summary>
	///     Gets the type of this invite.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public InviteType Type { get; internal set; }

	/// <summary>
	///     Gets the user that is currently livestreaming.
	/// </summary>
	[JsonProperty("target_user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser TargetUser { get; internal set; }

	/// <summary>
	///     Gets the embedded partial application to open for this voice channel.
	/// </summary>
	[JsonProperty("target_application", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordApplication TargetApplication { get; internal set; }

	/// <summary>
	///     Gets the approximate guild online member count for the invite.
	/// </summary>
	[JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximatePresenceCount { get; internal set; }

	/// <summary>
	///     Gets the approximate guild total member count for the invite.
	/// </summary>
	[JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximateMemberCount { get; internal set; }

	/// <summary>
	///     Gets the user who created the invite.
	/// </summary>
	[JsonProperty("inviter", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser Inviter { get; internal set; }

	/// <summary>
	///     Gets the number of times this invite has been used.
	/// </summary>
	[JsonProperty("uses", NullValueHandling = NullValueHandling.Ignore)]
	public int Uses { get; internal set; }

	/// <summary>
	///     Gets the max number of times this invite can be used.
	/// </summary>
	[JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxUses { get; internal set; }

	/// <summary>
	///     Gets duration in seconds after which the invite expires.
	/// </summary>
	[JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxAge { get; internal set; }

	/// <summary>
	///     Gets whether this invite only grants temporary membership.
	/// </summary>
	[JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsTemporary { get; internal set; }

	/// <summary>
	///     Gets the date and time this invite was created.
	/// </summary>
	[JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset CreatedAt { get; internal set; }

	/// <summary>
	///     Gets the date and time when this invite expires.
	/// </summary>
	[JsonProperty("expires_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ExpiresAt { get; internal set; }

	/// <summary>
	///     Gets the date and time when this invite got expired.
	/// </summary>
	[JsonProperty("expired_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ExpiredAt { get; internal set; }

	/// <summary>
	///     Gets whether this invite is revoked.
	/// </summary>
	[JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsRevoked { get; internal set; }

	/// <summary>
	///     Gets the guild scheduled event data for the invite.
	/// </summary>
	[JsonProperty("guild_scheduled_event", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordScheduledEvent GuildScheduledEvent { get; internal set; }

	/// <summary>
	///     Gets the invites flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public InviteFlags Flags { get; internal set; }

	/// <summary>
	///    Gets whether the useer can change their nickname on the guild.
	/// </summary>
	[JsonProperty("is_nickname_changeable", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsNicknameChangeable { get; internal set; } = null;

	/// <summary>
	///    Gets the guild profile, if applicable.
	/// </summary>
	[JsonProperty("profile", NullValueHandling = NullValueHandling.Ignore), RequiresOverride("eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRGlzY29yZCBDbGllbnQiLCJyZWxlYXNlX2NoYW5uZWwiOiJjYW5hcnkiLCJjbGllbnRfdmVyc2lvbiI6IjEuMC41NzgiLCJvc192ZXJzaW9uIjoiMTAuMC4yNjEyMCIsIm9zX2FyY2giOiJ4NjQiLCJhcHBfYXJjaCI6Ing2NCIsInN5c3RlbV9sb2NhbGUiOiJlbi1VUyIsImhhc19jbGllbnRfbW9kcyI6ZmFsc2UsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQpIEFwcGxlV2ViS2l0LzUzNy4zNiAoS0hUTUwsIGxpa2UgR2Vja28pIGRpc2NvcmQvMS4wLjU3OCBDaHJvbWUvMTM0LjAuNjk5OC40NCBFbGVjdHJvbi8zNS4wLjIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6IjM1LjAuMiIsIm9zX3Nka192ZXJzaW9uIjoiMjYxMjAiLCJjbGllbnRfYnVpbGRfbnVtYmVyIjozODA2NzUsIm5hdGl2ZV9idWlsZF9udW1iZXIiOjYwNjYzLCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==", "21/03/2025")]
	public DiscordGuildProfile? Profile { get; internal set; }

	/// <summary>
	///    Gets the roles to be assigned to the user on joining the guild via this invite.
	/// </summary>
	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordInviteRole>? RolesInternal { get; set; }

	/// <summary>
	///    Gets the roles to be assigned to the user on joining the guild via this invite.
	/// </summary>
	[JsonIgnore]
	public List<DiscordInviteRole>? Roles
		=> this.RolesInternal ?? this.RoleIdsInternal?.Select(id => (DiscordInviteRole)this.Discord.Guilds[this.GuildId].GetRole(id)).ToList();

	/// <summary>
	///    Gets the role ids to be assigned to the user on joining the guild via this invite.
	/// </summary>
	[JsonProperty("role_ids", NullValueHandling = NullValueHandling.Ignore)]
	internal List<ulong>? RoleIdsInternal { get; set; }

	/// <summary>
	///    Gets the role ids to be assigned to the user on joining the guild via this invite.
	/// </summary>
	[JsonIgnore]
	public List<ulong>? RoleIds
		=> this.RoleIdsInternal ?? this.RolesInternal?.Select(role => role.Id).ToList();

	/// <summary>
	///     Deletes the invite.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns></returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" /> permission or the <see cref="Permissions.ManageGuild" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordInvite> DeleteAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteInviteAsync(this.Code, reason);

	/// <summary>
	///     Gets the target users allowed to accept an invite.
	/// </summary>
	/// <returns>An allowlist of user ids.</returns>
	/// <exception cref="NotFoundException">Thrown when the invite does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<ulong>> GetTargetUsersAsync()
		=> this.Discord.ApiClient.GetInviteTargetUsersAsync(this.Code);

	/// <summary>
	///     Updates the target users allowed to accept an invite using a CSV stream.
	/// </summary>
	/// <param name="targetUsersCsv">
	///     CSV stream containing a single <c>Users</c> column. The CSV must have a header row where the first
	///     (and only) column header is <c>Users</c>, and each subsequent line must contain exactly one user ID.
	/// </param>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="NotFoundException">Thrown when the invite does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UpdateTargetUsersAsync(Stream targetUsersCsv, string reason = null)
		=> this.Discord.ApiClient.UpdateInviteTargetUsersAsync(this.Code, targetUsersCsv, null, null, reason);

	/// <summary>
	///     Updates the target users allowed to accept an invite using user ids.
	/// </summary>
	/// <param name="targetUserIds">User ids allowed to accept the invite.</param>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="NotFoundException">Thrown when the invite does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UpdateTargetUsersAsync(IEnumerable<ulong> targetUserIds, string reason = null)
		=> this.Discord.ApiClient.UpdateInviteTargetUsersAsync(this.Code, null, targetUserIds, null, reason);

	/// <summary>
	///     Updates the target users allowed to accept an invite using user objects.
	/// </summary>
	/// <param name="targetUsers">Users allowed to accept the invite.</param>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="NotFoundException">Thrown when the invite does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UpdateTargetUsersAsync(IEnumerable<DiscordUser> targetUsers, string reason = null)
		=> this.Discord.ApiClient.UpdateInviteTargetUsersAsync(this.Code, null, null, targetUsers, reason);

	/// <summary>
	///     Gets the invite target users job status.
	/// </summary>
	/// <returns>The job status.</returns>
	/// <exception cref="NotFoundException">Thrown when the invite does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordInviteTargetUsersJobStatus> GetTargetUsersJobStatusAsync()
		=> this.Discord.ApiClient.GetInviteTargetUsersJobStatusAsync(this.Code);

	/// <summary>
	///     Converts this invite into a link.
	/// </summary>
	/// <returns>A discord.gg invite link.</returns>
	public override string ToString()
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordShortlink).Url}/{this.Code}";
}
