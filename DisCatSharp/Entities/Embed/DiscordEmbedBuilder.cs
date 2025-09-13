using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DisCatSharp.Entities;

/// <summary>
///     Constructs embeds.
/// </summary>
public sealed class DiscordEmbedBuilder
{
	/// <summary>
	/// 
	/// </summary>
	private readonly List<DiscordEmbedField> _fields = [];

	/// <summary>
	/// 
	/// </summary>
	private string? _description;

	/// <summary>
	/// 
	/// </summary>
	private DiscordUri? _imageUri;

	/// <summary>
	/// 
	/// </summary>
	private string? _title;

	/// <summary>
	/// 
	/// </summary>
	private Uri? _url;

	/// <summary>
	///     Constructs a new empty embed builder.
	/// </summary>
	public DiscordEmbedBuilder()
	{
		this.Fields = new ReadOnlyCollection<DiscordEmbedField>(this._fields);
	}

	/// <summary>
	///     Constructs a new embed builder using another embed as prototype.
	/// </summary>
	/// <param name="original">Embed to use as prototype.</param>
	public DiscordEmbedBuilder(DiscordEmbed original)
		: this()
	{
		this.Title = original.Title;
		this.Description = original.Description;
		this.Url = original.Url?.ToString();
		this.ImageUrl = original.Image?.Url?.ToString();
		this.Color = original.Color;
		this.Timestamp = original.Timestamp;

		if (original.Thumbnail is not null)
			this.Thumbnail = new()
			{
				Url = original.Thumbnail.Url?.ToString(),
				Height = original.Thumbnail.Height,
				Width = original.Thumbnail.Width
			};

		if (original.Author is not null)
			this.Author = new()
			{
				IconUrl = original.Author.IconUrl?.ToString(),
				Name = original.Author.Name,
				Url = original.Author.Url?.ToString()
			};

		if (original.Footer is not null)
			this.Footer = new()
			{
				IconUrl = original.Footer.IconUrl?.ToString(),
				Text = original.Footer.Text
			};

		if (original.Fields.Any())
			this._fields.AddRange(original.Fields);

		while (this._fields.Count > 25)
			this._fields.RemoveAt(this._fields.Count - 1);
	}

	/// <summary>
	///     Gets or sets the embed's title.
	/// </summary>
	public string? Title
	{
		get => this._title;
		set
		{
			if (value is { Length: > 256 })
				throw new ArgumentException("Title length cannot exceed 256 characters.", nameof(value));

			this._title = value;
		}
	}

	/// <summary>
	///     Gets or sets the embed's description.
	/// </summary>
	public string? Description
	{
		get => this._description;
		set
		{
			if (value is { Length: > 4096 })
				throw new ArgumentException("Description length cannot exceed 4096 characters.", nameof(value));

			this._description = value;
		}
	}

	/// <summary>
	///     Gets or sets the url for the embed's title.
	/// </summary>
	public string? Url
	{
		get => this._url?.ToString();
		set => this._url = string.IsNullOrEmpty(value) ? null : new Uri(value);
	}

	/// <summary>
	///     Gets or sets the embed's color.
	/// </summary>
	public Optional<DiscordColor> Color { get; set; }

	/// <summary>
	///     Gets or sets the embed's timestamp.
	/// </summary>
	public DateTimeOffset? Timestamp { get; set; }

	/// <summary>
	///     Gets or sets the embed's image url.
	/// </summary>
	public string? ImageUrl
	{
		get => this._imageUri?.ToString();
		set => this._imageUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
	}

	/// <summary>
	///     Gets or sets the embed's author.
	/// </summary>
	public EmbedAuthor? Author { get; set; }

	/// <summary>
	///     Gets or sets the embed's footer.
	/// </summary>
	public EmbedFooter? Footer { get; set; }

	/// <summary>
	///     Gets or sets the embed's thumbnail.
	/// </summary>
	public EmbedThumbnail? Thumbnail { get; set; }

