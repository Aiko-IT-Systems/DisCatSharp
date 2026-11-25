using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an action within a <see cref="DiscordOverflowMenuComponent" />.
/// </summary>
public sealed class DiscordMenuActionItemComponent : DiscordComponent
{
	/// <summary>
	///     Initializes a new instance for payload deserialization.
	/// </summary>
	internal DiscordMenuActionItemComponent()
	{
		this.Type = ComponentType.MenuActionItem;
	}

	/// <summary>
	///     Constructs an overflow menu action item.
	/// </summary>
	/// <param name="style">Whether selection invokes the app, is destructive, or opens a link.</param>
	/// <param name="label">The visible label, up to 80 characters.</param>
	/// <param name="customId">The interaction identifier required for <see cref="MenuActionItemStyle.Default"/> and <see cref="MenuActionItemStyle.Danger"/>.</param>
	/// <param name="url">The destination required for <see cref="MenuActionItemStyle.Link"/>.</param>
	/// <param name="description">Optional supporting text, up to 100 characters.</param>
	/// <param name="emoji">Optional item emoji.</param>
	/// <param name="disabled">Whether the item is disabled.</param>
	public DiscordMenuActionItemComponent(MenuActionItemStyle style, string label, string? customId = null, string? url = null, string? description = null, DiscordComponentEmoji? emoji = null, bool disabled = false)
		: this()
	{
		if (string.IsNullOrEmpty(label) || label.Length > 80)
			throw new ArgumentException("The menu item label must contain between 1 and 80 characters.", nameof(label));
		if (description?.Length > 100)
			throw new ArgumentException("The menu item description cannot exceed 100 characters.", nameof(description));

		this.ValidateAction(style, customId, url);
		this.Style = style;
		this.Label = label;
		this.CustomId = customId;
		this.Url = url;
		this.Description = description;
		this.Emoji = emoji;
		this.Disabled = disabled;
	}

	/// <summary>
	///     Gets the interaction identifier for interactive styles.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public override string? CustomId { get; internal set; }

	/// <summary>
	///     Gets the label displayed for this item.
	/// </summary>
	[JsonProperty("label")]
	public string Label { get; internal set; } = string.Empty;

	/// <summary>
	///     Gets the action style.
	/// </summary>
	[JsonProperty("style")]
	public MenuActionItemStyle Style { get; internal set; }

	/// <summary>
	///     Gets optional supporting text displayed below the label.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	///     Gets the destination opened by a link item.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string? Url { get; internal set; }

	/// <summary>
	///     Gets the optional item emoji.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordComponentEmoji? Emoji { get; internal set; }

	/// <summary>
	///     Gets whether this item is disabled.
	/// </summary>
	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; internal set; }

	/// <summary>
	///     Assigns a component identifier.
	/// </summary>
	/// <param name="id">The identifier assigned by the application.</param>
	/// <returns>This item.</returns>
	public DiscordMenuActionItemComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}

	/// <summary>
	///     Sets whether this menu item is disabled.
	/// </summary>
	/// <param name="disabled">Whether the item should be unavailable for selection.</param>
	/// <returns>This menu item.</returns>
	public DiscordMenuActionItemComponent SetState(bool disabled)
	{
		this.Disabled = disabled;
		return this;
	}

	/// <summary>
	///     Disables this menu item.
	/// </summary>
	/// <returns>This menu item.</returns>
	public DiscordMenuActionItemComponent Disable()
		=> this.SetState(true);

	/// <summary>
	///     Enables this menu item.
	/// </summary>
	/// <returns>This menu item.</returns>
	public DiscordMenuActionItemComponent Enable()
		=> this.SetState(false);

	/// <summary>
	///     Validates that action-specific fields match this item's style.
	/// </summary>
	/// <param name="style">The action style.</param>
	/// <param name="customId">The interaction identifier.</param>
	/// <param name="url">The link destination.</param>
	/// <exception cref="ArgumentException">The style or its required fields are invalid.</exception>
	private void ValidateAction(MenuActionItemStyle style, string? customId, string? url)
	{
		switch (style)
		{
			case MenuActionItemStyle.Default or MenuActionItemStyle.Danger:
				if (string.IsNullOrEmpty(customId) || customId.Length > 100)
					throw new ArgumentException("Interactive menu items require a custom ID of 1 to 100 characters.", nameof(customId));
				if (url is not null)
					throw new ArgumentException("Interactive menu items cannot have a URL.", nameof(url));
				break;
			case MenuActionItemStyle.Link:
				if (string.IsNullOrEmpty(url))
					throw new ArgumentException("Link menu items require a URL.", nameof(url));
				if (customId is not null)
					throw new ArgumentException("Link menu items cannot have a custom ID.", nameof(customId));
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(style), style, "Unknown menu action item style.");
		}
	}
}
