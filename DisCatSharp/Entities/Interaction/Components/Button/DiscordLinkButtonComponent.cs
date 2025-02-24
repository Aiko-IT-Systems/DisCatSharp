using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a link button. Clicking a link button does not send an interaction.
/// </summary>
public class DiscordLinkButtonComponent : DiscordBaseButtonComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordLinkButtonComponent" />. This type of button does not send back and interaction
	///     when pressed.
	/// </summary>
	/// <param name="url">The url to set the button to.</param>
	/// <param name="label">The text to display on the button. Can be left blank if <paramref name="emoji" /> is set.</param>
	/// <param name="disabled">Whether or not this button can be pressed.</param>
	/// <param name="emoji">The emoji to set with this button. This is required if <paramref name="label" /> is null or empty.</param>
	public DiscordLinkButtonComponent(string url, string label, bool disabled = false, DiscordComponentEmoji emoji = null)
		: this()
	{
		this.Url = url;
		this.Label = label;
		this.Disabled = disabled;
		this.Emoji = emoji;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordLinkButtonComponent" /> class.
	/// </summary>
	public DiscordLinkButtonComponent()
	{
		this.Type = ComponentType.Button;
	}

	/// <summary>
	///     The url to open when pressing this button.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string Url { get; set; }

	/// <summary>
	///     The text to add to this button. If this is not specified, <see cref="Emoji" /> must be.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; set; }

	/// <summary>
	///     Whether this button can be pressed.
	/// </summary>
	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; set; }

	/// <summary>
	///     The emoji to add to the button. Can be used in conjunction with a label, or as standalone. Must be added if label
	///     is not specified.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordComponentEmoji Emoji { get; set; }

	/// <summary>
	///     Gets the style.
	/// </summary>
	[JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
	internal ButtonStyle Style { get; set; } = ButtonStyle.Link;

	/// <summary>
	///     Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordLinkButtonComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	///     Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordLinkButtonComponent Disable()
	{
		this.Disabled = true;
		return this;
	}

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordLinkButtonComponent WithId(uint id)
	{
		this.Id = id;
		return this;
	}
}
