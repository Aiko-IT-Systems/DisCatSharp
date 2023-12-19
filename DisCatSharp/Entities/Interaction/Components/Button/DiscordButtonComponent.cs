using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a button that can be pressed. Fires <see cref="DisCatSharp.DiscordClient.ComponentInteractionCreated"/> event when pressed.
/// </summary>
public sealed class DiscordButtonComponent : DiscordComponent
{
	/// <summary>
	/// The style of the button.
	/// </summary>
	[JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
	public ButtonStyle Style { get; internal set; }

	/// <summary>
	/// The text to apply to the button. If this is not specified <see cref="Emoji"/> becomes required.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	/// <summary>
	/// Whether this button can be pressed.
	/// </summary>
	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; internal set; }

	/// <summary>
	/// The emoji to add to the button. Can be used in conjunction with a label, or as standalone. Must be added if label is not specified.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordComponentEmoji Emoji { get; internal set; }

	/// <summary>
	/// Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordButtonComponent Enable()
		=> this.SetState(false);

	/// <summary>
	/// Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordButtonComponent Disable()
		=> this.SetState(true);

	/// <summary>
	/// Enables or disables this component.
	/// </summary>
	/// <param name="disabled">Whether this component should be disabled.</param>
	/// <returns>The current component.</returns>
	public DiscordButtonComponent SetState(bool disabled)
	{
		this.Disabled = disabled;
		return this;
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordButtonComponent"/>.
	/// </summary>
	internal DiscordButtonComponent()
	{
		this.Type = ComponentType.Button;
	}

	/// <summary>
	/// Constructs a new button based on another button.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordButtonComponent(DiscordButtonComponent other)
		: this()
	{
		this.CustomId = other.CustomId;
		this.Style = other.Style;
		this.Label = other.Label;
		this.Disabled = other.Disabled;
		this.Emoji = other.Emoji;
	}

	/// <summary>
	/// Constructs a new button with the specified options.
	/// </summary>
	/// <param name="style">The style/color of the button.</param>
	/// <param name="customId">The Id to assign to the button. This is sent back when a user presses it.</param>
	/// <param name="label">The text to display on the button, up to 80 characters. Can be left blank if <paramref name="emoji"/>is set.</param>
	/// <param name="disabled">Whether this button should be initialized as being disabled. User sees a greyed out button that cannot be interacted with.</param>
	/// <param name="emoji">The emoji to add to the button. This is required if <paramref name="label"/> is empty or null.</param>
	/// <exception cref="ArgumentException">Is thrown when neither the <paramref name="emoji"/> nor the <paramref name="label"/> is set.</exception>
	public DiscordButtonComponent(ButtonStyle style, string customId = null, string label = null, bool disabled = false, DiscordComponentEmoji emoji = null)
	{
		this.Style = style;
		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.Disabled = disabled;
		if (emoji != null)
		{
			this.Label = label;
			this.Emoji = emoji;
		}
		else
		{
			this.Label = label ?? throw new ArgumentException("Label can only be null if emoji is set.");
			this.Emoji = null;
		}

		this.Type = ComponentType.Button;
	}
}
