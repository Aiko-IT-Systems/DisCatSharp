// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp.Net.Serialization
{
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
        /// <param name="Writer">The writer.</param>
        /// <param name="Value">The value.</param>
        /// <param name="Serializer">The serializer.</param>
        public override void WriteJson(JsonWriter Writer, object Value, JsonSerializer Serializer) => throw new NotImplementedException();

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="Reader">The reader.</param>
        /// <param name="ObjectType">The object type.</param>
        /// <param name="ExistingValue">The existing value.</param>
        /// <param name="Serializer">The serializer.</param>
        public override object ReadJson(JsonReader Reader, Type ObjectType, object ExistingValue, JsonSerializer Serializer)
        {
            if (Reader.TokenType == JsonToken.Null)
                return null;

            var job = JObject.Load(Reader);
            var type = job["type"]?.ToObject<ComponentType>();

            if (type == null)
                throw new ArgumentException($"Value {Reader} does not have a component type specifier");

            var cmp = type switch
            {
                ComponentType.ActionRow => new DiscordActionRowComponent(),
                ComponentType.Button => new DiscordButtonComponent(),
                ComponentType.Select => new DiscordSelectComponent(),
                ComponentType.InputText => new DiscordTextComponent(),
                _ => new DiscordComponent() { Type = type.Value }
            };

            // Populate the existing component with the values in the JObject. This avoids a recursive JsonConverter loop
            using var jreader = job.CreateReader();
            Serializer.Populate(jreader, cmp);

            return cmp;
        }

        /// <summary>
        /// Whether the json can convert.
        /// </summary>
        /// <param name="ObjectType">The object type.</param>
        public override bool CanConvert(Type ObjectType) => typeof(DiscordComponent).IsAssignableFrom(ObjectType);
    }
}
