using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

public abstract class ObservableApiObject
{
	/// <summary>
	/// Gets the client instance this object is tied to.
	/// </summary>
	[JsonIgnore]
	internal BaseDiscordClient Discord { get; set; }

	/// <summary>
	/// Gets additional json properties that are not known to the deserializing object.
	/// </summary>
	[JsonIgnore]
	internal IDictionary<string, object> UnknownProperties = new Dictionary<string, object>();

	/// <summary>
	/// Lets JsonConvert set the unknown properties.
	/// </summary>
	[JsonExtensionData(ReadData = true, WriteData = false)]
	public IDictionary<string, object> AdditionalProperties
	{
		get => this.UnknownProperties;
		set => this.UnknownProperties = value;
	}

	[JsonIgnore]
	internal List<string> IgnoredJsonKeys { get; set; } = [];

	protected ObservableApiObject(List<string>? ignored = null)
	{
		if (ignored != null)
			foreach (var ignoredKey in ignored)
				this.IgnoredJsonKeys.Add(ignoredKey);
	}
}
