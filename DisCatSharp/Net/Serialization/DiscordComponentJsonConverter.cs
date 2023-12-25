using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Serialization;

/// <summary>
/// Represents a discord component json converter.
/// </summary>
internal sealed class DiscordComponentJsonConverter : JsonConverter
{
	/// <summary>
	/// Whether the converter can write.
	/// </summary>
	public override bool CanWrite
		=> false;

	/// <summary>
	/// Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		=> throw new NotImplementedException();

	/// <summary>
	/// Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType is JsonToken.Null)
			return null;

		var job = JObject.Load(reader);
		var type = job["type"]?.ToObject<ComponentType>() ?? throw new ArgumentException($"Value {reader} does not have a component type specifier");
		DiscordComponent cmp = type switch
		{
			ComponentType.ActionRow => new DiscordActionRowComponent(),
			ComponentType.Button when (string?)job["url"] is not null => new DiscordLinkButtonComponent(),
			ComponentType.Button => new DiscordButtonComponent(),
			ComponentType.StringSelect => new DiscordStringSelectComponent(),
			ComponentType.InputText => new DiscordTextComponent(),
			ComponentType.UserSelect => new DiscordUserSelectComponent(),
			ComponentType.RoleSelect => new DiscordRoleSelectComponent(),
			ComponentType.MentionableSelect => new DiscordMentionableSelectComponent(),
			ComponentType.ChannelSelect => new DiscordChannelSelectComponent(),
			_ => new()
			{
				Type = type
			}
		};

		// Populate the existing component with the values in the JObject. This avoids a recursive JsonConverter loop
		using var jreader = job.CreateReader();
		serializer.Populate(jreader, cmp);

		return cmp;
	}

	/// <summary>
	/// Whether the json can convert.
	/// </summary>
	/// <param name="objectType">The object type.</param>
	public override bool CanConvert(Type objectType)
		=> typeof(DiscordComponent).IsAssignableFrom(objectType);
}
