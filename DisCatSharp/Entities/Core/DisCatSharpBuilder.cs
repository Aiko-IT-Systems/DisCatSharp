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
	///     Whether flags were changed.
	/// </summary>
	internal bool FlagsChanged { get; set; } = false;

	/// <summary>
	///     The components of this builder.
	/// </summary>
	public IReadOnlyList<DiscordComponent> Components
		=> this.ComponentsInternal;

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
	public IReadOnlyList<DiscordEmbed> Embeds
		=> this.EmbedsInternal;

	/// <summary>
	///     The attachments of this builder.
	/// </summary>
	public IReadOnlyList<DiscordAttachment> Attachments
		=> this.AttachmentsInternal;

	/// <summary>
	///     The files of this builder.
	/// </summary>
	public IReadOnlyList<DiscordMessageFile> Files
		=> this.FilesInternal;

	/// <summary>
	///     Gets the Allowed Mentions for the message to be sent.
	/// </summary>
	public List<IMention>? Mentions { get; internal set; }

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

	/// <summary>
	///     Whether to send as voice message.
	/// </summary>
	private bool VOICE_MSG { get; set; }

	/// <summary>
	///     Whether to send as silent message.
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
	///     Whether to send as silent message.
	/// </summary>
	private bool NOTIFICATIONS_SUPPRESSED { get; set; }

	/// <summary>
	///     Whether this message should be using UI Kit.
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
	///     Whether this is using UI Kit.
	/// </summary>
	private bool UI_KIT { get; set; }

	/// <summary>
	///     Whether to suppress embeds.
	/// </summary>
	public bool EmbedsSuppressed
	{
		get => this.EMBEDS_SUPPRESSED;
		set
		{
			this.EMBEDS_SUPPRESSED = value;
			this.FlagsChanged = true;
		}
	}

	/// <summary>
	///     Whether embeds are suppressed.
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
		this.Mentions = null;
	}

	/// <summary>
	///     Validates the builder.
	/// </summary>
	internal virtual void Validate()
	{ }
}
