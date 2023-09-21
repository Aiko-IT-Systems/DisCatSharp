using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an object in Discord API.
/// </summary>
public abstract class SnowflakeObject : ObservableApiObject
{
	/// <summary>
	/// Gets the ID of this object.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; internal set; }

	/// <summary>
	/// Gets the date and time this object was created.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset CreationTimestamp
		=> this.Id.GetSnowflakeTime();

	/// <summary>
	/// Initializes a new instance of the <see cref="SnowflakeObject"/> class.
	/// </summary>
	internal SnowflakeObject(List<string>? ignored = null)
		: base(ignored)
	{ }
}
