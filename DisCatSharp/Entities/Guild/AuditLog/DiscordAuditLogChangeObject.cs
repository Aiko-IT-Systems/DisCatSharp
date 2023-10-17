using Newtonsoft.Json;

namespace DisCatSharp.Entities.Guild.AuditLog;

/// <summary>
/// Represents a <see cref="DiscordAuditLogChangeObject"/> in a <see cref="DiscordAuditLogEntry"/>.
/// </summary>
public class DiscordAuditLogChangeObject
{
	/// <summary>
	/// Gets the new value of the change.
	/// </summary>
	[JsonProperty("new_value", NullValueHandling = NullValueHandling.Ignore)]
	public object NewValue { get; set; }

	/// <summary>
	/// Gets the old value of the change.
	/// </summary>
	[JsonProperty("old_value", NullValueHandling = NullValueHandling.Ignore)]
	public object OldValue { get; set; }

	/// <summary>
	/// Gets the key of the changed entity.
	/// </summary>
	[JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
	public string Key { get; set; }
}
