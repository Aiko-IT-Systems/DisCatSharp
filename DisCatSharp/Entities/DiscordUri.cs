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
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace DisCatSharp.Net
{
    /// <summary>
    /// An URI in a Discord embed doesn't necessarily conform to the RFC 3986. If it uses the <c>attachment://</c>
    /// protocol, it mustn't contain a trailing slash to be interpreted correctly as an embed attachment reference by
    /// Discord.
    /// </summary>
    [JsonConverter(typeof(DiscordUriJsonConverter))]
    public class DiscordUri
    {
        private readonly object _value;

        /// <summary>
        /// The type of this URI.
        /// </summary>
        public DiscordUriType Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordUri"/> class.
        /// </summary>
        /// <param name="Value">The value.</param>
        internal DiscordUri(Uri Value)
        {
            this._value = Value ?? throw new ArgumentNullException(nameof(Value));
            this.Type = DiscordUriType.Standard;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordUri"/> class.
        /// </summary>
        /// <param name="Value">The value.</param>
        internal DiscordUri(string Value)
        {
            if (Value == null)
                throw new ArgumentNullException(nameof(Value));

            if (IsStandard(Value))
            {
                this._value = new Uri(Value);
                this.Type = DiscordUriType.Standard;
            }
            else
            {
                this._value = Value;
                this.Type = DiscordUriType.NonStandard;
            }
        }

        // can be changed in future
        /// <summary>
        /// If the uri is a standard uri
        /// </summary>
        /// <param name="Value">Uri string</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsStandard(string Value) => !Value.StartsWith("attachment://");

        /// <summary>
        /// Returns a string representation of this DiscordUri.
        /// </summary>
        /// <returns>This DiscordUri, as a string.</returns>
        public override string ToString() => this._value.ToString();

        /// <summary>
        /// Converts this DiscordUri into a canonical representation of a <see cref="System.Uri"/> if it can be represented as
        /// such, throwing an exception otherwise.
        /// </summary>
        /// <returns>A canonical representation of this DiscordUri.</returns>
        /// <exception cref="System.UriFormatException">If <see cref="System.Type"/> is not <see cref="DiscordUriType.Standard"/>, as
        /// that would mean creating an invalid Uri, which would result in loss of data.</exception>
        public Uri ToUri()
            => this.Type == DiscordUriType.Standard
                ? this._value as Uri
                : throw new UriFormatException(
                    $@"DiscordUri ""{this._value}"" would be invalid as a regular URI, please the {nameof(this.Type)} property first.");

        /// <summary>
        /// Represents a uri json converter.
        /// </summary>
        internal sealed class DiscordUriJsonConverter : JsonConverter
        {
            /// <summary>
            /// Writes the json.
            /// </summary>
            /// <param name="Writer">The writer.</param>
            /// <param name="Value">The value.</param>
            /// <param name="Serializer">The serializer.</param>
            public override void WriteJson(JsonWriter Writer, object Value, JsonSerializer Serializer) => Writer.WriteValue((Value as DiscordUri)._value);

            /// <summary>
            /// Reads the json.
            /// </summary>
            /// <param name="Reader">The reader.</param>
            /// <param name="ObjectType">The object type.</param>
            /// <param name="ExistingValue">The existing value.</param>
            /// <param name="Serializer">The serializer.</param>
            public override object ReadJson(JsonReader Reader, Type ObjectType, object ExistingValue,
                JsonSerializer Serializer)
            {
                var val = Reader.Value;
                return val == null
                    ? null
                    : val is not string s
                    ? throw new JsonReaderException("DiscordUri value invalid format! This is a bug in DisCatSharp. " +
                                                  $"Include the type in your bug report: [[{Reader.TokenType}]]")
                    : IsStandard(s)
                    ? new DiscordUri(new Uri(s))
                    : new DiscordUri(s);
            }

            /// <summary>
            /// Whether it can be converted.
            /// </summary>
            /// <param name="ObjectType">The object type.</param>
            /// <returns>A bool.</returns>
            public override bool CanConvert(Type ObjectType) => ObjectType == typeof(DiscordUri);
        }
    }

    /// <summary>
    /// Represents a uri type.
    /// </summary>
    public enum DiscordUriType : byte
    {
        /// <summary>
        /// Represents a URI that conforms to RFC 3986, meaning it's stored internally as a <see cref="System.Uri"/> and will
        /// contain a trailing slash after the domain name.
        /// </summary>
        Standard,

        /// <summary>
        /// Represents a URI that does not conform to RFC 3986, meaning it's stored internally as a plain string and
        /// should be treated as one.
        /// </summary>
        NonStandard
    }
}
