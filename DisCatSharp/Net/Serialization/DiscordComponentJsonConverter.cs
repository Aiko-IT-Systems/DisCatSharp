using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Serialization;

/// <summary>
///     Represents a discord component json converter.
/// </summary>
internal sealed class DiscordComponentJsonConverter : JsonConverter
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
		if (reader.TokenType is JsonToken.Null)
			return null;

		var job = JObject.Load(reader);
		var type = job["type"]?.ToObject<ComponentType>() ?? throw new ArgumentException($"Value {reader} does not has a component type specifier");
		DiscordComponent cmp = type switch
		{
			ComponentType.ActionRow => new DiscordActionRowComponent(),
			ComponentType.Button when (string?)job["url"] is not null => new DiscordLinkButtonComponent(),
			ComponentType.Button when (string?)job["sku_id"] is not null => new DiscordPremiumButtonComponent(),
			ComponentType.Button => new DiscordButtonComponent(),
			ComponentType.StringSelect => new DiscordStringSelectComponent(),
			ComponentType.TextInput => new DiscordTextInputComponent(),
			ComponentType.UserSelect => new DiscordUserSelectComponent(),
			ComponentType.RoleSelect => new DiscordRoleSelectComponent(),
			ComponentType.MentionableSelect => new DiscordMentionableSelectComponent(),
			ComponentType.ChannelSelect => new DiscordChannelSelectComponent(),
			ComponentType.Section => new DiscordSectionComponent(),
			ComponentType.TextDisplay => new DiscordTextDisplayComponent(),
			ComponentType.Thumbnail => new DiscordThumbnailComponent(),
			ComponentType.MediaGallery => new DiscordMediaGalleryComponent(),
			ComponentType.File => new DiscordFileDisplayComponent(),
			ComponentType.Separator => new DiscordSeparatorComponent(),
			ComponentType.Label => new DiscordLabelComponent(),
			ComponentType.Container => new DiscordContainerComponent(),
			ComponentType.FileUpload => new DiscordFileUploadComponent(),
			ComponentType.RadioGroup => new DiscordRadioGroupComponent(),
			ComponentType.CheckboxGroup => new DiscordCheckboxGroupComponent(),
			ComponentType.Checkbox => new DiscordCheckboxComponent(),
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

	/// <inheritdoc />
	public override bool CanConvert(Type objectType)
		=> typeof(DiscordComponent).IsAssignableFrom(objectType);
}
