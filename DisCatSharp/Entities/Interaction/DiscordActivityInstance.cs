using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the activity instance.
/// </summary>
public sealed class DiscordActivityInstance : ObservableApiObject
{
	/// <summary>
	///     Constructs a new <see cref="DiscordActivityInstance" /> object.
	/// </summary>
	internal DiscordActivityInstance()
	{ }

	/// <summary>
	///     Gets the instance ID of the activity.
	/// </summary>
	public string? Id
		=> this.InstanceId;

	/// <summary>
	///     Gets the application id that owns this activity instance, if available.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	///     Gets the activity instance id from interaction resource payloads.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	internal string? InteractionResourceId
	{
		get => this.InstanceId;
		set => this.InstanceId ??= value;
	}

	/// <summary>
	///     Gets the activity instance id from validation payloads.
	/// </summary>
	[JsonProperty("instance_id", NullValueHandling = NullValueHandling.Ignore)]
	public string? InstanceId { get; internal set; }

	/// <summary>
	///     Gets the launch id of the activity instance, if available.
	/// </summary>
	[JsonProperty("launch_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? LaunchId { get; internal set; }

	/// <summary>
	///     Gets the location of the activity instance, if available.
	/// </summary>
	[JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordActivityInstanceLocation? Location { get; internal set; }

	/// <summary>
	///     Gets the user ids currently associated with this activity instance.
	/// </summary>
	[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> Users { get; internal set; } = [];
}
