using System;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating a member in the server.
/// </summary>
public sealed class MemberUpdateChangeSet : DiscordAuditLogEntry
{
	public MemberUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberUpdate;
	}

	public bool NicknameChanged => this.NicknameBefore is null || this.NicknameAfter is null;
	public string? NicknameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "nick")?.OldValue;
	public string? NicknameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "nick")?.NewValue;

	public bool AvatarChanged => this.AvatarBefore is null || this.AvatarAfter is null;
	public string? AvatarBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar")?.OldValue;
	public string? AvatarAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar")?.NewValue;

	public bool DeafChanged => this.DeafBefore is null || this.DeafAfter is null;
	public bool? DeafBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "deaf")?.OldValue;
	public bool? DeafAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "deaf")?.NewValue;

	public bool MuteChanged => this.MuteBefore is null || this.MuteAfter is null;
	public bool? MuteBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "mute")?.OldValue;
	public bool? MuteAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "mute")?.NewValue;

	public bool FlagsChanged => this.FlagsBefore is null || this.FlagsAfter is null;
	public MemberFlags? FlagsBefore => (MemberFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;
	public MemberFlags? FlagsAfter => (MemberFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;

	public bool PendingChanged => this.PendingBefore is null || this.PendingAfter is null;
	public bool? PendingBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "pending")?.OldValue;
	public bool? PendingAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "pending")?.NewValue;

	public bool CommunicationDisabledUntilChanged => this.CommunicationDisabledUntilBefore is null || this.CommunicationDisabledUntilAfter is null;
	public DateTimeOffset? CommunicationDisabledUntilBefore => (DateTimeOffset?)this.Changes.FirstOrDefault(x => x.Key == "communication_disabled_until")?.OldValue;
	public DateTimeOffset? CommunicationDisabledUntilAfter => (DateTimeOffset?)this.Changes.FirstOrDefault(x => x.Key == "communication_disabled_until")?.NewValue;
}
