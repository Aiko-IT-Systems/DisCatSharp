using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DisCatSharp.Entities;

/// <summary>
/// Constructs an interaction response.
/// </summary>
public sealed class DiscordInteractionResponseBuilder
{
	/// <summary>
	/// Whether this interaction response is text-to-speech.
	/// </summary>
	public bool IsTts { get; set; }

	/// <summary>
	/// Whether this interaction response should be ephemeral.
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
	/// Content of the message to send.
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
	/// Embeds to send on this interaction response.
	/// </summary>
	public IReadOnlyList<DiscordEmbed> Embeds => this._embeds;

	private readonly List<DiscordEmbed> _embeds = new();

	/// <summary>
	/// Files to send on this interaction response.
	/// </summary>
	public IReadOnlyList<DiscordMessageFile> Files => this._files;

	private readonly List<DiscordMessageFile> _files = new();

	/// <summary>
	/// Components to send on this interaction response.
	/// </summary>
	public IReadOnlyList<DiscordActionRowComponent> Components => this._components;

	private readonly List<DiscordActionRowComponent> _components = new();

	/// <summary>
	/// The choices to send on this interaction response.
	/// Mutually exclusive with content, embed, and components.
	/// </summary>
	public IReadOnlyList<DiscordApplicationCommandAutocompleteChoice> Choices => this._choices;

	private readonly List<DiscordApplicationCommandAutocompleteChoice> _choices = new();

	/// <summary>
	/// Mentions to send on this interaction response.
	/// </summary>
	public IReadOnlyList<IMention> Mentions => this._mentions;

	private readonly List<IMention> _mentions = new();

	/// <summary>
	/// The hints to send on this interaction response.
	/// </summary>
	public IReadOnlyList<DiscordInteractionCallbackHint> CallbackHints => this._callbackHints;

	private readonly List<DiscordInteractionCallbackHint> _callbackHints = new();

	/// <summary>
	/// Constructs a new empty interaction response builder.
	/// </summary>
	public DiscordInteractionResponseBuilder() { }

	/// <summary>
	/// Constructs a new <see cref="DiscordInteractionResponseBuilder"/> based on an existing <see cref="DisCatSharp.Entities.DiscordMessageBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder to copy.</param>
	public DiscordInteractionResponseBuilder(DiscordMessageBuilder builder)
	{
		this._content = builder.Content;
		this._mentions = builder.Mentions;
		this._embeds.AddRange(builder.Embeds);
		this._components.AddRange(builder.Components);
	}

	/// <summary>
	/// Provides the interaction response with <see cref="DiscordInteractionCallbackHint"/>s.
	/// </summary>
	/// <param name="hintBuilder">The hint builder.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="hintBuilder"/> is <see langword="null"/>.</exception>
	public DiscordInteractionResponseBuilder WithCallbackHints(DiscordCallbackHintBuilder hintBuilder)
	{
		if (hintBuilder == null)
			throw new ArgumentNullException(nameof(hintBuilder), "Callback hint builder cannot be null.");

		if (!hintBuilder.CallbackHints.Any())
			return this;

		this._callbackHints.Clear();
		this._callbackHints.AddRange(hintBuilder.CallbackHints);
		return this;
	}

	/// <summary>
	/// Appends a collection of components to the builder. Each call will append to a new row.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionResponseBuilder AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	/// Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionResponseBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		var ara = components.ToArray();

		if (ara.Length + this._components.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this._components.Add(ar);

		return this;
	}

	/// <summary>
	/// Appends a collection of components to the builder. Each call will append to a new row.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionResponseBuilder AddComponents(IEnumerable<DiscordComponent> components)
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
	/// Indicates if the interaction response will be text-to-speech.
	/// </summary>
	/// <param name="tts">Text-to-speech</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder WithTts(bool tts)
	{
		this.IsTts = tts;
		return this;
	}

	/// <summary>
	/// Sets the interaction response to be ephemeral.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AsEphemeral()
	{
		this.FlagsChanged = true;
		this.IsEphemeral = true;
		return this;
	}

	/// <summary>
	/// Sets the interaction response to suppress embeds.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder SuppressEmbeds()
	{
		this.FlagsChanged = true;
		this.EmbedsSuppressed = true;
		return this;
	}

	/// <summary>
	/// Sets the interaction response to be send as silent message.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AsSilentMessage()
	{
		this.FlagsChanged = true;
		this.NotificationsSuppressed = true;
		return this;
	}

	/// <summary>
	/// Sets the content of the message to send.
	/// </summary>
	/// <param name="content">Content to send.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder WithContent(string content)
	{
		this.Content = content;
		return this;
	}

	/// <summary>
	/// Adds an embed to send with the interaction response.
	/// </summary>
	/// <param name="embed">Embed to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddEmbed(DiscordEmbed embed)
	{
		if (embed != null)
			this._embeds.Add(embed); // Interactions will 400 silently //
		return this;
	}

	/// <summary>
	/// Adds the given embeds to send with the interaction response.
	/// </summary>
	/// <param name="embeds">Embeds to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
	{
		this._embeds.AddRange(embeds);
		return this;
	}

	/// <summary>
	/// Adds a file to the interaction response.
	/// </summary>
	/// <param name="filename">Name of the file.</param>
	/// <param name="data">File data.</param>
	/// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddFile(string filename, Stream data, bool resetStreamPosition = false, string description = null)
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
	public DiscordInteractionResponseBuilder AddFile(FileStream stream, bool resetStreamPosition = false, string description = null)
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
	/// Adds the given files to the interaction response builder.
	/// </summary>
	/// <param name="files">Dictionary of file name and file data.</param>
	/// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddFiles(Dictionary<string, Stream> files, bool resetStreamPosition = false)
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
	/// Adds the mention to the mentions to parse, etc. with the interaction response.
	/// </summary>
	/// <param name="mention">Mention to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddMention(IMention mention)
	{
		this._mentions.Add(mention);
		return this;
	}

	/// <summary>
	/// Adds the mentions to the mentions to parse, etc. with the interaction response.
	/// </summary>
	/// <param name="mentions">Mentions to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddMentions(IEnumerable<IMention> mentions)
	{
		this._mentions.AddRange(mentions);
		return this;
	}

	/// <summary>
	/// Adds a single auto-complete choice to the builder.
	/// </summary>
	/// <param name="choice">The choice to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddAutoCompleteChoice(DiscordApplicationCommandAutocompleteChoice choice)
	{
		this._choices.Add(choice);
		return this;
	}

	/// <summary>
	/// Adds auto-complete choices to the builder.
	/// </summary>
	/// <param name="choices">The choices to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddAutoCompleteChoices(IEnumerable<DiscordApplicationCommandAutocompleteChoice> choices)
	{
		this._choices.AddRange(choices);
		return this;
	}

	/// <summary>
	/// Adds auto-complete choices to the builder.
	/// </summary>
	/// <param name="choices">The choices to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddAutoCompleteChoices(params DiscordApplicationCommandAutocompleteChoice[] choices)
		=> this.AddAutoCompleteChoices((IEnumerable<DiscordApplicationCommandAutocompleteChoice>)choices);

	/// <summary>
	/// Clears all message components on this builder.
	/// </summary>
	public void ClearComponents()
		=> this._components.Clear();

	/// <summary>
	/// Allows for clearing the Interaction Response Builder so that it can be used again to send a new response.
	/// </summary>
	public void Clear()
	{
		this.Content = "";
		this._embeds.Clear();
		this.IsTts = false;
		this.IsEphemeral = false;
		this._mentions.Clear();
		this._components.Clear();
		this._choices.Clear();
		this._files.Clear();
	}
}
