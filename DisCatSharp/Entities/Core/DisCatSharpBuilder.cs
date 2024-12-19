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
	///     The allowed mentions of this builder.
	/// </summary>
	internal List<IMention> MentionsInternal { get; } = [];

	/// <summary>
	///     The content of this builder.
	/// </summary>
	internal string? ContentInternal { get; set; }

	/// <summary>
	///     Whether flags were changed in this builder.
	/// </summary>
	internal bool FlagsChanged { get; set; } = false;

	/// <summary>
	///     Sets the content of this builder.
	/// </summary>
	public string? Content
	{
		get => this.ContentInternal;
		set
		{
			if (this.IsUIKit || this.IsVoiceMessage)
				throw new InvalidOperationException("You cannot set the content for UI Kit / Voice messages");

			if (value is { Length: > 2000 })
				throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));

			this.ContentInternal = value;
		}
	}

	/// <summary>
	///     Gets the components of this builder.
	/// </summary>
	public IReadOnlyList<DiscordComponent> Components
		=> this.ComponentsInternal;

	/// <summary>
	///     Gets the embeds of this builder.
	/// </summary>
	public IReadOnlyList<DiscordEmbed> Embeds
		=> this.EmbedsInternal;

	/// <summary>
	///     Gets the attachments of this builder.
	/// </summary>
	public IReadOnlyList<DiscordAttachment> Attachments
		=> this.AttachmentsInternal;

	/// <summary>
	///     Gets the files of this builder.
	/// </summary>
	public IReadOnlyList<DiscordMessageFile> Files
		=> this.FilesInternal;

	/// <summary>
	///     Gets the allowed mentions of this builder.
	/// </summary>
	public IReadOnlyList<IMention> Mentions
		=> this.MentionsInternal;

	/// <summary>
	///     Sets whether this builder sends a voice message.
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

	/// <summary>
	///     Whether this builder sends a voice message.
	/// </summary>
	private bool VOICE_MSG { get; set; }

	/// <summary>
	///     Sets whether this builder should send a silent message.
	/// </summary>
	public bool NotificationsSuppressed
	{
		get => this.NOTIFICATIONS_SUPPRESSED;
		set
		{
			this.NOTIFICATIONS_SUPPRESSED = value;
			this.FlagsChanged = true;
		}
	}

	/// <summary>
	///     Whether this builder sends a silent message.
	/// </summary>
	private bool NOTIFICATIONS_SUPPRESSED { get; set; }

	/// <summary>
	///     Sets whether this builder should be using UI Kit.
	/// </summary>
	public bool IsUIKit
	{
		get => this.UI_KIT;
		set
		{
			this.UI_KIT = value;
			this.FlagsChanged = true;
		}
	}

	/// <summary>
	///     Whether this builder is using UI Kit.
	/// </summary>
	private bool UI_KIT { get; set; }

	/// <summary>
	///     Sets whether this builder suppresses its embeds.
	/// </summary>
	public bool EmbedsSuppressed
	{
		get => this.EMBEDS_SUPPRESSED;
		set
		{
			if (this.IsUIKit)
				throw new InvalidOperationException("You cannot set embeds suppressed for UI Kit messages since they cannot have embeds");

			this.EMBEDS_SUPPRESSED = value;
			this.FlagsChanged = true;
		}
	}

	/// <summary>
	///     Whether this builder has its embeds suppressed.
	/// </summary>
	private bool EMBEDS_SUPPRESSED { get; set; }

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
		this.IsVoiceMessage = false;
		this.IsUIKit = false;
		this.EmbedsSuppressed = false;
		this.NotificationsSuppressed = false;
		this.FlagsChanged = false;
		this.MentionsInternal.Clear();
	}

	/// <summary>
	///     Validates the builder.
	/// </summary>
	internal virtual void Validate()
	{ }
}
