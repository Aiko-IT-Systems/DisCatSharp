using System;

using DisCatSharp.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Serialization;

/// <summary>
///     Handles Discord SKU names that may be emitted either as a legacy string or as a localized object payload.
/// </summary>
internal sealed class DiscordSkuNameJsonConverter : JsonConverter
{
	/// <inheritdoc />
	public override bool CanWrite
		=> false;

	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		=> throw new NotImplementedException();

	/// <inheritdoc />
	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		return reader.TokenType switch
		{
			JsonToken.Null => null,
			JsonToken.String => new DiscordSkuName
			{
				Default = (string?)reader.Value
			},
			JsonToken.StartObject => ReadObject(reader, existingValue as DiscordSkuName, serializer),
			_ => throw new JsonSerializationException($"Unexpected token {reader.TokenType} when deserializing {nameof(DiscordSkuName)}.")
		};
	}

	/// <inheritdoc />
	public override bool CanConvert(Type objectType)
		=> objectType == typeof(DiscordSkuName);

	private static DiscordSkuName ReadObject(JsonReader reader, DiscordSkuName? existingValue, JsonSerializer serializer)
	{
		var result = existingValue ?? new DiscordSkuName();
		var job = JObject.Load(reader);

		using var objectReader = job.CreateReader();
		serializer.Populate(objectReader, result);

		return result;
	}
}
