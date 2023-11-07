using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting a webhook.
/// </summary>
public sealed class WebhookDeleteChangeSet : DiscordAuditLogEntry
{
	public WebhookDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.WebhookDelete;
	}

	/// <summary>
	/// Gets the type of the webhook.
	/// </summary>
	public WebhookType Type => (WebhookType)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;

	/// <summary>
	/// Gets the name of the webhook.
	/// </summary>
	public string? Name => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;

	/// <summary>
	/// Gets the avatar hash of the webhook.
	/// </summary>
	public string? Avatar => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.OldValue;

	/// <summary>
	/// Gets the token of the webhook.
	/// </summary>
	public string? Token => (string?)this.Changes.FirstOrDefault(x => x.Key == "token")?.OldValue;

	/// <summary>
	/// Gets the application id of the webhook, if applicable.
	/// </summary>
	public ulong? ApplicationId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "application_id")?.OldValue;

	/// <summary>
	/// Gets the source guild id of the webhook, if applicable.
	/// </summary>
	public ulong? SourceGuildId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "source_guild_id")?.OldValue;

	/// <summary>
	/// Gets the source channel id of the webhook, if applicable.
	/// </summary>
	public ulong? SourceChannelId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "source_channel_id")?.OldValue;

	/// <summary>
	/// Gets the url of the webhook, if applicable.
	/// </summary>
	public string? Url => (string?)this.Changes.FirstOrDefault(x => x.Key == "url")?.OldValue;
}
