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
	///     Whether flags were changed.
	/// </summary>
	internal bool FlagsChanged = false;

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
	///     Whether to suppress embeds.
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
	///     Whether to send as voice message.
	///     You can't use that on your own, it needs DisCatSharp.Experimental.
	/// </summary>
	internal bool IsVoiceMessage
	{
		get => this.VOICE_MSG;
		set
		{
			this.VOICE_MSG = value;
			this.FlagsChanged = true;
		}
	}

	private bool VOICE_MSG { get; set; }

	/// <summary>
	///     Whether to send as silent message.
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
	///     Mentions to send on this followup message.
	/// </summary>
	public List<IMention>? Mentions { get; private set; }

	/// <summary>
	///     Gets the poll for this message.
	/// </summary>
	public DiscordPollBuilder? Poll { get; private set; }

	/// <summary>
	///     Appends a collection of components to the message.
	/// </summary>
	/// <param name="components">The collection of components to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	/// <exception cref="ArgumentException"><paramref name="components" /> contained more than 5 components.</exception>
	public DiscordFollowupMessageBuilder AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns></returns>
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
		var compArr = components.ToArray();
		var count = compArr.Length;

		if (count > 5)
			throw new ArgumentException("Cannot add more than 5 components per action row!");

		var arc = new DiscordActionRowComponent(compArr);
		this.ComponentsInternal.Add(arc);
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
	public DiscordFollowupMessageBuilder AddMention(IMention mention)
	{
		if (this.Mentions != null)
			this.Mentions.Add(mention);
		else
			this.Mentions = [mention];
		return this;
	}

	/// <summary>
	///     Adds the mentions to the mentions to parse, etc. with the followup message.
	/// </summary>
	/// <param name="mentions">Mentions to add.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordFollowupMessageBuilder AddMentions(IEnumerable<IMention> mentions)
	{
		if (this.Mentions != null)
			this.Mentions.AddRange(mentions);
		else
			this.Mentions = [.. mentions];
		return this;
	}

	/// <summary>
	///     Sets the followup message to be ephemeral.
	/// </summary>
	public DiscordFollowupMessageBuilder AsEphemeral()
	{
		this.FlagsChanged = true;
		this.IsEphemeral = true;
		return this;
	}

	/// <summary>
	///     Sets the followup message to suppress embeds.
	/// </summary>
	public DiscordFollowupMessageBuilder SuppressEmbeds()
	{
		this.FlagsChanged = true;
		this.EmbedsSuppressed = true;
		return this;
	}

	/// <summary>
	///     Sets the followup message to be send as voice message.
	/// </summary>
	internal DiscordFollowupMessageBuilder AsVoiceMessage(bool asVoiceMessage = true)
	{
		this.FlagsChanged = true;
		this.IsVoiceMessage = asVoiceMessage;
		return this;
	}

	/// <summary>
	///     Sets the followup message to be send as silent message.
	/// </summary>
	public DiscordFollowupMessageBuilder AsSilentMessage()
	{
		this.FlagsChanged = true;
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
		this.Mentions = null;
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
