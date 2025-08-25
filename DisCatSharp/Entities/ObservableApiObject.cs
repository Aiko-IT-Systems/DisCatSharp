using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an observable API object that provides functionality for handling
///     additional JSON properties and ignored keys during serialization and deserialization.
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
	/// <param name="ignored">
	///     A list of JSON property names to be ignored during serialization and deserialization.
	/// </param>
	protected ObservableApiObject(List<string>? ignored = null)
	{
		if (ignored != null)
			foreach (var ignoredKey in ignored)
				this.IgnoredJsonKeys.Add(ignoredKey);
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
	///     Gets or sets a list of JSON keys that should be ignored during serialization and deserialization.
	/// </summary>
	/// <remarks>
	///     This property is used to specify keys that are excluded from processing when handling JSON data.
	///     It is particularly useful for ignoring fields that are not relevant or should not be modified.
	/// </remarks>
	[JsonIgnore]
	internal List<string> IgnoredJsonKeys { get; set; } = [];
}
