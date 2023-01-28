// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
	public override bool CanWrite => false;

	/// <summary>
	/// Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

	/// <summary>
	/// Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
			return null;

		var job = JObject.Load(reader);
		var type = job["type"]?.ToObject<ComponentType>();

		if (type == null)
			throw new ArgumentException($"Value {reader} does not have a component type specifier");

		DiscordComponent cmp;
		cmp = type switch
		{
			ComponentType.ActionRow => new DiscordActionRowComponent(),
			ComponentType.Button when (string)job["url"] is not null => new DiscordLinkButtonComponent(),
			ComponentType.Button => new DiscordButtonComponent(),
			ComponentType.StringSelect => new DiscordStringSelectComponent(),
			ComponentType.InputText => new DiscordTextComponent(),
			ComponentType.UserSelect => new DiscordUserSelectComponent(),
			ComponentType.RoleSelect => new DiscordRoleSelectComponent(),
			ComponentType.MentionableSelect => new DiscordMentionableSelectComponent(),
			ComponentType.ChannelSelect => new DiscordChannelSelectComponent(),
			_ => new DiscordComponent() { Type = type.Value }
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
	public override bool CanConvert(Type objectType) => typeof(DiscordComponent).IsAssignableFrom(objectType);
}
