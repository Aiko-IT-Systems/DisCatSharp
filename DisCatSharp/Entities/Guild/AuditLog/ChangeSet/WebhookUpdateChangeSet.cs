using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating a webhook or its properties.
/// </summary>
public sealed class WebhookUpdateChangeSet : DiscordAuditLogEntry
{
	public WebhookUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.WebhookUpdate;
	}

	/// <summary>
	/// Gets the old name of the webhook.
	/// </summary>
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;

	/// <summary>
	/// Gets the new name of the webhook.
	/// </summary>
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	/// <summary>
	/// Gets the old avatar hash of the webhook.
	/// </summary>
	public string? AvatarBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.OldValue;

	/// <summary>
	/// Gets the new avatar hash of the webhook.
	/// </summary>
	public string? AvatarAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.NewValue;
}
