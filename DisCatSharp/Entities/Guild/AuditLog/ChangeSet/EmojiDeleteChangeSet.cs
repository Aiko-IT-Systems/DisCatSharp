using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting an emoji.
/// </summary>
public sealed class EmojiDeleteChangeSet : DiscordAuditLogEntry
{
	public EmojiDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.EmojiDelete;
	}

	public string? EmojiName => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public IReadOnlyList<ulong>? RolesAllowed => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "roles")?.OldValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public bool EmojiNameChanged => this.EmojiName is not null;
	public bool RolesAllowedChanged => this.RolesAllowed is not null;

	/// <inheritdoc />
	internal override string ChangeDescription
	{
		get
		{
			var description = $"{this.User} deleted emoji {this.TargetId}:\n";
			if (this.EmojiNameChanged)
				description += $"- Name: {this.EmojiName}\n";
			if (this.RolesAllowedChanged)
				description += $"- Roles Allowed: {string.Join(", ", this.RolesAllowed)}\n";
			return description;
		}
	}
}
