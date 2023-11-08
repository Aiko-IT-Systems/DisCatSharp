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
	/// Gets a value indicating whether the name of the webhook has changed.
	/// </summary>
	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;

	/// <summary>
	/// Gets the old name of the webhook.
	/// </summary>
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;

	/// <summary>
	/// Gets the new name of the webhook.
	/// </summary>
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the avatar hash of the webhook has changed.
	/// </summary>
	public bool AvatarChanged => this.AvatarBefore is not null || this.AvatarAfter is not null;

	/// <summary>
	/// Gets the old avatar hash of the webhook.
	/// </summary>
	public string? AvatarBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.OldValue;

	/// <summary>
	/// Gets the new avatar hash of the webhook.
	/// </summary>
	public string? AvatarAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.NewValue;
}
