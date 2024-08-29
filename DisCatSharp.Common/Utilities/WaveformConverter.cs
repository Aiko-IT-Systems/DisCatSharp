using System;

using Newtonsoft.Json;

namespace DisCatSharp.Common.Utilities;

/// <inheritdoc />
public sealed class WaveformConverter : JsonConverter<byte[]?>
{
	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
	{
		if (value is not null)
			writer.WriteValue(Convert.ToBase64String(value));
		else
			writer.WriteNull();
	}

	/// <inheritdoc />
	public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType is not JsonToken.String)
			return existingValue;

		var base64String = (string?)reader.Value;
		if (base64String is null)
			return null;

		try
		{
			return Convert.FromBase64String(base64String);
		}
		catch (FormatException)
		{
			return existingValue;
		}
	}
}
