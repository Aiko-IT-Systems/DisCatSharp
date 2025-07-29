using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities.Core;
using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Constructs ready-to-send webhook requests.
/// </summary>
public sealed class DiscordWebhookBuilder : DisCatSharpBuilder
{
	/// <summary>
	///     Gets the applied tags.
	/// </summary>
	private List<ulong>? _appliedTags = null;

	/// <summary>
	///     Whether to keep previous attachments.
	/// </summary>
	internal bool? KeepAttachmentsInternal;

	/// <summary>
	///     Constructs a new empty webhook request builder.
	/// </summary>
	public DiscordWebhookBuilder()
	{ }

	/// <summary>
	///     Username to use for this webhook request.
	/// </summary>
	public Optional<string> Username { get; set; }

	/// <summary>
	///     Avatar url to use for this webhook request.
	/// </summary>
	public Optional<string> AvatarUrl { get; set; }

	/// <summary>
	///     Whether this webhook request is text-to-speech.
	/// </summary>
	public bool IsTts { get; private set; }

	/// <summary>
	///     Name of the new thread.
	///     Only works if the webhook is sent in a <see cref="ChannelType.Forum" />.
	/// </summary>
	public string? ThreadName { get; set; }

	/// <summary>
	///     Forum post tags to send on this webhook request.
	/// </summary>
	public IReadOnlyList<ulong>? AppliedTags => this._appliedTags;

	/// <summary>
	///     Gets the poll for this message.
	/// </summary>
	public DiscordPollBuilder? Poll { get; private set; }

	/// <summary>
	///     Whether to send components with this webhook requests.
	///     Set to <see langword="true" /> if you want to send components with non-application-owned webhooks.
	/// </summary>
	public bool? WithComponents { get; internal set; }

	/// <summary>
	///     Sets the webhook response to suppress embeds.
	/// </summary>
	public DiscordWebhookBuilder SuppressEmbeds()
	{
		this.EmbedsSuppressed = true;
		return this;
	}

	/// <summary>
	///     Sets the webhook to be sent as silent message.
	/// </summary>
	public DiscordWebhookBuilder AsSilentMessage()
	{
		this.NotificationsSuppressed = true;
		return this;
	}

	/// <summary>
	///     Sets if you want to send components with non-application-owned webhooks.
	/// </summary>
	public DiscordWebhookBuilder SendWithComponents()
	{
		this.WithComponents = true;
		return this;
	}

	/// <summary>
	///     Sets the webhook to be sent as voice message.
	/// </summary>
	public DiscordWebhookBuilder AsVoiceMessage()
	{
		this.IsVoiceMessage = true;
		return this;
	}

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
	/// <param name="components">The components to add to the builder.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordWebhookBuilder AddComponents(params DiscordComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordComponent>)components);

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	public DiscordWebhookBuilder AddComponents(params DiscordActionRowComponent[] components)
		=> this.AddComponents((IEnumerable<DiscordActionRowComponent>)components);

	/// <summary>
	///     Appends several rows of components to the builder
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The builder to chain calls with.</returns>
	public DiscordWebhookBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		this.HasComponents = true;
		this.ComponentsInternal ??= [];
		var ara = components.ToArray();

		if (ara.Length + this.ComponentsInternal.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this.ComponentsInternal.Add(ar);

		return this;
	}

