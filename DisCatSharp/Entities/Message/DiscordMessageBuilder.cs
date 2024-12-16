using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities.Core;

namespace DisCatSharp.Entities;

/// <summary>
///     Constructs a message to be sent.
/// </summary>
public sealed class DiscordMessageBuilder : DisCatSharpBuilder
{
	/// <summary>
	///     Whether to keep previous attachments.
	/// </summary>
	internal bool? KeepAttachmentsInternal;

	/// <summary>
	///     Gets the Sticker to be send.
	/// </summary>
	public DiscordSticker Sticker { get; set; }

	/// <summary>
	///     Gets or Sets if the message should be TTS.
	/// </summary>
	public bool IsTts { get; private set; }

	/// <summary>
	///     Gets or Sets if the message should be send silent.
	/// </summary>
	public bool Silent { get; private set; }

	/// <summary>
	///     Whether to send as voice message.
	///     You can't use that on your own, it needs DisCatSharp.Experimental.
	/// </summary>
	public bool IsVoiceMessage { get; private set; }

	/// <summary>
	///     Gets the Reply Message ID.
	/// </summary>
	public ulong? ReplyId { get; private set; }

	/// <summary>
	///     Gets if the Reply should mention the user.
	/// </summary>
	public bool MentionOnReply { get; private set; }

	/// <summary>
	///     Gets if the Reply will error if the Reply Message Id does not reference a valid message.
	///     <para>If set to false, invalid replies are send as a regular message.</para>
	///     <para>Defaults to false.</para>
	/// </summary>
	public bool FailOnInvalidReply { get; set; }

	/// <summary>
	///     Gets the nonce for the message.
	/// </summary>
	public string? Nonce { get; set; }

	/// <summary>
	///     Gets whether to enforce the nonce.
	/// </summary>
	public bool EnforceNonce { get; set; }

	/// <summary>
	///     Gets the poll for this message.
	/// </summary>
	public DiscordPollBuilder? Poll { get; private set; }

	/// <summary>
	///     Sets the nonce for the message.
	/// </summary>
	/// <param name="nonce">The nonce for the message. Max 25 chars.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithNonce(string nonce)
	{
		this.Nonce = nonce;
		return this;
	}

	/// <summary>
	///     Whether to enforce the nonce.
	/// </summary>
	/// <param name="enforceNonce">Controls the nonce enforcement.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithEnforceNonce(bool enforceNonce)
	{
		this.EnforceNonce = enforceNonce;
		return this;
	}

	/// <summary>
	///     Sets the Content of the Message.
	/// </summary>
	/// <param name="content">The content to be set.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithContent(string content)
	{
		this.Content = content;
		return this;
	}

	/// <summary>
	///     Adds a sticker to the message. Sticker must be from current guild.
	/// </summary>
	/// <param name="sticker">The sticker to add.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithSticker(DiscordSticker sticker)
	{
		this.Sticker = sticker;
		return this;
	}

	/// <summary>
	///     Adds a poll to the message.
	/// </summary>
	/// <param name="pollBuilder">The poll builder to add.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithPoll(DiscordPollBuilder pollBuilder)
	{
		this.Poll = pollBuilder;
		return this;
	}

	/// <summary>
	///     Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
	/// </summary>
	/// <param name="components">The components to add to the message.</param>
	/// <returns>The current builder to be chained.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	public DiscordMessageBuilder AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	public DiscordMessageBuilder AddComponents(params DiscordActionRowComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordActionRowComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns></returns>
	public DiscordMessageBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		var ara = components.ToArray();

		if (ara.Length + this.ComponentsInternal.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this.ComponentsInternal.Add(ar);

		return this;
	}

