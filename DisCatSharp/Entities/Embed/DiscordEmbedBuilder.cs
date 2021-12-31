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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DisCatSharp.Net;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Constructs embeds.
    /// </summary>
    public sealed class DiscordEmbedBuilder
    {
        /// <summary>
        /// Gets or sets the embed's title.
        /// </summary>
        public string Title
        {
            get => this._title;
            set
            {
                if (value != null && value.Length > 256)
                    throw new ArgumentException("Title length cannot exceed 256 characters.", nameof(value));
                this._title = value;
            }
        }
        private string _title;

        /// <summary>
        /// Gets or sets the embed's description.
        /// </summary>
        public string Description
        {
            get => this._description;
            set
            {
                if (value != null && value.Length > 4096)
                    throw new ArgumentException("Description length cannot exceed 4096 characters.", nameof(value));
                this._description = value;
            }
        }
        private string _description;

        /// <summary>
        /// Gets or sets the url for the embed's title.
        /// </summary>
        public string Url
        {
            get => this._url?.ToString();
            set => this._url = string.IsNullOrEmpty(value) ? null : new Uri(value);
        }
        private Uri _url;

        /// <summary>
        /// Gets or sets the embed's color.
        /// </summary>
        public Optional<DiscordColor> Color { get; set; }

        /// <summary>
        /// Gets or sets the embed's timestamp.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the embed's image url.
        /// </summary>
        public string ImageUrl
        {
            get => this._imageUri?.ToString();
            set => this._imageUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        private DiscordUri _imageUri;

        /// <summary>
        /// Gets or sets the embed's author.
        /// </summary>
        public EmbedAuthor Author { get; set; }

        /// <summary>
        /// Gets or sets the embed's footer.
        /// </summary>
        public EmbedFooter Footer { get; set; }

        /// <summary>
        /// Gets or sets the embed's thumbnail.
        /// </summary>
        public EmbedThumbnail Thumbnail { get; set; }

        /// <summary>
        /// Gets the embed's fields.
        /// </summary>
        public IReadOnlyList<DiscordEmbedField> Fields { get; }
        private readonly List<DiscordEmbedField> _fields = new();

        /// <summary>
        /// Constructs a new empty embed builder.
        /// </summary>
        public DiscordEmbedBuilder()
        {
            this.Fields = new ReadOnlyCollection<DiscordEmbedField>(this._fields);
        }

        /// <summary>
        /// Constructs a new embed builder using another embed as prototype.
        /// </summary>
        /// <param name="Original">Embed to use as prototype.</param>
        public DiscordEmbedBuilder(DiscordEmbed Original)
            : this()
        {
            this.Title = Original.Title;
            this.Description = Original.Description;
            this.Url = Original.Url?.ToString();
            this.ImageUrl = Original.Image?.Url?.ToString();
            this.Color = Original.Color;
            this.Timestamp = Original.Timestamp;

            if (Original.Thumbnail != null)
                this.Thumbnail = new EmbedThumbnail
                {
                    Url = Original.Thumbnail.Url?.ToString(),
                    Height = Original.Thumbnail.Height,
                    Width = Original.Thumbnail.Width
                };

            if (Original.Author != null)
                this.Author = new EmbedAuthor
                {
                    IconUrl = Original.Author.IconUrl?.ToString(),
                    Name = Original.Author.Name,
                    Url = Original.Author.Url?.ToString()
                };

            if (Original.Footer != null)
                this.Footer = new EmbedFooter
                {
                    IconUrl = Original.Footer.IconUrl?.ToString(),
                    Text = Original.Footer.Text
                };

            if (Original.Fields?.Any() == true)
                this._fields.AddRange(Original.Fields);

            while (this._fields.Count > 25)
                this._fields.RemoveAt(this._fields.Count - 1);
        }

        /// <summary>
        /// Sets the embed's title.
        /// </summary>
        /// <param name="Title">Title to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTitle(string Title)
        {
            this.Title = Title;
            return this;
        }

        /// <summary>
        /// Sets the embed's description.
        /// </summary>
        /// <param name="Description">Description to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithDescription(string Description)
        {
            this.Description = Description;
            return this;
        }

        /// <summary>
        /// Sets the embed's title url.
        /// </summary>
        /// <param name="Url">Title url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithUrl(string Url)
        {
            this.Url = Url;
            return this;
        }

        /// <summary>
        /// Sets the embed's title url.
        /// </summary>
        /// <param name="Url">Title url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithUrl(Uri Url)
        {
            this._url = Url;
            return this;
        }

        /// <summary>
        /// Sets the embed's color.
        /// </summary>
        /// <param name="Color">Embed color to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithColor(DiscordColor Color)
        {
            this.Color = Color;
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp.
        /// </summary>
        /// <param name="Timestamp">Timestamp to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(DateTimeOffset? Timestamp)
        {
            this.Timestamp = Timestamp;
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp.
        /// </summary>
        /// <param name="Timestamp">Timestamp to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(DateTime? Timestamp)
        {
            this.Timestamp = Timestamp == null ? null : (DateTimeOffset?)new DateTimeOffset(Timestamp.Value);
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp based on a snowflake.
        /// </summary>
        /// <param name="Snowflake">Snowflake to calculate timestamp from.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(ulong Snowflake)
        {
            this.Timestamp = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Snowflake >> 22);
            return this;
        }

        /// <summary>
        /// Sets the embed's image url.
        /// </summary>
        /// <param name="Url">Image url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithImageUrl(string Url)
        {
            this.ImageUrl = Url;
            return this;
        }

        /// <summary>
        /// Sets the embed's image url.
        /// </summary>
        /// <param name="Url">Image url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithImageUrl(Uri Url)
        {
            this._imageUri = new DiscordUri(Url);
            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail.
        /// </summary>
        /// <param name="Url">Thumbnail url to set.</param>
        /// <param name="Height">The height of the thumbnail to set.</param>
        /// <param name="Width">The width of the thumbnail to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnail(string Url, int Height = 0, int Width = 0)
        {
            this.Thumbnail = new EmbedThumbnail
            {
                Url = Url,
                Height = Height,
                Width = Width
            };

            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail.
        /// </summary>
        /// <param name="Url">Thumbnail url to set.</param>
        /// <param name="Height">The height of the thumbnail to set.</param>
        /// <param name="Width">The width of the thumbnail to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnail(Uri Url, int Height = 0, int Width = 0)
        {
            this.Thumbnail = new EmbedThumbnail
            {
                _uri = new DiscordUri(Url),
                Height = Height,
                Width = Width
            };

            return this;
        }

        /// <summary>
        /// Sets the embed's author.
        /// </summary>
        /// <param name="Name">Author's name.</param>
        /// <param name="Url">Author's url.</param>
        /// <param name="IconUrl">Author icon's url.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithAuthor(string Name = null, string Url = null, string IconUrl = null)
        {
            if (!string.IsNullOrEmpty(Name) && Name.Length > 256)
                throw new NotSupportedException("Embed author name can not exceed 256 chars. See https://discord.com/developers/docs/resources/channel#embed-limits.");
            this.Author = string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Url) && string.IsNullOrEmpty(IconUrl)
                ? null
                : new EmbedAuthor
                {
                    Name = Name,
                    Url = Url,
                    IconUrl = IconUrl
                };
            return this;
        }

        /// <summary>
        /// Sets the embed's footer.
        /// </summary>
        /// <param name="Text">Footer's text.</param>
        /// <param name="IconUrl">Footer icon's url.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithFooter(string Text = null, string IconUrl = null)
        {
            if (Text != null && Text.Length > 2048)
                throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(Text));

            this.Footer = string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(IconUrl)
                ? null
                : new EmbedFooter
                {
                    Text = Text,
                    IconUrl = IconUrl
                };
            return this;
        }

        /// <summary>
        /// Adds a field to this embed.
        /// </summary>
        /// <param name="Name">Name of the field to add.</param>
        /// <param name="Value">Value of the field to add.</param>
        /// <param name="Inline">Whether the field is to be inline or not.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder AddField(string Name, string Value, bool Inline = false)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                if (Name == null)
                    throw new ArgumentNullException(nameof(Name));
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(Name));
            }
            if (string.IsNullOrWhiteSpace(Value))
            {
                if (Value == null)
                    throw new ArgumentNullException(nameof(Value));
                throw new ArgumentException("Value cannot be empty or whitespace.", nameof(Value));
            }

            if (Name.Length > 256)
                throw new ArgumentException("Embed field name length cannot exceed 256 characters.");
            if (Value.Length > 1024)
                throw new ArgumentException("Embed field value length cannot exceed 1024 characters.");

            if (this._fields.Count >= 25)
                throw new InvalidOperationException("Cannot add more than 25 fields.");

            this._fields.Add(new DiscordEmbedField
            {
                Inline = Inline,
                Name = Name,
                Value = Value
            });
            return this;
        }

        /// <summary>
        /// Removes a field of the specified index from this embed.
        /// </summary>
        /// <param name="Index">Index of the field to remove.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder RemoveFieldAt(int Index)
        {
            this._fields.RemoveAt(Index);
            return this;
        }

        /// <summary>
        /// Removes fields of the specified range from this embed.
        /// </summary>
        /// <param name="Index">Index of the first field to remove.</param>
        /// <param name="Count">Number of fields to remove.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder RemoveFieldRange(int Index, int Count)
        {
            this._fields.RemoveRange(Index, Count);
            return this;
        }

        /// <summary>
        /// Removes all fields from this embed.
        /// </summary>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder ClearFields()
        {
            this._fields.Clear();
            return this;
        }

        /// <summary>
        /// Constructs a new embed from data supplied to this builder.
        /// </summary>
        /// <returns>New discord embed.</returns>
        public DiscordEmbed Build()
        {
            var embed = new DiscordEmbed
            {
                Title = this._title,
                Description = this._description,
                Url = this._url,
                _color = this.Color.IfPresent(E => E.Value),
                Timestamp = this.Timestamp
            };

            if (this.Footer != null)
                embed.Footer = new DiscordEmbedFooter
                {
                    Text = this.Footer.Text,
                    IconUrl = this.Footer._iconUri
                };

            if (this.Author != null)
                embed.Author = new DiscordEmbedAuthor
                {
                    Name = this.Author.Name,
                    Url = this.Author._uri,
                    IconUrl = this.Author._iconUri
                };

            if (this._imageUri != null)
                embed.Image = new DiscordEmbedImage { Url = this._imageUri };
            if (this.Thumbnail != null)
                embed.Thumbnail = new DiscordEmbedThumbnail
                {
                    Url = this.Thumbnail._uri,
                    Height = this.Thumbnail.Height,
                    Width = this.Thumbnail.Width
                };

            embed.Fields = new ReadOnlyCollection<DiscordEmbedField>(new List<DiscordEmbedField>(this._fields)); // copy the list, don't wrap it, prevents mutation

            var charCount = 0;
            if (embed.Fields.Any())
            {
                foreach (var field in embed.Fields)
                {
                    charCount += field.Name.Length;
                    charCount += field.Value.Length;
                }
            }

            if (embed.Author != null && !string.IsNullOrEmpty(embed.Author.Name))
                charCount += embed.Author.Name.Length;

            if (embed.Footer != null && !string.IsNullOrEmpty(embed.Footer.Text))
                charCount += embed.Footer.Text.Length;

            if (!string.IsNullOrEmpty(embed.Title))
                charCount += embed.Title.Length;

            if (!string.IsNullOrEmpty(embed.Description))
                charCount += embed.Description.Length;

            return charCount >= 6000
                ? throw new NotSupportedException("Total char count can not exceed 6000 chars. See https://discord.com/developers/docs/resources/channel#embed-limits.")
                : embed;
        }

        /// <summary>
        /// Implicitly converts this builder to an embed.
        /// </summary>
        /// <param name="Builder">Builder to convert.</param>
        public static implicit operator DiscordEmbed(DiscordEmbedBuilder Builder)
            => Builder?.Build();

        /// <summary>
        /// Represents an embed author.
        /// </summary>
        public class EmbedAuthor
        {
            /// <summary>
            /// Gets or sets the name of the author.
            /// </summary>
            public string Name
            {
                get => this._name;
                set
                {
                    if (value != null && value.Length > 256)
                        throw new ArgumentException("Author name length cannot exceed 256 characters.", nameof(value));
                    this._name = value;
                }
            }
            private string _name;

            /// <summary>
            /// Gets or sets the Url to which the author's link leads.
            /// </summary>
            public string Url
            {
                get => this._uri?.ToString();
                set => this._uri = string.IsNullOrEmpty(value) ? null : new Uri(value);
            }
            internal Uri _uri;

            /// <summary>
            /// Gets or sets the Author's icon url.
            /// </summary>
            public string IconUrl
            {
                get => this._iconUri?.ToString();
                set => this._iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
            }
            internal DiscordUri _iconUri;
        }

        /// <summary>
        /// Represents an embed footer.
        /// </summary>
        public class EmbedFooter
        {
            /// <summary>
            /// Gets or sets the text of the footer.
            /// </summary>
            public string Text
            {
                get => this._text;
                set
                {
                    if (value != null && value.Length > 2048)
                        throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(value));
                    this._text = value;
                }
            }
            private string _text;

            /// <summary>
            /// Gets or sets the Url
            /// </summary>
            public string IconUrl
            {
                get => this._iconUri?.ToString();
                set => this._iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
            }
            internal DiscordUri _iconUri;
        }

        /// <summary>
        /// Represents an embed thumbnail.
        /// </summary>
        public class EmbedThumbnail
        {
            /// <summary>
            /// Gets or sets the thumbnail's image url.
            /// </summary>
            public string Url
            {
                get => this._uri?.ToString();
                set => this._uri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
            }
            internal DiscordUri _uri;

            /// <summary>
            /// Gets or sets the thumbnail's height.
            /// </summary>
            public int Height
            {
                get => this._height;
                set => this._height = value >= 0 ? value : 0;
            }
            private int _height;

            /// <summary>
            /// Gets or sets the thumbnail's width.
            /// </summary>
            public int Width
            {
                get => this._width;
                set => this._width = value >= 0 ? value : 0;
            }
            private int _width;
        }
    }
}
