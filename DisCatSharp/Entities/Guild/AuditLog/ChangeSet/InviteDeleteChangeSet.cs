using System;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting a server invite.
/// </summary>
public sealed class InviteDeleteChangeSet : DiscordAuditLogEntry
{
	internal InviteDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.InviteCreate;

		var rawMaxAge = this.MaxAge;
		this.ExpiryDateTime = rawMaxAge is not null && rawMaxAge != 0 ? DateTime.UtcNow.AddSeconds(rawMaxAge ?? 0) : null;
	}

	public string Code => (string?)this.Changes.FirstOrDefault(x => x.Key == "code")?.OldValue;
	public string Url => DiscordDomain.GetDomain(CoreDomain.DiscordShortlink).Url + "/" + this.Code;

	public ulong? ChannelId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "channel_id")?.OldValue;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId ?? 0ul, out var channel) ? channel : null;

	public ulong? InviterId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "inviter_id")?.OldValue;
	public DiscordUser? Inviter => this.InviterId is null ? null : this.Discord.GetCachedOrEmptyUserInternal(this.InviterId.Value);

	public int? Uses => (int?)this.Changes.FirstOrDefault(x => x.Key == "uses")?.OldValue;
	public int? MaxUses => (int?)this.Changes.FirstOrDefault(x => x.Key == "max_uses")?.OldValue;
	public int? MaxAge => (int?)this.Changes.FirstOrDefault(x => x.Key == "max_age")?.OldValue;
	public DateTime? ExpiryDateTime { get; internal set; } = null;
	public bool? Temporary => (bool?)this.Changes.FirstOrDefault(x => x.Key == "temporary")?.OldValue;
	public InviteFlags? Flags => (InviteFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;

	public TargetType? TargetType => (TargetType?)this.Changes.FirstOrDefault(x => x.Key == "target_type")?.OldValue;
	public DiscordUser? TargetUser => (DiscordUser?)this.Changes.FirstOrDefault(x => x.Key == "target_user")?.OldValue;
	public DiscordApplication? TargetApplication => (DiscordApplication?)this.Changes.FirstOrDefault(x => x.Key == "target_application")?.OldValue;
	public DiscordStageInstance? TargetStage => (DiscordStageInstance?)this.Changes.FirstOrDefault(x => x.Key == "stage_instance")?.OldValue;
	public DiscordScheduledEvent? TargetEvent => (DiscordScheduledEvent?)this.Changes.FirstOrDefault(x => x.Key == "guild_scheduled_event")?.OldValue;
}