	/// <summary>
	///     Gets the embed's fields.
	/// </summary>
	public IReadOnlyList<DiscordEmbedField>? Fields { get; }

	/// <summary>
	///     Sets the embed's title.
	/// </summary>
	/// <param name="title">Title to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithTitle(string title)
	{
		this.Title = title;
		return this;
	}

	/// <summary>
	///     Sets the embed's description.
	/// </summary>
	/// <param name="description">Description to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithDescription(string description)
	{
		this.Description = description;
		return this;
	}

	/// <summary>
	///     Sets the embed's title url.
	/// </summary>
	/// <param name="url">Title url to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithUrl(string url)
	{
		this.Url = url;
		return this;
	}

	/// <summary>
	///     Sets the embed's title url.
	/// </summary>
	/// <param name="url">Title url to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithUrl(Uri url)
	{
		this._url = url;
		return this;
	}

	/// <summary>
	///     Sets the embed's color.
	/// </summary>
	/// <param name="color">Embed color to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithColor(DiscordColor color)
	{
		this.Color = color;
		return this;
	}

	/// <summary>
	///     Sets the embed's timestamp.
	/// </summary>
	/// <param name="timestamp">Timestamp to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithTimestamp(DateTimeOffset? timestamp)
	{
		this.Timestamp = timestamp;
		return this;
	}

	/// <summary>
	///     Sets the embed's timestamp.
	/// </summary>
	/// <param name="timestamp">Timestamp to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithTimestamp(DateTime? timestamp)
	{
		this.Timestamp = timestamp == null ? null : new DateTimeOffset(timestamp.Value);
		return this;
	}

	/// <summary>
	///     Sets the embed's timestamp based on a snowflake.
	/// </summary>
	/// <param name="snowflake">Snowflake to calculate timestamp from.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithTimestamp(ulong snowflake)
	{
		this.Timestamp = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(snowflake >> 22);
		return this;
	}

	/// <summary>
	///     Sets the embed's image url.
	/// </summary>
	/// <param name="url">Image url to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithImageUrl(string url)
	{
		this.ImageUrl = url;
		return this;
	}

	/// <summary>
	///     Sets the embed's image url.
	/// </summary>
	/// <param name="url">Image url to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithImageUrl(Uri url)
	{
		this._imageUri = new(url);
		return this;
	}

	/// <summary>
	///     Sets the embed's thumbnail.
	/// </summary>
	/// <param name="url">Thumbnail url to set.</param>
	/// <param name="height">The height of the thumbnail to set.</param>
	/// <param name="width">The width of the thumbnail to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithThumbnail(string url, int height = 0, int width = 0)
	{
		this.Thumbnail = new()
		{
			Url = url,
			Height = height,
			Width = width
		};

		return this;
	}

	/// <summary>
	///     Sets the embed's thumbnail.
	/// </summary>
	/// <param name="url">Thumbnail url to set.</param>
	/// <param name="height">The height of the thumbnail to set.</param>
	/// <param name="width">The width of the thumbnail to set.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithThumbnail(Uri url, int height = 0, int width = 0)
	{
		this.Thumbnail = new()
		{
			Uri = new(url),
			Height = height,
			Width = width
		};

		return this;
	}

