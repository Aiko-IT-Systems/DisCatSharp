using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DisCatSharp.Entities.Core;

namespace DisCatSharp.Entities;

/// <summary>
///     Constructs an interaction response.
/// </summary>
public sealed class DiscordInteractionResponseBuilder : DisCatSharpBuilder
{
	/// <summary>
	///     Constructs a new empty interaction response builder.
	/// </summary>
	public DiscordInteractionResponseBuilder()
	{ }

	/// <summary>
	///     Constructs a new <see cref="DiscordInteractionResponseBuilder" /> based on an existing
	///     <see cref="DisCatSharp.Entities.DiscordMessageBuilder" />.
	/// </summary>
	/// <param name="builder">The builder to copy.</param>
	public DiscordInteractionResponseBuilder(DisCatSharpBuilder builder)
	{
		this.Content = builder.Content;
		this.MentionsInternal ??= builder.MentionsInternal;
		this.EmbedsInternal ??= builder.EmbedsInternal;
		this.ComponentsInternal ??= builder.ComponentsInternal;
		this.EmbedsSuppressed = builder.EmbedsSuppressed;
		this.IsComponentsV2 = builder.IsComponentsV2;
		this.FilesInternal ??= builder.FilesInternal;
		this.AttachmentsInternal ??= builder.AttachmentsInternal;
	}

	/// <summary>
	///     Gets the choices.
	/// </summary>
	internal List<DiscordApplicationCommandAutocompleteChoice>? ChoicesInternal { get; set; } = null;

	/// <summary>
	///     Whether this interaction response is text-to-speech.
	/// </summary>
	public bool IsTts { get; set; }

	/// <summary>
	///     Whether this interaction response should be ephemeral.
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

	/// <summary>
	///     Gets or sets a value indicating whether the followup message is ephemeral.
	/// </summary>
	/// <remarks>
	///     An ephemeral message is only visible to the user who triggered the interaction.
	/// </remarks>
	private bool EPH { get; set; }

	/// <summary>
	///     The choices to sent on this interaction response.
	///     Mutually exclusive with content, embed, and components.
	/// </summary>
	public IReadOnlyList<DiscordApplicationCommandAutocompleteChoice>? Choices
		=> this.ChoicesInternal;

	/// <summary>
	///     Gets the poll for this message.
	/// </summary>
	public DiscordPollBuilder? Poll { get; private set; }

