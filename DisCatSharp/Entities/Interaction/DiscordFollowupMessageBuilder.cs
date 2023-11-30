using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DisCatSharp.Entities;

/// <summary>
/// Constructs a followup message to an interaction.
/// </summary>
public sealed class DiscordFollowupMessageBuilder
{
	/// <summary>
	/// Whether this followup message is text-to-speech.
	/// </summary>
	public bool IsTts { get; set; }

	/// <summary>
	/// Whether this followup message should be ephemeral.
	/// </summary>
	public bool IsEphemeral
	{
		get => this.EPH;
		set
		{
			this.EPH = value;
			this.FlagsChanged = true;
		}
	}

	private bool EPH { get; set; }

	/// <summary>
	/// Whether to suppress embeds.
	/// </summary>
	public bool EmbedsSuppressed
	{
		get => this.EMB_SUP;
		set
		{
			this.EMB_SUP = value;
			this.FlagsChanged = true;
		}
	}

	private bool EMB_SUP { get; set; }

	/// <summary>
	/// Whether to send as silent message.
	/// </summary>
	public bool NotificationsSuppressed
	{
		get => this.NOTI_SUP;
		set
		{
			this.NOTI_SUP = value;
			this.FlagsChanged = true;
		}
	}

	private bool NOTI_SUP { get; set; }

	/// <summary>
	/// Whether flags were changed.
	/// </summary>
	internal bool FlagsChanged = false;

	/// <summary>
	/// Message to send on followup message.
	/// </summary>
	public string Content
	{
		get => this._content;
		set
		{
			if (value != null && value.Length > 2000)
				throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));

