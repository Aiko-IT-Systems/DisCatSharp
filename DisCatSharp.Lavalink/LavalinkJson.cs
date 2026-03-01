using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink;

/// <summary>
///     Represents a custom json (de-)serializer.
/// </summary>
internal static class LavalinkJson
{
	/// <summary>
	///		Gets the serializer settings.
	/// </summary>
	private static readonly JsonSerializerSettings s_settings = new()
	{
		ContractResolver = new DisCatSharpContractResolver()
	};

	/// <summary>
	///     Gets the serializer.
	/// </summary>
	private static readonly JsonSerializer s_serializer = JsonSerializer.CreateDefault(s_settings);

	/// <summary>
	///     Deserializes the JSON to the specified .NET type using <see cref="s_serializer" />.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
	/// <param name="value">The object to deserialize.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeObject<T>(string value)
		=> JsonConvert.DeserializeObject<T>(value, s_settings);

	/// <summary>
	///     Serializes the specified object to a JSON string using formatting and <see cref="s_serializer" />.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <returns>A JSON string representation of the object.</returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value)
		=> SerializeObjectInternal(value, null!, s_serializer);

	/// <summary>
	///     Serializes the object.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="type">The type.</param>
	/// <param name="jsonSerializer">The json serializer.</param>
	private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
	{
		var stringWriter = new StringWriter(new(), CultureInfo.InvariantCulture);
		using (var jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonTextWriter, value, type);
		}

		return stringWriter.ToString();
	}
}
