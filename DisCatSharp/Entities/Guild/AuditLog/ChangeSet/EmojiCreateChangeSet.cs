using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for creating an emoji.
/// </summary>
public class EmojiCreateChangeSet : DiscordAuditLogEntry
{
	public EmojiCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.EmojiCreate;
	}

	public string? EmojiName => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public IReadOnlyList<ulong>? RolesAllowed => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "roles")?.NewValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public bool EmojiNameChanged => this.EmojiName is not null;
	public bool RolesAllowedChanged => this.RolesAllowed is not null;

	/// <inheritdoc />
	internal override string ChangeDescription
	{
		get
		{
			var description = $"{this.UserId} created a new emoji with the following details:\n";
			if (this.EmojiNameChanged)
				description += $"- Emoji Name: {this.EmojiName}\n";
			if (this.RolesAllowedChanged)
				description += $"- Roles Allowed: {string.Join(", ", this.RolesAllowed)}\n";
			return description;
		}
	}
}
