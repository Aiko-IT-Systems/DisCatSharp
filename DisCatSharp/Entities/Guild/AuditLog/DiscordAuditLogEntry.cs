using System;
using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord audit log entry.
/// </summary>
public class DiscordAuditLogEntry : SnowflakeObject
{
	/// <summary>
	/// Gets the ID of the affected entity (webhook, user, role, etc.).
	/// </summary>
	[JsonProperty("target_id", NullValueHandling = NullValueHandling.Ignore)]
	public SnowflakeObject? TargetId { get; internal set; }

	/// <summary>
	/// Gets the list of changes made to the target_id.
	/// </summary>
	[JsonProperty("changes", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAuditLogChangeObject> Changes { get; internal set; } = new();

	/// <summary>
	/// Gets the ID of the user or app that made the changes.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; internal set; }

	/// <summary>
	/// Gets the type of action that occurred in the audit log entry.
	/// </summary>
	[JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
	public AuditLogActionType ActionType { get; internal set; }

	/// <summary>
	/// Gets additional information for certain event types.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordAuditEntryInfo? Options { get; internal set; }

	/// <summary>
	/// Gets the reason for the change (1-512 characters).
	/// </summary>
	[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
	public string? Reason { get; internal set; }

	/// <summary>
	/// Gets this <see cref="DiscordAuditLogEntry"/> as an easy to-use <see cref="AuditLogChangeSet"/> with the given type.
	/// </summary>
	/// <typeparam name="T">The type to convert to. Use the type based on the <see cref="ActionType"/>.</typeparam>
	/// <returns>The easy to-use audit log entry.</returns>
	/// <exception cref="InvalidCastException">Thrown when the <see cref="ActionType"/> is not compatible with the targets <see cref="AuditLogChangeSet.ValidFor"/> type.</exception>
	public T? As<T>()
		where T : AuditLogChangeSet
		=> this is T { IsValid: true } toConvert
			? toConvert
			: throw new InvalidCastException($"Cannot convert {this.GetType().Name} with action type {this.ActionType} to {typeof(T).Name}.");
}
