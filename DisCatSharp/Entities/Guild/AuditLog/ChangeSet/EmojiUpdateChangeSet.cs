using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating the name of an emoji.
/// </summary>
public sealed class EmojiUpdateChangeSet : DiscordAuditLogEntry
{
	public EmojiUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.EmojiUpdate;
	}

	public string? EmojiNameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? EmojiNameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public IReadOnlyList<ulong>? RolesAllowedBefore => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "roles")?.OldValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IReadOnlyList<ulong>? RolesAllowedAfter => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "roles")?.NewValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public bool EmojiNameChanged => this.EmojiNameBefore is not null || this.EmojiNameAfter is not null;
	public bool RolesAllowedChanged => this.RolesAllowedBefore is not null || this.RolesAllowedAfter is not null;

	/// <inheritdoc />
	internal override string ChangeDescription
	{
		get
		{
			var description = $"{this.User} updated emoji {this.TargetId}:\n";
			if (this.EmojiNameChanged)
			{
				description += this.EmojiNameBefore is not null ? $"- Emoji Name Before: {this.EmojiNameBefore}\n" : "- Emoji Name Before: Not set\n";
				description += this.EmojiNameAfter is not null ? $"- Emoji Name After: {this.EmojiNameAfter}\n" : "- Emoji Name After: Not set\n";
			}

			if (this.RolesAllowedChanged)
			{
				description += this.RolesAllowedBefore is not null ? $"- Roles Allowed Before: {string.Join(", ", this.RolesAllowedBefore.Select(this.Discord.Guilds[this.GuildId].GetRole))}\n" : "- Roles Allowed Before: Not set\n";
				description += this.RolesAllowedAfter is not null ? $"- Roles Allowed After: {string.Join(", ", this.RolesAllowedAfter.Select(this.Discord.Guilds[this.GuildId].GetRole))}\n" : "- Roles Allowed After: Not set\n";
			}

			return description;
		}
	}
}