	/// <summary>
	///     <para>
	///         Adds a row of components to the builder, up to <c>5</c> components per row, and up to <c>5</c> rows per
	///         message.
	///     </para>
	///     <para>
	///         If <see cref="WithV2Components" /> was called, the limit changes to <c>10</c> top-level components and max
	///         <c>30</c> components in total.
	///     </para>
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The builder to chain calls with.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	public DiscordInteractionResponseBuilder AddComponents(params DiscordActionRowComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordActionRowComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionResponseBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		this.ComponentsInternal ??= [];
		var ara = components.ToArray();

		if (ara.Length + this.ComponentsInternal.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this.ComponentsInternal.Add(ar);

		return this;
	}

	/// <summary>
	///     Appends a collection of components to the builder. Each call will append to a new row.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionResponseBuilder AddComponents(IEnumerable<DiscordComponent> components)
	{
		this.ComponentsInternal ??= [];
		var cmpArr = components.ToArray();
		var count = cmpArr.Length;

		if (this.IsComponentsV2)
		{
			switch (count)
			{
				case 0:
					throw new ArgumentOutOfRangeException(nameof(components), "You must provide at least one component");
				case > 10:
					throw new ArgumentException("Cannot add more than 10 components!");
			}

			this.ComponentsInternal.AddRange(cmpArr);
		}
		else
		{
			switch (count)
			{
				case 0:
					throw new ArgumentOutOfRangeException(nameof(components), "You must provide at least one component");
				case > 5:
					throw new ArgumentException("Cannot add more than 5 components per action row!");
			}

			var comp = new DiscordActionRowComponent(cmpArr);
			this.ComponentsInternal.Add(comp);
		}

		return this;
	}

	/// <summary>
	///     Adds a poll to this builder.
	/// </summary>
	/// <param name="pollBuilder">The poll builder to add.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordInteractionResponseBuilder WithPoll(DiscordPollBuilder pollBuilder)
	{
		this.Poll = pollBuilder;
		return this;
	}

	/// <summary>
	///     Indicates if the interaction response will be text-to-speech.
	/// </summary>
	/// <param name="tts">Text-to-speech</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder WithTts(bool tts)
	{
		this.IsTts = tts;
		return this;
	}

	/// <summary>
	///     Sets the interaction response to be ephemeral.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AsEphemeral()
	{
		this.IsEphemeral = true;
		return this;
	}

	/// <summary>
	///     Sets that this builder should be using UI Kit.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder WithV2Components()
	{
		this.IsComponentsV2 = true;
		return this;
	}

	/// <summary>
	///     Sets the interaction response to suppress embeds.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder SuppressEmbeds()
	{
		this.EmbedsSuppressed = true;
		return this;
	}

	/// <summary>
	///     Sets the interaction response to be sent as silent message.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AsSilentMessage()
	{
		this.NotificationsSuppressed = true;
		return this;
	}

	/// <summary>
	///     Sets the followup message to be sent as voice message.
	/// </summary>
	internal DiscordInteractionResponseBuilder AsVoiceMessage(bool asVoiceMessage = true)
	{
		this.IsVoiceMessage = asVoiceMessage;
		return this;
	}

	/// <summary>
	///     Sets the content of the message to send.
	/// </summary>
	/// <param name="content">Content to send.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder WithContent(string content)
	{
		this.Content = content;
		return this;
	}

	/// <summary>
	///     Adds an embed to sent with the interaction response.
	/// </summary>
	/// <param name="embed">Embed to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddEmbed(DiscordEmbed embed)
	{
		ArgumentNullException.ThrowIfNull(embed, nameof(embed));
		this.EmbedsInternal ??= [];
		this.EmbedsInternal.Add(embed);
		return this;
	}

	/// <summary>
	///     Adds the given embeds to sent with the interaction response.
	/// </summary>
	/// <param name="embeds">Embeds to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
	{
		this.EmbedsInternal ??= [];
		this.EmbedsInternal.AddRange(embeds);
		return this;
	}

	/// <summary>
	///     Adds a file to the interaction response.
	/// </summary>
	/// <param name="filename">Name of the file.</param>
	/// <param name="data">File data.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddFile(string filename, Stream data, bool resetStreamPosition = false, string? description = null)
	{
		this.FilesInternal ??= [];
		if (this.FilesInternal.Count >= 10)
			throw new ArgumentException("Cannot sent more than 10 files with a single message.");

		if (this.FilesInternal.Any(x => x.Filename == filename))
			throw new ArgumentException("A File with that filename already exists");

		if (resetStreamPosition)
			this.FilesInternal.Add(new(filename, data, data.Position, description: description));
		else
			this.FilesInternal.Add(new(filename, data, null, description: description));

		return this;
	}

	/// <summary>
	///     Sets if the message has files to be sent.
	/// </summary>
	/// <param name="stream">The Stream to the file.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddFile(FileStream stream, bool resetStreamPosition = false, string? description = null)
	{
		this.FilesInternal ??= [];
		if (this.FilesInternal.Count >= 10)
			throw new ArgumentException("Cannot sent more than 10 files with a single message.");

		if (this.FilesInternal.Any(x => x.Filename == stream.Name))
			throw new ArgumentException("A File with that filename already exists");

		if (resetStreamPosition)
			this.FilesInternal.Add(new(stream.Name, stream, stream.Position, description: description));
		else
			this.FilesInternal.Add(new(stream.Name, stream, null, description: description));

		return this;
	}

	/// <summary>
	///     Adds the given files to the interaction response builder.
	/// </summary>
	/// <param name="files">Dictionary of file name and file data.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddFiles(Dictionary<string, Stream> files, bool resetStreamPosition = false)
	{
		this.FilesInternal ??= [];
		if (this.FilesInternal.Count + files.Count > 10)
			throw new ArgumentException("Cannot sent more than 10 files with a single message.");

		foreach (var file in files)
		{
			if (this.FilesInternal.Any(x => x.Filename == file.Key))
				throw new ArgumentException("A File with that filename already exists");

			if (resetStreamPosition)
				this.FilesInternal.Add(new(file.Key, file.Value, file.Value.Position));
			else
				this.FilesInternal.Add(new(file.Key, file.Value, null));
		}

		return this;
	}

	/// <summary>
	///     Adds the mention to the mentions to parse, etc. with the interaction response.
	/// </summary>
	/// <param name="mention">Mention to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder WithAllowedMention(IMention mention)
	{
		this.MentionsInternal ??= [];
		this.MentionsInternal.Add(mention);
		return this;
	}

	/// <summary>
	///     Adds the mentions to the mentions to parse, etc. with the interaction response.
	/// </summary>
	/// <param name="mentions">Mentions to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder WithAllowedMentions(IEnumerable<IMention> mentions)
	{
		this.MentionsInternal ??= [];
		this.MentionsInternal.AddRange(mentions);
		return this;
	}

	/// <summary>
	///     Adds a single auto-complete choice to the builder.
	/// </summary>
	/// <param name="choice">The choice to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddAutoCompleteChoice(DiscordApplicationCommandAutocompleteChoice choice)
	{
		this.ChoicesInternal ??= [];
		this.ChoicesInternal.Add(choice);
		return this;
	}

	/// <summary>
	///     Adds auto-complete choices to the builder.
	/// </summary>
	/// <param name="choices">The choices to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddAutoCompleteChoices(IEnumerable<DiscordApplicationCommandAutocompleteChoice> choices)
	{
		this.ChoicesInternal ??= [];
		this.ChoicesInternal.AddRange(choices);
		return this;
	}

	/// <summary>
	///     Adds auto-complete choices to the builder.
	/// </summary>
	/// <param name="choices">The choices to add.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionResponseBuilder AddAutoCompleteChoices(params DiscordApplicationCommandAutocompleteChoice[] choices)
		=> this.AddAutoCompleteChoices((IEnumerable<DiscordApplicationCommandAutocompleteChoice>)choices);

	/// <summary>
	///     Clears the poll from this builder.
	/// </summary>
	public void ClearPoll()
		=> this.Poll = null;

	/// <inheritdoc />
	public override void Clear()
	{
		this.IsEphemeral = false;
		this.ChoicesInternal?.Clear();
		this.ChoicesInternal = null;
		this.Poll = null;
		base.Clear();
	}
}
