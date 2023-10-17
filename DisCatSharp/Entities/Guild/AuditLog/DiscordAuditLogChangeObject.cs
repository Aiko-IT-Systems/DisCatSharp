using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities.Guild;

/// <summary>
/// Represents a <see cref="DiscordAuditLogChangeObject"/> in a <see cref="DiscordAuditLogEntry"/>.
/// </summary>
public class DiscordAuditLogChangeObject
{
	/// <summary>
	/// Gets the new value of the change.
	/// </summary>
	[JsonProperty("new_value", NullValueHandling = NullValueHandling.Ignore)]
	public object? NewValue { get; internal set; }

	/// <summary>
	/// Gets the old value of the change.
	/// </summary>
	[JsonProperty("old_value", NullValueHandling = NullValueHandling.Ignore)]
	public object? OldValue { get; internal set; }

	/// <summary>
	/// Gets the key of the changed entity.
	/// </summary>
	[JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
	public string Key { get; internal set; }

	[JsonIgnore]
	public bool IsSpecialKey
		=> SpecialChangeKeys.SpecialKeys.Contains(this.Key);
}

/// <summary>
/// Represents special change keys for the <see cref="DiscordAuditLogChangeObject"/>.
/// </summary>
public static class SpecialChangeKeys
{
	/// <summary>
	/// Special <see cref="DiscordAuditLogChangeObject.Key"/> for role add.
	/// Contains a partial role object.
	/// </summary>
	public const string PARTIAL_ROLE_ADD = "$add";

	/// <summary>
	/// Special <see cref="DiscordAuditLogChangeObject.Key"/> for role remove.
	/// Contains a partial role object.
	/// </summary>
	public const string PARTIAL_ROLE_REMOVE = "$remove";

	/// <summary>
	/// Special <see cref="DiscordAuditLogChangeObject.Key"/> for invite and invite metadata channel ids.
	/// Uses <c>channel_id</c> instead of <c>channel</c>.<c>id</c>.
	/// </summary>
	public const string INVITE_CHANNEL_ID = "channel_id";

	/// <summary>
	/// Special <see cref="DiscordAuditLogChangeObject.Key"/> for webhook avatars.
	/// Uses <c>avatar_hash</c> instead of <c>avatar</c>.
	/// </summary>
	public const string WEBHOOK_AVATAR = "avatar_hash";

	public static readonly List<string> SpecialKeys = new()
	{
		PARTIAL_ROLE_ADD, PARTIAL_ROLE_REMOVE, INVITE_CHANNEL_ID, WEBHOOK_AVATAR
	};
}
