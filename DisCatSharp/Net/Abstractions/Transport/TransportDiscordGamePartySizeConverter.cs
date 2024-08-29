using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents a game party size converter.
/// </summary>
internal sealed class TransportDiscordGamePartySizeConverter : JsonConverter
{
	/// <summary>
	///     Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		var obj = value is TransportDiscordGamePartySize sinfo
			? new object[] { sinfo.Current, sinfo.Maximum }
			: null;
		serializer.Serialize(writer, obj);
	}

	/// <summary>
	///     Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var arr = this.ReadArrayObject(reader, serializer);
		return new TransportDiscordGamePartySize
		{
			Current = (long)arr[0],
			Maximum = (long)arr[1]
		};
	}

	/// <summary>
	///     Reads the array object.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="serializer">The serializer.</param>
	private JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer)
		=> serializer.Deserialize<JToken>(reader) is not JArray { Count: 2 } arr
			? throw new JsonSerializationException("Expected array of length 2")
			: arr;

	/// <summary>
	///     Whether it can convert.
	/// </summary>
	/// <param name="objectType">The object type.</param>
	public override bool CanConvert(Type objectType)
		=> objectType == typeof(TransportDiscordGamePartySize);
}
