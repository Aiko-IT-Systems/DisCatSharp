using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a button component.
/// </summary>
public class DiscordBaseButtonComponent : DiscordSectionAccessory
{
	/// <summary>
	///     Whether this button can be pressed.
	/// </summary>
	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; internal set; }

	/// <summary>
	///     Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordBaseButtonComponent Enable()
		=> this.SetState(false);

	/// <summary>
	///     Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordBaseButtonComponent Disable()
		=> this.SetState(true);

	/// <summary>
	///     Enables or disables this component.
	/// </summary>
	/// <param name="disabled">Whether this component should be disabled.</param>
	/// <returns>The current component.</returns>
	public DiscordBaseButtonComponent SetState(bool disabled)
	{
		this.Disabled = disabled;
		return this;
	}

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordBaseButtonComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
