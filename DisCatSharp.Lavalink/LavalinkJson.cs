using System.Diagnostics;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink;

/// <summary>
/// Represents a custom json (de-)serializer.
/// </summary>
internal static class LavalinkJson
{
	/// <summary>
	/// The <see cref="JsonSerializerSettings"/> with an <see cref="Newtonsoft.Json.Serialization.IContractResolver"/> for <see cref="Optional"/> types.
	/// </summary>
	private static readonly JsonSerializerSettings s_setting = new()
	{
		ContractResolver = new OptionalJsonContractResolver()
	};

	/// <summary>
	/// Deserializes the JSON to the specified .NET type using <see cref="s_setting"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
	/// <param name="value">The object to deserialize.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeObject<T>(string value)
		=> JsonConvert.DeserializeObject<T>(value, s_setting);

	/// <summary>
	/// Serializes the specified object to a JSON string using formatting and <see cref="s_setting"/>.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted. Defaults to <see cref="Formatting.Indented"/>.</param>
	/// <returns>A JSON string representation of the object.</returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, Formatting formatting = Formatting.None)
		=> JsonConvert.SerializeObject(value, formatting, s_setting);
}
