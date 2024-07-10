using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DisCatSharp.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Serialization;

/// <summary>
/// Used for a <see cref="Dictionary{TKey,TValue}"/> or <see cref="ConcurrentDictionary{TKey,TValue}"/> mapping
/// <see cref="ulong"/> to any class extending <see cref="SnowflakeObject"/> (or, as a special case,
/// <see cref="DiscordVoiceState"/>). When serializing, discards the ulong
/// keys and writes only the values. When deserializing, pulls the keys from <see cref="SnowflakeObject.Id"/> (or,
/// in the case of <see cref="DiscordVoiceState"/>, <see cref="DiscordVoiceState.UserId"/>.
/// </summary>
internal class SnowflakeArrayAsDictionaryJsonConverter : JsonConverter
{
	/// <summary>
	/// Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value is null)
			writer.WriteNull();
		else
		{
			var type = value.GetType().GetTypeInfo();
			JToken.FromObject(type.GetDeclaredProperty("Values")!.GetValue(value)!).WriteTo(writer);
		}
	}

	/// <summary>
	/// Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var constructor = objectType.GetTypeInfo().DeclaredConstructors
			.FirstOrDefault(e => !e.IsStatic && e.GetParameters().Length == 0);

		var dict = constructor?.Invoke([]);

		// the default name of an indexer is "Item"
		var properties = objectType.GetTypeInfo().GetDeclaredProperty("Item");

		var entries = (IEnumerable?)serializer.Deserialize(reader, objectType.GenericTypeArguments[1].MakeArrayType());
		if (entries is null || properties is null)
			return dict!;

		foreach (var entry in entries)
			properties.SetValue(dict, entry, [
				(entry as SnowflakeObject)?.Id
				?? (entry as DiscordVoiceState)?.UserId
				?? throw new InvalidOperationException($"Type {entry?.GetType()} is not deserializable")
			]);

		return dict;
	}

	/// <summary>
	/// Whether the snowflake can be converted.
	/// </summary>
	/// <param name="objectType">The object type.</param>
	public override bool CanConvert(Type objectType)
	{
		var genericTypedef = objectType.GetGenericTypeDefinition();
		if (genericTypedef != typeof(Dictionary<,>) && genericTypedef != typeof(ConcurrentDictionary<,>))
			return false;

		if (objectType.GenericTypeArguments[0] != typeof(ulong))
			return false;

		var valueParam = objectType.GenericTypeArguments[1];

		return typeof(SnowflakeObject).GetTypeInfo().IsAssignableFrom(valueParam.GetTypeInfo()) ||
		       valueParam == typeof(DiscordVoiceState);
	}
}
