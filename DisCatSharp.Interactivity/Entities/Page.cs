using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity.Entities;

/// <summary>
///     The page.
/// </summary>
public class Page
{
	/// <summary>
	///		Gets a value indicating whether the CV2 feature is used.
	/// </summary>
	internal bool UsesCV2 { get; private set;} = false;

	public Page()
	{ }

	/// <summary>
	///     <para>Initializes a new instance of the <see cref="Page" /> class.</para>
	///     <para>You need to specify at least <paramref name="content" /> or <paramref name="embed" /> or both.</para>
	/// </summary>
	/// <param name="content">The content.</param>
	/// <param name="embed">The embed.</param>
	[Deprecated("This constructor is deprecated, use empty Page instead.")]
	public Page(string? content = null, DiscordEmbedBuilder? embed = null)
	{
		if (string.IsNullOrEmpty(content) && embed is null)
			throw new ArgumentException("You need to specify at least content or embed or both.");

		this.Content = content;
		this.Embed = embed?.Build();
	}

	/// <summary>
	///     Gets or sets the content.
	/// </summary>
	public string? Content { get; set; }

	/// <summary>
	///		Sets the content of the page.
	///		<note type="warning">This cannot be used together with <see cref="AddComponents(DiscordComponent[])"/> / <see cref="AddComponents(IEnumerable{DiscordComponent})"/>.</note>
	/// </summary>
	/// <param name="content">The new content to assign to the page.</param>
	/// <returns>The current <see cref="Page"/> instance with updated content.</returns>
	public Page WithContent(string content)
	{
		if (this.UsesCV2)
			throw new InvalidOperationException("You cannot use content and components on the same page.");
		this.Content = content;
		return this;
	}

	/// <summary>
	///     Gets or sets the embed.
	/// </summary>
	public DiscordEmbed? Embed { get; set; }

	/// <summary>
	///		Sets the embed of the page.
	///		<note type="warning">This cannot be used together with <see cref="AddComponents(DiscordComponent[])"/> / <see cref="AddComponents(IEnumerable{DiscordComponent})"/>.</note>
	/// </summary>
	/// <param name="embedBuilder">The builder used to construct the embed to add to the page.</param>
	/// <returns>The current <see cref="Page"/> instance with the embed applied.</returns>
	public Page WithEmbed(DiscordEmbedBuilder embedBuilder)
	{
		if (this.UsesCV2)
			throw new InvalidOperationException("You cannot use embeds and components on the same page.");
		this.Embed = embedBuilder.Build();
		return this;
	}

	/// <summary>
	///     The files of this page.
	/// </summary>
	internal List<DiscordMessageFile>? FilesInternal { get; set; } = null;

	/// <summary>
	///     The components of this page.
	/// </summary>
	internal List<DiscordComponent>? ComponentsInternal { get; set; } = null;

	/// <summary>
	///     Adds a file to the page.
	/// </summary>
	/// <param name="filename">Name of the file.</param>
	/// <param name="data">File data.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	public Page AddFile(string filename, Stream data, bool resetStreamPosition = false, string description = null)
	{
		this.FilesInternal ??= [];
		if (this.FilesInternal.Count > 10)
			throw new ArgumentException("Cannot send more than 10 files with a single message.");

		if (this.FilesInternal.Any(x => x.Filename == filename))
			throw new ArgumentException("A File with that filename already exists");

		if (resetStreamPosition)
			this.FilesInternal.Add(new(filename, data, data.Position, description: description));
		else
			this.FilesInternal.Add(new(filename, data, null, description: description));

		return this;
	}

	/// <summary>
	///     Adds v2 components to the message. This enables the use of up to <c>40</c> components per message.
	///     <note type="warning">This does not support old component behavior. Only V2.</note>
	/// </summary>
	/// <param name="components">The components to add to the page.</param>
	/// <returns>The current page to be chained.</returns>
	public Page AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	///     Adds v2 components to the message. This enables the use of up to <c>40</c> components per message.
	///     <note type="warning">This does not support old component behavior. Only V2.</note>
	/// </summary>
	/// <param name="components">The components to add to the page.</param>
	/// <returns>The current page to be chained.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	/// <returns>The current page to be chained.</returns>
	public Page AddComponents(IEnumerable<DiscordComponent> components)
	{
		this.UsesCV2 = true;
		this.Embed = null;
		this.Content = null;
		this.ComponentsInternal ??= [];
		var cmpArr = components.ToArray();
		var count = cmpArr.Length;

		switch (count)
		{
			case 0:
				throw new ArgumentOutOfRangeException(nameof(components), "You must provide at least one component");
			case > 40:
				throw new ArgumentException("Cannot add more than 40 components!");
		}

		this.ComponentsInternal.AddRange(cmpArr);

		return this;
	}
}
