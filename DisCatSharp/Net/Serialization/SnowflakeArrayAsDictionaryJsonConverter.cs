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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DisCatSharp.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Serialization
{
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
        /// <param name="Writer">The writer.</param>
        /// <param name="Value">The value.</param>
        /// <param name="Serializer">The serializer.</param>
        public override void WriteJson(JsonWriter Writer, object Value, JsonSerializer Serializer)
        {
            if (Value == null)
            {
                Writer.WriteNull();
            }
            else
            {
                var type = Value.GetType().GetTypeInfo();
                JToken.FromObject(type.GetDeclaredProperty("Values").GetValue(Value)).WriteTo(Writer);
            }
        }

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="Reader">The reader.</param>
        /// <param name="ObjectType">The object type.</param>
        /// <param name="ExistingValue">The existing value.</param>
        /// <param name="Serializer">The serializer.</param>
        public override object ReadJson(JsonReader Reader, Type ObjectType, object ExistingValue, JsonSerializer Serializer)
        {
            var constructor = ObjectType.GetTypeInfo().DeclaredConstructors
                .FirstOrDefault(E => !E.IsStatic && E.GetParameters().Length == 0);

            var dict = constructor.Invoke(new object[] {});

            // the default name of an indexer is "Item"
            var properties = ObjectType.GetTypeInfo().GetDeclaredProperty("Item");

            var entries = (IEnumerable) Serializer.Deserialize(Reader, ObjectType.GenericTypeArguments[1].MakeArrayType());
            foreach (var entry in entries)
            {
                properties.SetValue(dict, entry, new object[]
                {
                    (entry as SnowflakeObject)?.Id
                    ?? (entry as DiscordVoiceState)?.UserId
                    ?? throw new InvalidOperationException($"Type {entry?.GetType()} is not deserializable")
                });
            }

            return dict;
        }

        /// <summary>
        /// Whether the snowflake can be converted.
        /// </summary>
        /// <param name="ObjectType">The object type.</param>
        public override bool CanConvert(Type ObjectType)
        {
            var genericTypedef = ObjectType.GetGenericTypeDefinition();
            if (genericTypedef != typeof(Dictionary<,>) && genericTypedef != typeof(ConcurrentDictionary<,>)) return false;
            if (ObjectType.GenericTypeArguments[0] != typeof(ulong)) return false;

            var valueParam = ObjectType.GenericTypeArguments[1];
            return typeof(SnowflakeObject).GetTypeInfo().IsAssignableFrom(valueParam.GetTypeInfo()) ||
                   valueParam == typeof(DiscordVoiceState);
        }
    }
}
