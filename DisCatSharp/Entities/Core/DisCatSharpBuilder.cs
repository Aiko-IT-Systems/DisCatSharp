using System;
using System.Collections.Generic;

namespace DisCatSharp.Entities.Core;

/// <summary>
///     Represents the common base for most builders.
/// </summary>
public class DisCatSharpBuilder
{
	/// <summary>
	///     The attachments of this builder.
	/// </summary>
	internal List<DiscordAttachment> AttachmentsInternal { get; } = [];

	/// <summary>
	///     The components of this builder.
	/// </summary>
	internal List<DiscordComponent> ComponentsInternal { get; } = [];

	/// <summary>
	///     The embeds of this builder.
	/// </summary>
	internal List<DiscordEmbed> EmbedsInternal { get; } = [];

	/// <summary>
	///     The files of this builder.
	/// </summary>
	internal List<DiscordMessageFile> FilesInternal { get; } = [];

	/// <summary>
	///     The content of this builder.
	/// </summary>
	internal string? ContentInternal { get; set; }

	/// <summary>
	///     The components of this builder.
	/// </summary>
	public IReadOnlyList<DiscordComponent> Components => this.ComponentsInternal;

	/// <summary>
	///     The content of this builder.
	/// </summary>
	public string? Content
	{
		get => this.ContentInternal;
		set
		{
			if (value is { Length: > 2000 })
				throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));

			this.ContentInternal = value;
		}
	}

	/// <summary>
	///     The embeds for this builder.
	/// </summary>
	public IReadOnlyList<DiscordEmbed> Embeds => this.EmbedsInternal;

	/// <summary>
	///     The attachments of this builder.
	/// </summary>
	public IReadOnlyList<DiscordAttachment> Attachments => this.AttachmentsInternal;

	/// <summary>
	///     The files of this builder.
	/// </summary>
	public IReadOnlyList<DiscordMessageFile> Files => this.FilesInternal;

	/// <summary>
	///     Clears the components on this builder.
	/// </summary>
	public void ClearComponents()
		=> this.ComponentsInternal.Clear();

	/// <summary>
	///     Allows for clearing the builder so that it can be used again.
	/// </summary>
	public virtual void Clear()
	{
		this.Content = null;
		this.FilesInternal.Clear();
		this.EmbedsInternal.Clear();
		this.AttachmentsInternal.Clear();
		this.ComponentsInternal.Clear();
	}

	/// <summary>
	///     Validates the builder.
	/// </summary>
	internal virtual void Validate()
	{ }
}
