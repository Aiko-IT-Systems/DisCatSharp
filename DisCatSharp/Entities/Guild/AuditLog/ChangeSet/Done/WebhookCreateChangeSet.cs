using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for creating a webhook.
/// </summary>
public sealed class WebhookCreateChangeSet : DiscordAuditLogEntry
{
	public WebhookCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.WebhookCreate;
	}

	/// <summary>
	/// Gets the type of the webhook.
	/// </summary>
	public WebhookType Type => (WebhookType)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;

	/// <summary>
	/// Gets the name of the webhook.
	/// </summary>
	public string? Name => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	/// <summary>
	/// Gets the avatar hash of the webhook.
	/// </summary>
	public string? Avatar => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.NewValue;

	/// <summary>
	/// Gets the token of the webhook.
	/// </summary>
	public string? Token => (string?)this.Changes.FirstOrDefault(x => x.Key == "token")?.NewValue;

	/// <summary>
	/// Gets the application id of the webhook, if applicable.
	/// </summary>
	public ulong? ApplicationId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "application_id")?.NewValue;

	/// <summary>
	/// Gets the source guild id of the webhook, if applicable.
	/// </summary>
	public ulong? SourceGuildId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "source_guild_id")?.NewValue;

	/// <summary>
	/// Gets the source channel id of the webhook, if applicable.
	/// </summary>
	public ulong? SourceChannelId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "source_channel_id")?.NewValue;

	/// <summary>
	/// Gets the url of the webhook, if applicable.
	/// </summary>
	public string? Url => (string?)this.Changes.FirstOrDefault(x => x.Key == "url")?.NewValue;
}