	/// <summary>
	///     Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
	/// </summary>
	/// <param name="components">The components to add to the message.</param>
	/// <returns>The current builder to be chained.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	public DiscordMessageBuilder AddComponents(IEnumerable<DiscordComponent> components)
	{
		var cmpArr = components.ToArray();
		var count = cmpArr.Length;

		if (this.IsUIKit)
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
	///     Sets if the message should be TTS.
	/// </summary>
	/// <param name="isTts">If TTS should be set.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder HasTts(bool isTts)
	{
		this.IsTts = isTts;
		return this;
	}

	/// <summary>
	///     Sets the message response to suppress embeds.
	/// </summary>
	public DiscordMessageBuilder SuppressEmbeds(bool suppress = true)
	{
		this.EmbedsSuppressed = suppress;
		return this;
	}

	/// <summary>
	/// Sets that this builder should be using UI Kit.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordMessageBuilder AsUIKitMessage()
	{
		this.IsUIKit = true;
		return this;
	}

	/// <summary>
	///     Sets the message to be send as voice message.
	/// </summary>
	public DiscordMessageBuilder AsVoiceMessage(bool asVoiceMessage = true)
	{
		this.IsVoiceMessage = asVoiceMessage;
		return this;
	}

	/// <summary>
	///     Sets the followup message to be send as silent message.
	/// </summary>
	public DiscordMessageBuilder AsSilentMessage(bool silent = true)
	{
		this.Silent = silent;
		return this;
	}

	/// <summary>
	///     Appends an embed to the current builder.
	/// </summary>
	/// <param name="embed">The embed that should be appended.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder AddEmbed(DiscordEmbed embed)
	{
		ArgumentNullException.ThrowIfNull(embed, nameof(embed));
		this.EmbedsInternal.Add(embed);
		return this;
	}

	/// <summary>
	///     Appends several embeds to the current builder.
	/// </summary>
	/// <param name="embeds">The embeds that should be appended.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
	{
		this.EmbedsInternal.AddRange(embeds);
		return this;
	}

	/// <summary>
	///     Sets if the message has allowed mentions.
	/// </summary>
	/// <param name="allowedMention">The allowed Mention that should be sent.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithAllowedMention(IMention allowedMention)
	{
		if (this.Mentions != null)
			this.Mentions.Add(allowedMention);
		else
			this.Mentions = [allowedMention];

		return this;
	}

	/// <summary>
	///     Sets if the message has allowed mentions.
	/// </summary>
	/// <param name="allowedMentions">The allowed Mentions that should be sent.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithAllowedMentions(IEnumerable<IMention> allowedMentions)
	{
		if (this.Mentions != null)
			this.Mentions.AddRange(allowedMentions);
		else
			this.Mentions = [.. allowedMentions];

		return this;
	}

	/// <summary>
	///     Sets if the message has files to be sent.
	/// </summary>
	/// <param name="fileName">The fileName that the file should be sent as.</param>
	/// <param name="stream">The Stream to the file.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithFile(string fileName, Stream stream, bool resetStreamPosition = false, string description = null)
	{
		if (this.FilesInternal.Count > 10)
			throw new ArgumentException("Cannot send more than 10 files with a single message.");

		if (this.FilesInternal.Any(x => x.Filename == fileName))
			throw new ArgumentException("A File with that filename already exists");

		if (resetStreamPosition)
			this.FilesInternal.Add(new(fileName, stream, stream.Position, description: description));
		else
			this.FilesInternal.Add(new(fileName, stream, null, description: description));

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
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithFile(FileStream stream, bool resetStreamPosition = false, string description = null)
	{
		if (this.FilesInternal.Count > 10)
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
	///     Sets if the message has files to be sent.
	/// </summary>
	/// <param name="files">The Files that should be sent.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithFiles(Dictionary<string, Stream> files, bool resetStreamPosition = false)
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
	///     Modifies the given attachments on edit.
	/// </summary>
	/// <param name="attachments">Attachments to edit.</param>
	/// <returns></returns>
	public DiscordMessageBuilder ModifyAttachments(IEnumerable<DiscordAttachment> attachments)
	{
		this.AttachmentsInternal.AddRange(attachments);
		return this;
	}

	/// <summary>
	///     Whether to keep the message attachments, if new ones are added.
	/// </summary>
	/// <returns></returns>
	public DiscordMessageBuilder KeepAttachments(bool keep)
	{
		this.KeepAttachmentsInternal = keep;
		return this;
	}

	/// <summary>
	///     Sets if the message is a reply
	/// </summary>
	/// <param name="messageId">The ID of the message to reply to.</param>
	/// <param name="mention">If we should mention the user in the reply.</param>
	/// <param name="failOnInvalidReply">Whether sending a reply that references an invalid message should be </param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordMessageBuilder WithReply(ulong messageId, bool mention = false, bool failOnInvalidReply = false)
	{
		this.ReplyId = messageId;
		this.MentionOnReply = mention;
		this.FailOnInvalidReply = failOnInvalidReply;

		if (mention)
		{
			this.Mentions ??= [];
			this.Mentions.Add(new RepliedUserMention());
		}

		return this;
	}

	/// <summary>
	///     Sends the Message to a specific channel
	/// </summary>
	/// <param name="channel">The channel the message should be sent to.</param>
	/// <returns>The current builder to be chained.</returns>
	public Task<DiscordMessage> SendAsync(DiscordChannel channel)
		=> channel.SendMessageAsync(this);

	/// <summary>
	///     Sends the modified message.
	///     <para>
	///         Note: Message replies cannot be modified. To clear the reply, simply pass <see langword="null" /> to
	///         <see cref="WithReply" />.
	///     </para>
	/// </summary>
	/// <param name="msg">The original Message to modify.</param>
	/// <returns>The current builder to be chained.</returns>
	public Task<DiscordMessage> ModifyAsync(DiscordMessage msg)
		=> msg.ModifyAsync(this);

	/// <summary>
	///     Clears the poll from this builder.
	/// </summary>
	public void ClearPoll()
		=> this.Poll = null;

	/// <summary>
	///     Allows for clearing the Message Builder so that it can be used again to send a new message.
	/// </summary>
	public override void Clear()
	{
		this.IsTts = false;
		this.ReplyId = null;
		this.MentionOnReply = false;
		this.Sticker = null!;
		this.KeepAttachmentsInternal = false;
		this.Nonce = null;
		this.EnforceNonce = false;
		this.Poll = null;
		base.Clear();
	}

	/// <summary>
	///     Does the validation before we send a the Create/Modify request.
	/// </summary>
	/// <param name="isModify">Tells the method to perform the Modify Validation or Create Validation.</param>
	internal void Validate(bool isModify = false)
	{
		if (this.EmbedsInternal.Count > 10)
			throw new ArgumentException("A message can only have up to 10 embeds.");

		if (isModify)
			this.AsVoiceMessage(false);

		if (!isModify)
		{
			if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && (!this.Embeds?.Any() ?? true) && this.Sticker is null && (!this.Components?.Any() ?? true) && this.Poll is null && this?.Attachments.Count == 0)
				throw new ArgumentException("You must specify content, an embed, a sticker, a component, a poll or at least one file.");

			if (this.IsUIKit && (!string.IsNullOrEmpty(this.Content) || (this.Embeds?.Any() ?? false)))
				throw new ArgumentException("Using UI Kit mode. You cannot specify content or embeds.");

			switch (this.IsUIKit)
			{
				case true when this.Components?.Count > 10:
					throw new InvalidOperationException("You can only have 10 components per message.");
				case false when this.Components?.Count > 5:
					throw new InvalidOperationException("You can only have 5 action rows per message.");
			}

			if (this.EnforceNonce && string.IsNullOrEmpty(this.Nonce))
				throw new InvalidOperationException("Nonce enforcement is enabled, but no nonce is set.");

			this.Poll?.Validate();
		}
		else if (this.Poll is not null)
			throw new InvalidOperationException("Messages with polls can't be edited.");

		base.Validate();
	}
}