	/// <summary>
	///     Sets the embed's author.
	/// </summary>
	/// <param name="name">Author's name.</param>
	/// <param name="url">Author's url.</param>
	/// <param name="iconUrl">Author icon's url.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithAuthor(string? name = null, string? url = null, string? iconUrl = null)
	{
		if (!string.IsNullOrEmpty(name) && name.Length > 256)
			throw new NotSupportedException("Embed author name can not exceed 256 chars. See https://discord.com/developers/docs/resources/channel#embed-limits.");

		this.Author = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url) && string.IsNullOrEmpty(iconUrl)
			? null
			: new EmbedAuthor
			{
				Name = name,
				Url = url,
				IconUrl = iconUrl
			};
		return this;
	}

	/// <summary>
	///     Sets the embed's footer.
	/// </summary>
	/// <param name="text">Footer's text.</param>
	/// <param name="iconUrl">Footer icon's url.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder WithFooter(string? text = null, string? iconUrl = null)
	{
		if (text is { Length: > 2048 })
			throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(text));

		this.Footer = string.IsNullOrEmpty(text) && string.IsNullOrEmpty(iconUrl)
			? null
			: new EmbedFooter
			{
				Text = text,
				IconUrl = iconUrl
			};
		return this;
	}

	/// <summary>
	///     Adds a field to this embed.
	/// </summary>
	/// <param name="field">The field to add.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder AddField(DiscordEmbedField field)
	{
		if (this._fields.Count >= 25)
			throw new InvalidOperationException("Cannot add more than 25 fields.");

		if (string.IsNullOrWhiteSpace(field.Name))
		{
			ArgumentNullException.ThrowIfNull(field.Name);

			throw new ArgumentException("Name cannot be empty or whitespace.", nameof(field));
		}

		if (field.Name.Length > 256)
			throw new ArgumentException("Embed field name length cannot exceed 256 characters.", nameof(field));

		if (string.IsNullOrWhiteSpace(field.Value))
		{
			ArgumentNullException.ThrowIfNull(field.Value);

			throw new ArgumentException("Value cannot be empty or whitespace.", nameof(field));
		}

		if (field.Value.Length > 1024)
			throw new ArgumentException("Embed field value length cannot exceed 1024 characters.", nameof(field));

		this._fields.Add(field);

		return this;
	}

	/// <summary>
	///     Adds multiple fields to this embed.
	/// </summary>
	/// <param name="fields">The fields to add.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder AddFields(params DiscordEmbedField[] fields)
		=> this.AddFields((IEnumerable<DiscordEmbedField>)fields);

	/// <summary>
	///     Adds multiple fields to this embed.
	/// </summary>
	/// <param name="fields">The fields to add.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder AddFields(IEnumerable<DiscordEmbedField> fields)
		=> fields.Aggregate(this, (x, y) => x.AddField(y));

	/// <summary>
	///     Removes a field from this embed, if it is part of it.
	/// </summary>
	/// <param name="field">The field to remove.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder RemoveField(DiscordEmbedField field)
	{
		this._fields.Remove(field);
		return this;
	}

	/// <summary>
	///     Removes multiple fields from this embed, if they are part of it.
	/// </summary>
	/// <param name="fields">The fields to remove.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder RemoveFields(params DiscordEmbedField[] fields)
	{
		this.RemoveFields((IEnumerable<DiscordEmbedField>)fields);
		return this;
	}

	/// <summary>
	///     Removes multiple fields from this embed, if they are part of it.
	/// </summary>
	/// <param name="fields">The fields to remove.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder RemoveFields(IEnumerable<DiscordEmbedField> fields)
	{
		this._fields.RemoveAll(fields.Contains);
		return this;
	}

	/// <summary>
	///     Removes a field of the specified index from this embed.
	/// </summary>
	/// <param name="index">Index of the field to remove.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder RemoveFieldAt(int index)
	{
		this._fields.RemoveAt(index);
		return this;
	}

	/// <summary>
	///     Removes fields of the specified range from this embed.
	/// </summary>
	/// <param name="index">Index of the first field to remove.</param>
	/// <param name="count">Number of fields to remove.</param>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder RemoveFieldRange(int index, int count)
	{
		this._fields.RemoveRange(index, count);
		return this;
	}

	/// <summary>
	///     Removes all fields from this embed.
	/// </summary>
	/// <returns>This embed builder.</returns>
	public DiscordEmbedBuilder ClearFields()
	{
		this._fields.Clear();
		return this;
	}

	/// <summary>
	///     Constructs a new embed from data supplied to this builder.
	/// </summary>
	/// <returns>New discord embed.</returns>
	public DiscordEmbed Build()
	{
		var embed = new DiscordEmbed
		{
			Title = this._title,
			Description = this._description,
			Url = this._url,
			ColorInternal = this.Color.Map(e => e.Value),
			Timestamp = this.Timestamp
		};

		if (this.Footer is not null)
			embed.Footer = new()
			{
				Text = this.Footer.Text,
				IconUrl = this.Footer.IconUri
			};

		if (this.Author is not null)
			embed.Author = new()
			{
				Name = this.Author.Name,
				Url = this.Author.Uri,
				IconUrl = this.Author.IconUri
			};

		if (this._imageUri is not null)
			embed.Image = new()
			{
				Url = this._imageUri
			};
		if (this.Thumbnail?.Uri is not null)
			embed.Thumbnail = new()
			{
				Url = this.Thumbnail.Uri,
				Height = this.Thumbnail.Height,
				Width = this.Thumbnail.Width
			};

		embed.Fields = new ReadOnlyCollection<DiscordEmbedField>([.. this._fields]);

		var charCount = 0;
		if (embed.Fields.Any())
			foreach (var field in embed.Fields)
			{
				charCount += field.Name.Length;
				charCount += field.Value.Length;
			}

		if (embed.Author is not null && !string.IsNullOrEmpty(embed.Author.Name))
			charCount += embed.Author.Name.Length;

		if (embed.Footer is not null && !string.IsNullOrEmpty(embed.Footer.Text))
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
	///     Implicitly converts this builder to an embed.
	/// </summary>
	/// <param name="builder">Builder to convert.</param>
	public static implicit operator DiscordEmbed(DiscordEmbedBuilder builder)
		=> builder is not null ? builder.Build() : throw new NullReferenceException("Argument builder was null");

	/// <summary>
	///     Represents an embed author.
	/// </summary>
	public sealed class EmbedAuthor
	{
		private string? _name;

		internal DiscordUri? IconUri;

		internal Uri? Uri;

		/// <summary>
		///     Gets or sets the name of the author.
		/// </summary>
		public string? Name
		{
			get => this._name;
			set
			{
				if (value is { Length: > 256 })
					throw new ArgumentException("Author name length cannot exceed 256 characters.", nameof(value));

				this._name = value;
			}
		}

		/// <summary>
		///     Gets or sets the Url to which the author's link leads.
		/// </summary>
		public string? Url
		{
			get => this.Uri?.ToString();
			set => this.Uri = string.IsNullOrEmpty(value) ? null : new Uri(value);
		}

		/// <summary>
		///     Gets or sets the Author's icon url.
		/// </summary>
		public string? IconUrl
		{
			get => this.IconUri?.ToString();
			set => this.IconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
		}
	}

	/// <summary>
	///     Represents an embed footer.
	/// </summary>
	public sealed class EmbedFooter
	{
		private string? _text;

		internal DiscordUri? IconUri;

		/// <summary>
		///     Gets or sets the text of the footer.
		/// </summary>
		public string? Text
		{
			get => this._text;
			set
			{
				if (value is { Length: > 2048 })
					throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(value));

				this._text = value;
			}
		}

		/// <summary>
		///     Gets or sets the Url
		/// </summary>
		public string? IconUrl
		{
			get => this.IconUri?.ToString();
			set => this.IconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
		}
	}

	/// <summary>
	///     Represents an embed thumbnail.
	/// </summary>
	public sealed class EmbedThumbnail
	{
		private int? _height;

		private int? _width;

		internal DiscordUri? Uri;

		/// <summary>
		///     Gets or sets the thumbnail's image url.
		/// </summary>
		public string? Url
		{
			get => this.Uri?.ToString();
			set => this.Uri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
		}

		/// <summary>
		///     Gets or sets the thumbnail's height.
		/// </summary>
		public int? Height
		{
			get => this._height;
			set => this._height = value >= 0 ? value : 0;
		}

		/// <summary>
		///     Gets or sets the thumbnail's width.
		/// </summary>
		public int? Width
		{
			get => this._width;
			set => this._width = value >= 0 ? value : 0;
		}
	}
}
