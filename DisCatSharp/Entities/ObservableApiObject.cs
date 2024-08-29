using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an observable api object in Discord API.
/// </summary>
public abstract class ObservableApiObject
{
	/// <summary>
	///     Gets additional json properties that are not known to the deserializing object.
	/// </summary>
	[JsonIgnore]
	internal IDictionary<string, object> UnknownProperties = new Dictionary<string, object>();

	/// <summary>
	///     Initializes a new instance of the <see cref="ObservableApiObject" /> class.
	/// </summary>
	/// <param name="ignored">The ignored json keys.</param>
	protected ObservableApiObject(List<string>? ignored = null)
	{
		if (ignored is null)
			return;

		this.IgnoredJsonKeys = [..ignored];
	}

	/// <summary>
	///     Gets the client instance this object is tied to.
	/// </summary>
	[JsonIgnore]
	internal BaseDiscordClient Discord { get; set; }

	/// <summary>
	///     Lets JsonConvert set the unknown properties.
	/// </summary>
	[JsonExtensionData(ReadData = true, WriteData = false)]
	public IDictionary<string, object> AdditionalProperties
	{
		get => this.UnknownProperties;
		set => this.UnknownProperties = value;
	}

	/// <summary>
	///     Gets or sets the ignored json keys.
	/// </summary>
	[JsonIgnore]
	internal List<string> IgnoredJsonKeys { get; init; } = [];
}
