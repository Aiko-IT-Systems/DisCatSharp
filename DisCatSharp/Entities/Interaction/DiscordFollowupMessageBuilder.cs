using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DisCatSharp.Entities.Core;

namespace DisCatSharp.Entities;

/// <summary>
///     Constructs a followup message to an interaction.
/// </summary>
public sealed class DiscordFollowupMessageBuilder : DisCatSharpBuilder
{
	/// <summary>
	///     Whether this followup message is text-to-speech.
	/// </summary>
	public bool IsTts { get; set; }

	/// <summary>
	///     Whether this followup message should be ephemeral.
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
	///     Gets the poll for this message.
	/// </summary>
	public DiscordPollBuilder? Poll { get; private set; }

	/// <summary>
	///     Sets that this builder should be using UI Kit.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder WithV2Components()
	{
		this.IsComponentsV2 = true;
		return this;
	}

	/// <summary>
	///     <para>Adds a row of components to the builder, up to <c>5</c> components per row, and up to <c>5</c> rows per message.</para>
	///     <para>If <see cref="WithV2Components"/> was called, the limit changes to <c>10</c> top-level components and max <c>30</c> components in total.</para>
	/// </summary>
	/// <param name="components">The collection of components to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The builder to chain calls with.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	public DiscordFollowupMessageBuilder AddComponents(params DiscordActionRowComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordActionRowComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		var ara = components.ToArray();

		if (ara.Length + this.ComponentsInternal.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this.ComponentsInternal.Add(ar);

		return this;
	}

	/// <summary>
	///     Appends a collection of components to the message.
	/// </summary>
	/// <param name="components">The collection of components to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	/// <exception cref="ArgumentException"><paramref name="components" /> contained more than 5 components.</exception>
	public DiscordFollowupMessageBuilder AddComponents(IEnumerable<DiscordComponent> components)
	{
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
	public DiscordFollowupMessageBuilder WithPoll(DiscordPollBuilder pollBuilder)
	{
		this.Poll = pollBuilder;
		return this;
	}

	/// <summary>
	///     Indicates if the followup message must use text-to-speech.
	/// </summary>
	/// <param name="tts">Text-to-speech</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder WithTts(bool tts)
	{
		this.IsTts = tts;
		return this;
	}

	/// <summary>
	///     Sets the message to send with the followup message.
	/// </summary>
	/// <param name="content">Message to send.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder WithContent(string content)
	{
		this.Content = content;
		return this;
	}

	/// <summary>
	///     Adds an embed to the followup message.
	/// </summary>
	/// <param name="embed">Embed to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddEmbed(DiscordEmbed embed)
	{
		ArgumentNullException.ThrowIfNull(embed, nameof(embed));
		this.EmbedsInternal.Add(embed);
		return this;
	}

	/// <summary>
	///     Adds the given embeds to the followup message.
	/// </summary>
	/// <param name="embeds">Embeds to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
	{
		this.EmbedsInternal.AddRange(embeds);
		return this;
	}

	/// <summary>
	///     Adds a file to the followup message.
	/// </summary>
	/// <param name="filename">Name of the file.</param>
	/// <param name="data">File data.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddFile(string filename, Stream data, bool resetStreamPosition = false, string description = null)
	{
		if (this.FilesInternal.Count >= 10)
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
	///     Sets if the message has files to be sent.
	/// </summary>
	/// <param name="stream">The Stream to the file.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddFile(FileStream stream, bool resetStreamPosition = false, string description = null)
	{
		if (this.FilesInternal.Count >= 10)
			throw new ArgumentException("Cannot send more than 10 files with a single message.");

		if (this.FilesInternal.Any(x => x.Filename == stream.Name))
			throw new ArgumentException("A File with that filename already exists");

		if (resetStreamPosition)
			this.FilesInternal.Add(new(stream.Name, stream, stream.Position, description: description));
		else
			this.FilesInternal.Add(new(stream.Name, stream, null, description: description));

		return this;
	}

	/// <summary>
	///     Adds the given files to the followup message.
	/// </summary>
	/// <param name="files">Dictionary of file name and file data.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddFiles(Dictionary<string, Stream> files, bool resetStreamPosition = false)
	{
		if (this.FilesInternal.Count + files.Count > 10)
			throw new ArgumentException("Cannot send more than 10 files with a single message.");

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
	///     Adds the mention to the mentions to parse, etc. with the followup message.
	/// </summary>
	/// <param name="mention">Mention to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder WithAllowedMention(IMention mention)
	{
		this.MentionsInternal.Add(mention);
		return this;
	}

	/// <summary>
	///     Adds the mentions to the mentions to parse, etc. with the followup message.
	/// </summary>
	/// <param name="mentions">Mentions to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder WithAllowedMentions(IEnumerable<IMention> mentions)
	{
		this.MentionsInternal.AddRange(mentions);
		return this;
	}

	/// <summary>
	///     Sets the followup message to be ephemeral.
	/// </summary>
	public DiscordFollowupMessageBuilder AsEphemeral()
	{
		this.IsEphemeral = true;
		return this;
	}

	/// <summary>
	///     Sets the followup message to suppress embeds.
	/// </summary>
	public DiscordFollowupMessageBuilder SuppressEmbeds()
	{
		this.EmbedsSuppressed = true;
		return this;
	}

	/// <summary>
	///     Sets the followup message to be send as voice message.
	/// </summary>
	internal DiscordFollowupMessageBuilder AsVoiceMessage(bool asVoiceMessage = true)
	{
		this.IsVoiceMessage = asVoiceMessage;
		return this;
	}

	/// <summary>
	///     Sets the followup message to be send as silent message.
	/// </summary>
	public DiscordFollowupMessageBuilder AsSilentMessage()
	{
		this.NotificationsSuppressed = true;
		return this;
	}

	/// <summary>
	///     Clears the poll from this builder.
	/// </summary>
	public void ClearPoll()
		=> this.Poll = null;

	/// <inheritdoc />
	public override void Clear()
	{
		this.IsTts = false;
		this.IsEphemeral = false;
		base.Clear();
	}

	/// <inheritdoc />
	internal override void Validate()
	{
		if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && this.EmbedsInternal.Count == 0 && this.ComponentsInternal.Count == 0 && this.Poll is null && this.AttachmentsInternal.Count == 0)
			throw new ArgumentException("You must specify content, an embed, a component, a poll, or at least one file.");

		this.Poll?.Validate();
		base.Validate();
	}
}
