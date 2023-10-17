using System;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating a server invite.
/// </summary>
public sealed class InviteUpdateChangeSet : DiscordAuditLogEntry
{
	internal InviteUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.InviteUpdate;

		var rawMaxAgeBefore = this.MaxAgeBefore;
		this.ExpiryDateTimeBefore = rawMaxAgeBefore is not null && rawMaxAgeBefore != 0 ? DateTime.UtcNow.AddSeconds(rawMaxAgeBefore.Value) : null;

		var rawMaxAgeAfter = this.MaxAgeAfter;
		this.ExpiryDateTimeAfter = rawMaxAgeAfter is not null && rawMaxAgeAfter != 0 ? DateTime.UtcNow.AddSeconds(rawMaxAgeAfter.Value) : null;
	}

	public bool CodeChanged => this.CodeBefore is not null || this.CodeAfter is not null;
	public string CodeBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "code")?.OldValue;
	public string CodeAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "code")?.OldValue;
	public string UrlBefore => DiscordDomain.GetDomain(CoreDomain.DiscordShortlink).Url + "/" + this.CodeBefore;
	public string UrlAfter => DiscordDomain.GetDomain(CoreDomain.DiscordShortlink).Url + "/" + this.CodeAfter;

	public bool ChannelIdChanged => this.ChannelIdBefore is not null || this.ChannelIdAfter is not null;
	public ulong? ChannelIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "channel_id")?.OldValue;
	public ulong? ChannelIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "channel_id")?.NewValue;
	public DiscordChannel? ChannelBefore => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelIdBefore ?? 0ul, out var channel) ? channel : null;
	public DiscordChannel? ChannelAfter => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelIdAfter ?? 0ul, out var channel) ? channel : null;

	public bool InviterIdChanged => this.InviterIdBefore is not null || this.InviterIdAfter is not null;
	public ulong? InviterIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "inviter_id")?.OldValue;
	public ulong? InviterIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "inviter_id")?.NewValue;
	public DiscordUser? InviterBefore => this.InviterIdBefore is null ? null : this.Discord.GetCachedOrEmptyUserInternal(this.InviterIdBefore.Value);
	public DiscordUser? InviterAfter => this.InviterAfter is null ? null : this.Discord.GetCachedOrEmptyUserInternal(this.InviterIdAfter.Value);

	public bool UsesChanged => this.UsesBefore is not null || this.UsesAfter is not null;
	public int? UsesBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "uses")?.OldValue;
	public int? UsesAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "uses")?.NewValue;

	public bool MaxUsesChanged => this.MaxUsesBefore is not null || this.MaxUsesAfter is not null;
	public int? MaxUsesBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "max_uses")?.OldValue;
	public int? MaxUsesAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "max_uses")?.NewValue;

	public bool MaxAgeChanged => this.MaxAgeBefore is not null || this.MaxAgeAfter is not null;
	public int? MaxAgeBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "max_age")?.OldValue;
	public int? MaxAgeAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "max_age")?.NewValue;
	public DateTime? ExpiryDateTimeBefore { get; internal set; } = null;
	public DateTime? ExpiryDateTimeAfter { get; internal set; } = null;

	public bool TemporaryChanged => this.TemporaryBefore is not null || this.TemporaryAfter is not null;
	public bool? TemporaryBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "temporary")?.OldValue;
	public bool? TemporaryAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "temporary")?.NewValue;

	public bool FlagsChanged => this.FlagsBefore is not null || this.FlagsAfter is not null;
	public InviteFlags? FlagsBefore => (InviteFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;
	public InviteFlags? FlagsAfter => (InviteFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;

	public bool TargetTypeChanged => this.TargetTypeBefore is not null || this.TargetTypeAfter is not null;
	public TargetType? TargetTypeBefore => (TargetType?)this.Changes.FirstOrDefault(x => x.Key == "target_type")?.OldValue;
	public TargetType? TargetTypeAfter => (TargetType?)this.Changes.FirstOrDefault(x => x.Key == "target_type")?.NewValue;

	public bool TargetUserChanged => this.TargetUserBefore is not null || this.TargetUserAfter is not null;
	public DiscordUser? TargetUserBefore => (DiscordUser?)this.Changes.FirstOrDefault(x => x.Key == "target_user")?.OldValue;
	public DiscordUser? TargetUserAfter => (DiscordUser?)this.Changes.FirstOrDefault(x => x.Key == "target_user")?.NewValue;

	public bool TargetApplicationChanged => this.TargetApplicationBefore is not null || this.TargetApplicationAfter is not null;
	public DiscordApplication? TargetApplicationBefore => (DiscordApplication?)this.Changes.FirstOrDefault(x => x.Key == "target_application")?.OldValue;
	public DiscordApplication? TargetApplicationAfter => (DiscordApplication?)this.Changes.FirstOrDefault(x => x.Key == "target_application")?.NewValue;

	public bool TargetStageChanged => this.TargetStageBefore is not null || this.TargetStageAfter is not null;
	public DiscordStageInstance? TargetStageBefore => (DiscordStageInstance?)this.Changes.FirstOrDefault(x => x.Key == "stage_instance")?.OldValue;
	public DiscordStageInstance? TargetStageAfter => (DiscordStageInstance?)this.Changes.FirstOrDefault(x => x.Key == "stage_instance")?.NewValue;

	public bool TargetEventChanged => this.TargetEventBefore is not null || this.TargetEventAfter is not null;
	public DiscordScheduledEvent? TargetEventBefore => (DiscordScheduledEvent?)this.Changes.FirstOrDefault(x => x.Key == "guild_scheduled_event")?.OldValue;
	public DiscordScheduledEvent? TargetEventAfter => (DiscordScheduledEvent?)this.Changes.FirstOrDefault(x => x.Key == "guild_scheduled_event")?.NewValue;
}