	/// <summary>
	///     Adds a row of components to the builder, up to 5 components per row, and up to 5 rows per message.
	/// </summary>
	/// <param name="components">The components to add to the builder.</param>
	/// <returns>The current builder to be chained.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
	public DiscordWebhookBuilder AddComponents(IEnumerable<DiscordComponent> components)
	{
		this.HasComponents = true;
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
	///     Sets that this builder should be using UI Kit.
	/// </summary>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordWebhookBuilder WithV2Components()
	{
		this.FlagsChanged = true;
		this.IsComponentsV2 = true;
		return this;
	}

	/// <summary>
	///     Adds a poll to this webhook builder.
	/// </summary>
	/// <param name="pollBuilder">The poll builder to add.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordWebhookBuilder WithPoll(DiscordPollBuilder pollBuilder)
	{
		this.Poll = pollBuilder;
		return this;
	}

	/// <summary>
	///     Sets the username for this webhook builder.
	/// </summary>
	/// <param name="username">Username of the webhook</param>
	public DiscordWebhookBuilder WithUsername(string username)
	{
		this.Username = username;
		return this;
	}

	/// <summary>
	///     Sets the avatar of this webhook builder from its url.
	/// </summary>
	/// <param name="avatarUrl">Avatar url of the webhook</param>
	public DiscordWebhookBuilder WithAvatarUrl(string avatarUrl)
	{
		this.AvatarUrl = avatarUrl;
		return this;
	}

	/// <summary>
	///     Indicates if the webhook must use text-to-speech.
	/// </summary>
	/// <param name="tts">Text-to-speech</param>
	public DiscordWebhookBuilder WithTts(bool tts)
	{
		this.IsTts = tts;
		return this;
	}

	/// <summary>
	///     Sets the message to send at the execution of the webhook.
	/// </summary>
	/// <param name="content">Message to send.</param>
	public DiscordWebhookBuilder WithContent(string content)
	{
		this.Content = content;
		return this;
	}

	/// <summary>
	///     Sets the thread name to create at the execution of the webhook.
	///     Only works for <see cref="ChannelType.Forum" />.
	/// </summary>
	/// <param name="name">The thread name.</param>
	public DiscordWebhookBuilder WithThreadName(string name)
	{
		this.ThreadName = name;
		return this;
	}

	/// <summary>
	///     Adds an embed to send at the execution of the webhook.
	/// </summary>
	/// <param name="embed">Embed to add.</param>
	public DiscordWebhookBuilder AddEmbed(DiscordEmbed embed)
	{
		this.HasEmbeds = true;
		ArgumentNullException.ThrowIfNull(embed, nameof(embed));
		this.EmbedsInternal ??= [];
		this.EmbedsInternal.Add(embed);
		return this;
	}

	/// <summary>
	///     Adds the given embeds to send at the execution of the webhook.
	/// </summary>
	/// <param name="embeds">Embeds to add.</param>
	public DiscordWebhookBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds)
	{
		this.HasEmbeds = true;
		this.EmbedsInternal ??= [];
		this.EmbedsInternal.AddRange(embeds);
		return this;
	}

	/// <summary>
	///     Adds a file to send at the execution of the webhook.
	/// </summary>
	/// <param name="filename">Name of the file.</param>
	/// <param name="data">File data.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	public DiscordWebhookBuilder AddFile(string filename, Stream data, bool resetStreamPosition = false, string description = null)
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
	///     Sets if the message has files to be sent.
	/// </summary>
	/// <param name="stream">The Stream to the file.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	/// <param name="description">Description of the file.</param>
	/// <returns></returns>
	public DiscordWebhookBuilder AddFile(FileStream stream, bool resetStreamPosition = false, string description = null)
	{
		this.FilesInternal ??= [];
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
	///     Adds the given files to send at the execution of the webhook.
	/// </summary>
	/// <param name="files">Dictionary of file name and file data.</param>
	/// <param name="resetStreamPosition">
	///     Tells the API Client to reset the stream position to what it was after the file is
	///     sent.
	/// </param>
	public DiscordWebhookBuilder AddFiles(Dictionary<string, Stream> files, bool resetStreamPosition = false)
	{
		this.FilesInternal ??= [];
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
	public DiscordWebhookBuilder ModifyAttachments(IEnumerable<DiscordAttachment> attachments)
	{
		this.AttachmentsInternal ??= [];
		this.AttachmentsInternal.AddRange(attachments);
		return this;
	}

	/// <summary>
	///     Whether to keep the message attachments, if new ones are added.
	/// </summary>
	/// <returns></returns>
	public DiscordWebhookBuilder KeepAttachments(bool keep)
	{
		this.KeepAttachmentsInternal = keep;
		return this;
	}

	/// <summary>
	///     Adds the mention to the mentions to parse, etc. at the execution of the webhook.
	/// </summary>
	/// <param name="mention">Mention to add.</param>
	public DiscordWebhookBuilder WithAllowedMention(IMention mention)
	{
		this.MentionsInternal ??= [];
		this.MentionsInternal.Add(mention);
		return this;
	}

	/// <summary>
	///     Adds the mentions to the mentions to parse, etc. at the execution of the webhook.
	/// </summary>
	/// <param name="mentions">Mentions to add.</param>
	public DiscordWebhookBuilder WithAllowedMentions(IEnumerable<IMention> mentions)
	{
		this.MentionsInternal ??= [];
		this.MentionsInternal.AddRange(mentions);
		return this;
	}

	/// <summary>
	///     Tags to apply to forum posts.
	/// </summary>
	/// <param name="tags">Tags to add.</param>
	public DiscordWebhookBuilder WithAppliedTags(IEnumerable<ulong> tags)
	{
		this._appliedTags ??= [];
		this._appliedTags.AddRange(tags);
		return this;
	}

	/// <summary>
	///     Executes a webhook.
	/// </summary>
	/// <param name="webhook">The webhook that should be executed.</param>
	/// <returns>The message sent</returns>
	public async Task<DiscordMessage> SendAsync(DiscordWebhook webhook)
		=> await webhook.ExecuteAsync(this).ConfigureAwait(false);

	/// <summary>
	///     Executes a webhook.
	/// </summary>
	/// <param name="webhook">The webhook that should be executed.</param>
	/// <param name="threadId">Target thread id.</param>
	/// <returns>The message sent</returns>
	public async Task<DiscordMessage> SendAsync(DiscordWebhook webhook, ulong threadId)
		=> await webhook.ExecuteAsync(this, threadId.ToString()).ConfigureAwait(false);

	/// <summary>
	///     Sends the modified webhook message.
	/// </summary>
	/// <param name="webhook">The webhook that should be executed.</param>
	/// <param name="message">The message to modify.</param>
	/// <returns>The modified message</returns>
	public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, DiscordMessage message)
		=> await this.ModifyAsync(webhook, message.Id).ConfigureAwait(false);

	/// <summary>
	///     Sends the modified webhook message.
	/// </summary>
	/// <param name="webhook">The webhook that should be executed.</param>
	/// <param name="messageId">The id of the message to modify.</param>
	/// <returns>The modified message</returns>
	public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, ulong messageId)
		=> await webhook.EditMessageAsync(messageId, this).ConfigureAwait(false);

