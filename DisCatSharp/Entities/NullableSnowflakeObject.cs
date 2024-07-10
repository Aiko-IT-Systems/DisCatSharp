using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an object in Discord API.
/// </summary>
public abstract class NullableSnowflakeObject : ObservableApiObject
{
	/// <summary>
	/// Gets the ID of this object.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Id { get; internal set; }

	/// <summary>
	/// Gets the date and time this object was created.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? CreationTimestamp
		=> this.Id.GetSnowflakeTime();

	/// <summary>
	/// Initializes a new instance of the <see cref="NullableSnowflakeObject"/> class.
	/// </summary>
	internal NullableSnowflakeObject()
	{ }
}