			this._content = value;
		}
	}

	private string _content;

	/// <summary>
	/// Embeds to send on followup message.
	/// </summary>
	public IReadOnlyList<DiscordEmbed> Embeds => this._embeds;

	private readonly List<DiscordEmbed> _embeds = new();

	/// <summary>
	/// Files to send on this followup message.
	/// </summary>
	public IReadOnlyList<DiscordMessageFile> Files => this._files;

	private readonly List<DiscordMessageFile> _files = new();

	/// <summary>
	/// Components to send on this followup message.
	/// </summary>
	public IReadOnlyList<DiscordActionRowComponent> Components => this._components;

	private readonly List<DiscordActionRowComponent> _components = new();

	/// <summary>
	/// Mentions to send on this followup message.
	/// </summary>
	public IReadOnlyList<IMention> Mentions => this._mentions;

	private readonly List<IMention> _mentions = new();

	/// <summary>
	/// Appends a collection of components to the message.
	/// </summary>
	/// <param name="components">The collection of components to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	/// <exception cref="ArgumentException"><paramref name="components"/> contained more than 5 components.</exception>
	public DiscordFollowupMessageBuilder AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	/// Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns></returns>
	public DiscordFollowupMessageBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		var ara = components.ToArray();

		if (ara.Length + this._components.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this._components.Add(ar);

		return this;
	}

	/// <summary>
	/// Appends a collection of components to the message.
	/// </summary>
	/// <param name="components">The collection of components to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	/// <exception cref="ArgumentException"><paramref name="components"/> contained more than 5 components.</exception>
	public DiscordFollowupMessageBuilder AddComponents(IEnumerable<DiscordComponent> components)
	{
		var compArr = components.ToArray();
		var count = compArr.Length;

		if (count > 5)
			throw new ArgumentException("Cannot add more than 5 components per action row!");

		var arc = new DiscordActionRowComponent(compArr);
		this._components.Add(arc);
		return this;
	}

	/// <summary>
	/// Indicates if the followup message must use text-to-speech.
	/// </summary>
	/// <param name="tts">Text-to-speech</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder WithTts(bool tts)
	{
		this.IsTts = tts;
		return this;
	}

	/// <summary>
	/// Sets the message to send with the followup message..
	/// </summary>
	/// <param name="content">Message to send.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder WithContent(string content)
	{
		this.Content = content;
		return this;
	}

	/// <summary>
	/// Adds an embed to the followup message.
	/// </summary>
	/// <param name="embed">Embed to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddEmbed(DiscordEmbed embed)
	{
		this._embeds.Add(embed);
		return this;
	}

	/// <summary>
	/// Adds the given embeds to the followup message.
	/// </summary>
	/// <param name="embeds">Embeds to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
	{
		this._embeds.AddRange(embeds);
		return this;
	}

	/// <summary>
	/// Adds a file to the followup message.
	/// </summary>
	/// <param name="filename">Name of the file.</param>
	/// <param name="data">File data.</param>
	/// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddFile(string filename, Stream data, bool resetStreamPosition = false, string description = null)
	{
		if (this.Files.Count >= 10)
			throw new ArgumentException("Cannot send more than 10 files with a single message.");

		if (this._files.Any(x => x.Filename == filename))
			throw new ArgumentException("A File with that filename already exists");

		if (resetStreamPosition)
			this._files.Add(new(filename, data, data.Position, description: description));
		else
			this._files.Add(new(filename, data, null, description: description));

		return this;
	}

	/// <summary>
	/// Sets if the message has files to be sent.
	/// </summary>
	/// <param name="stream">The Stream to the file.</param>
	/// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddFile(FileStream stream, bool resetStreamPosition = false, string description = null)
	{
		if (this.Files.Count >= 10)
			throw new ArgumentException("Cannot send more than 10 files with a single message.");

		if (this._files.Any(x => x.Filename == stream.Name))
			throw new ArgumentException("A File with that filename already exists");

		if (resetStreamPosition)
			this._files.Add(new(stream.Name, stream, stream.Position, description: description));
		else
			this._files.Add(new(stream.Name, stream, null, description: description));

		return this;
	}

	/// <summary>
	/// Adds the given files to the followup message.
	/// </summary>
	/// <param name="files">Dictionary of file name and file data.</param>
	/// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddFiles(Dictionary<string, Stream> files, bool resetStreamPosition = false)
	{
		if (this.Files.Count + files.Count > 10)
			throw new ArgumentException("Cannot send more than 10 files with a single message.");

		foreach (var file in files)
		{
			if (this._files.Any(x => x.Filename == file.Key))
				throw new ArgumentException("A File with that filename already exists");

			if (resetStreamPosition)
				this._files.Add(new(file.Key, file.Value, file.Value.Position));
			else
				this._files.Add(new(file.Key, file.Value, null));
		}

		return this;
	}

	/// <summary>
	/// Adds the mention to the mentions to parse, etc. with the followup message.
	/// </summary>
	/// <param name="mention">Mention to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddMention(IMention mention)
	{
		this._mentions.Add(mention);
		return this;
	}

	/// <summary>
	/// Adds the mentions to the mentions to parse, etc. with the followup message.
	/// </summary>
	/// <param name="mentions">Mentions to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddMentions(IEnumerable<IMention> mentions)
	{
		this._mentions.AddRange(mentions);
		return this;
	}

	/// <summary>
	/// Sets the followup message to be ephemeral.
	/// </summary>
	public DiscordFollowupMessageBuilder AsEphemeral()
	{
		this.FlagsChanged = true;
		this.IsEphemeral = true;
		return this;
	}

	/// <summary>
	/// Sets the followup message to suppress embeds.
	/// </summary>
	public DiscordFollowupMessageBuilder SuppressEmbeds()
	{
		this.FlagsChanged = true;
		this.EmbedsSuppressed = true;
		return this;
	}

	/// <summary>
	/// Sets the followup message to be send as silent message.
	/// </summary>
	public DiscordFollowupMessageBuilder AsSilentMessage()
	{
		this.FlagsChanged = true;
		this.NotificationsSuppressed = true;
		return this;
	}

	/// <summary>
	/// Clears all message components on this builder.
	/// </summary>
	public void ClearComponents()
		=> this._components.Clear();

	/// <summary>
	/// Allows for clearing the Followup Message builder so that it can be used again to send a new message.
	/// </summary>
	public void Clear()
	{
		this.Content = "";
		this._embeds.Clear();
		this.IsTts = false;
		this._mentions.Clear();
		this._files.Clear();
		this.IsEphemeral = false;
		this._components.Clear();
	}

	/// <summary>
	/// Validates the builder.
	/// </summary>
	internal void Validate()
	{
		if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && !this.Embeds.Any() && !this.Components.Any())
			throw new ArgumentException("You must specify content, an embed, a component, or at least one file.");
	}
}