	/// <summary>
	///     Sends the modified webhook message.
	/// </summary>
	/// <param name="webhook">The webhook that should be executed.</param>
	/// <param name="message">The message to modify.</param>
	/// <param name="thread">Target thread.</param>
	/// <returns>The modified message</returns>
	public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, DiscordMessage message, DiscordThreadChannel thread)
		=> await this.ModifyAsync(webhook, message.Id, thread.Id).ConfigureAwait(false);

	/// <summary>
	///     Sends the modified webhook message.
	/// </summary>
	/// <param name="webhook">The webhook that should be executed.</param>
	/// <param name="messageId">The id of the message to modify.</param>
	/// <param name="threadId">Target thread id.</param>
	/// <returns>The modified message</returns>
	public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, ulong messageId, ulong threadId)
		=> await webhook.EditMessageAsync(messageId, this, threadId.ToString()).ConfigureAwait(false);

	/// <summary>
	///     Clears the poll from this builder.
	/// </summary>
	public void ClearPoll()
		=> this.Poll = null;

	/// <inheritdoc />
	public override void ClearComponents()
		=> base.ClearComponents();

	/// <summary>
	/// Removes existing components from the webhook message.
	/// </summary>
	public void RemoveComponents()
		=> this.ComponentsInternal = [];

	/// <summary>
	/// Removes existing embeds from the webhook message.
	/// </summary>
	public void RemoveEmbeds()
		=> this.EmbedsInternal = [];

	/// <summary>
	/// Removes existing attachments from the webhook message.
	/// </summary>
	public void RemoveAttachments()
		=> this.AttachmentsInternal = [];

	/// <inheritdoc />
	public override void Clear()
	{
		this.IsTts = false;
		this.KeepAttachmentsInternal = false;
		this.ThreadName = null;
		this.Poll = null;
		base.Clear();
	}

	/// <summary>
	///     Does the validation before we send the Create/Modify request.
	/// </summary>
	/// <param name="isModify">Tells the method to perform the Modify Validation or Create Validation.</param>
	/// <param name="isFollowup">Tells the method to perform the follow-up message validation.</param>
	/// <param name="isInteractionResponse">Tells the method to perform the interaction response validation.</param>
	internal void Validate(bool isModify = false, bool isFollowup = false, bool isInteractionResponse = false)
	{
		if (isModify)
		{
			if (this.Username.HasValue)
				throw new ArgumentException("You cannot change the username of a message.");

			if (this.AvatarUrl.HasValue)
				throw new ArgumentException("You cannot change the avatar of a message.");

			if (this.Poll is not null)
				throw new InvalidOperationException("You cannnot edit a poll.");
		}
		else if (isFollowup)
		{
			if (this.Username.HasValue)
				throw new ArgumentException("You cannot change the username of a follow up message.");

			if (this.AvatarUrl.HasValue)
				throw new ArgumentException("You cannot change the avatar of a follow up message.");

			if (this.Poll is not null)
				throw new InvalidOperationException("You cannnot edit a poll.");
		}
		else if (isInteractionResponse)
		{
			if (this.Username.HasValue)
				throw new ArgumentException("You cannot change the username of an interaction response.");

			if (this.AvatarUrl.HasValue)
				throw new ArgumentException("You cannot change the avatar of an interaction response.");
		}

		if (this.Files?.Count is 0 or null && string.IsNullOrEmpty(this.Content) && this.Embeds?.Count is 0 or null && this.Components?.Count is 0 or null && this.Poll is null && this.Attachments?.Count is 0 or null)
			throw new ArgumentException("You must specify content, an embed, a component, a poll, or at least one file.");

		this.Poll?.Validate();

		base.Validate();
	}
}
