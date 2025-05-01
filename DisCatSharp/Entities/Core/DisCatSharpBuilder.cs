using System;
using System.Collections.Generic;
using System.Linq;

namespace DisCatSharp.Entities.Core;

/// <summary>
///     Represents the common base for most builders.
/// </summary>
public class DisCatSharpBuilder
{
	/// <summary>
	///     The attachments of this builder.
	/// </summary>
	internal List<DiscordAttachment>? AttachmentsInternal { get; set; } = null;

	/// <summary>
	///     The components of this builder.
	/// </summary>
	internal List<DiscordComponent>? ComponentsInternal { get; set; } = null;

	/// <summary>
	///     The embeds of this builder.
	/// </summary>
	internal List<DiscordEmbed>? EmbedsInternal { get; set; } = null;

	/// <summary>
	///     The files of this builder.
	/// </summary>
	internal List<DiscordMessageFile>? FilesInternal { get; set; } = null;

	/// <summary>
	///     The allowed mentions of this builder.
	/// </summary>
	internal List<IMention>? MentionsInternal { get; set; } = null;

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
			if (this.IsComponentsV2 || this.IsVoiceMessage)
				throw new InvalidOperationException("You cannot set the content for UI Kit / Voice messages");

			if (value is { Length: > 2000 })
				throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));

			this.ContentInternal = value;
		}
	}

	/// <summary>
	///     Gets the components of this builder.
	/// </summary>
	public IReadOnlyList<DiscordComponent>? Components
		=> this.ComponentsInternal;

	/// <summary>
	///     Gets the embeds of this builder.
	/// </summary>
	public IReadOnlyList<DiscordEmbed>? Embeds
		=> this.EmbedsInternal;

	/// <summary>
	///     Gets the attachments of this builder.
	/// </summary>
	public IReadOnlyList<DiscordAttachment>? Attachments
		=> this.AttachmentsInternal;

	/// <summary>
	///     Gets the files of this builder.
	/// </summary>
	public IReadOnlyList<DiscordMessageFile>? Files
		=> this.FilesInternal;

	/// <summary>
	///     Gets the allowed mentions of this builder.
	/// </summary>
	public IReadOnlyList<IMention>? Mentions
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
	public bool IsComponentsV2
	{
		get => this.IS_COMPONENTS_V2;
		set
		{
			this.IS_COMPONENTS_V2 = value;
			this.FlagsChanged = true;
		}
	}

	/// <summary>
	///     Whether this builder is using UI Kit.
	/// </summary>
	private bool IS_COMPONENTS_V2 { get; set; }

	/// <summary>
	///     Sets whether this builder suppresses its embeds.
	/// </summary>
	public bool EmbedsSuppressed
	{
		get => this.EMBEDS_SUPPRESSED;
		set
		{
			if (this.IsComponentsV2)
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
		=> this.ComponentsInternal?.Clear();

	/// <summary>
	///     Allows for clearing the builder so that it can be used again.
	/// </summary>
	public virtual void Clear()
	{
		this.Content = null;
		this.FilesInternal?.Clear();
		this.FilesInternal = null;
		this.EmbedsInternal?.Clear();
		this.EmbedsInternal = null;
		this.AttachmentsInternal?.Clear();
		this.AttachmentsInternal = null;
		this.ComponentsInternal?.Clear();
		this.ComponentsInternal = null;
		this.IsVoiceMessage = false;
		this.IsComponentsV2 = false;
		this.EmbedsSuppressed = false;
		this.NotificationsSuppressed = false;
		this.FlagsChanged = false;
		this.MentionsInternal?.Clear();
		this.MentionsInternal = null;
	}

	/// <summary>
	///     Validates the builder.
	/// </summary>
	internal virtual void Validate()
	{
		if (this.Components?.Count > 0)
		{
			HashSet<int> ids = [];
			Dictionary<int, List<string>> duplicateIds = [];

			foreach (var component in this.Components)
				this.CheckComponentIds(component, ids, duplicateIds);

			if (duplicateIds.Count > 0)
			{
				var duplicateDetails = string.Join(", ", duplicateIds.Select(kvp => $"ID: {kvp.Key}, Types: {string.Join(", ", kvp.Value)}"));
				throw new AggregateException($"You provided one or more components with the same id. They have to be unique. Duplicates: {duplicateDetails}");
			}
		}
	}

	/// <summary>
	///     Validates the uniqueness of component IDs within a given <see cref="DiscordComponent" /> hierarchy.
	/// </summary>
	/// <param name="component">The root component to validate.</param>
	/// <param name="ids">A set of IDs already encountered, used to track uniqueness.</param>
	/// <param name="duplicateIds">
	///     A dictionary to store duplicate IDs and their associated component types,
	///     for reporting purposes if duplicates are found.
	/// </param>
	/// <exception cref="AggregateException">
	///     Thrown when duplicate component IDs are detected, providing details about the duplicates.
	/// </exception>
	private void CheckComponentIds(DiscordComponent component, HashSet<int> ids, Dictionary<int, List<string>> duplicateIds)
	{
		if (component is DiscordActionRowComponent actionRowComponent)
			foreach (var actionRowComponentChild in actionRowComponent.Components)
				this.AddId(actionRowComponentChild, ids, duplicateIds);
		else if (component is DiscordContainerComponent containerComponent)
			foreach (var containerComponentChild in containerComponent.Components)
			{
				if (containerComponentChild is DiscordActionRowComponent actionRowContainerComponentChild)
					foreach (var actionRowComponentChild in actionRowContainerComponentChild.Components)
						this.AddId(actionRowComponentChild, ids, duplicateIds);
				else if (containerComponentChild is DiscordSectionComponent subSectionComponent)
				{
					foreach (var sectionComponentChild in subSectionComponent.Components)
						this.AddId(sectionComponentChild, ids, duplicateIds);
					this.AddId(subSectionComponent.Accessory, ids, duplicateIds);
				}

				this.AddId(containerComponentChild, ids, duplicateIds);
			}
		else if (component is DiscordSectionComponent sectionComponent)
		{
			foreach (var sectionComponentChild in sectionComponent.Components)
				this.AddId(sectionComponentChild, ids, duplicateIds);
			this.AddId(sectionComponent.Accessory, ids, duplicateIds);
		}

		this.AddId(component, ids, duplicateIds);
	}

	private void AddId(DiscordComponent component, HashSet<int> ids, Dictionary<int, List<string>> duplicateIds)
	{
		if (component.Id.HasValue)
		{
			var id = component.Id.Value;
			if (!ids.Add(id))
			{
				if (!duplicateIds.ContainsKey(id))
					duplicateIds[id] = [];
				duplicateIds[id].Add(component.Type.ToString());
			}
		}
	}
}
