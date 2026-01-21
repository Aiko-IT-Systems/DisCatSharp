using System;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Serialization;

/// <summary>
///     Represents a label component json converter.
/// </summary>
internal sealed class ILabelComponentJsonConverter : JsonConverter
{
	/// <inheritdoc />
    public override bool CanWrite => false;

	/// <inheritdoc />
    public override bool CanConvert(Type objectType)
        => typeof(ILabelComponent).IsAssignableFrom(objectType);

	/// <inheritdoc />
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var job = JObject.Load(reader);
        // You may need a more robust way to determine the type
        var type = job["type"]?.ToObject<ComponentType>() ?? throw new JsonSerializationException("Missing type discriminator for ILabelComponent.");

        ILabelComponent cmp = type switch
        {
            ComponentType.TextInput => job.ToObject<DiscordTextInputComponent>(serializer),
            ComponentType.StringSelect => job.ToObject<DiscordStringSelectComponent>(serializer),
            ComponentType.UserSelect => job.ToObject<DiscordUserSelectComponent>(serializer),
			ComponentType.RoleSelect => job.ToObject<DiscordRoleSelectComponent>(serializer),
			ComponentType.ChannelSelect => job.ToObject<DiscordChannelSelectComponent>(serializer),
			ComponentType.MentionableSelect => job.ToObject<DiscordMentionableSelectComponent>(serializer),
			ComponentType.FileUpload => job.ToObject<DiscordFileUploadComponent>(serializer),
            ComponentType.RadioGroup => job.ToObject<DiscordRadioGroupComponent>(serializer),
            ComponentType.CheckboxGroup => job.ToObject<DiscordCheckboxGroupComponent>(serializer),
            ComponentType.Checkbox => job.ToObject<DiscordCheckboxComponent>(serializer),
			_ => throw new JsonSerializationException($"Unknown ILabelComponent type: {type}")
        };

        return cmp;
    }

	/// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => throw new NotImplementedException();
}
