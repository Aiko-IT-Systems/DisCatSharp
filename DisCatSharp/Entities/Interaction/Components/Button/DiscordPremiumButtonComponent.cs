using DisCatSharp.Attributes;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a premium button. Clicking a premium button does not send an interaction.
///     <para>Requires your app to have monitization enabled.</para>
/// </summary>
public class DiscordPremiumButtonComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordPremiumButtonComponent" />. This type of button does not send back and
	///     interaction when pressed.
	/// </summary>
	/// <param name="skuId">The sku id to set the button to.</param>
	/// <param name="disabled">Whether or not this button can be pressed.</param>
	[RequiresFeature(Features.MonetizedApplication)]
	public DiscordPremiumButtonComponent(ulong skuId, bool disabled = false)
		: this()
	{
		this.SkuId = skuId;
		this.Disabled = disabled;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPremiumButtonComponent" /> class.
	/// </summary>
	public DiscordPremiumButtonComponent()
	{
		this.Type = ComponentType.Button;
	}

	/// <summary>
	///     The premium sku to open when pressing this button.
	/// </summary>
	[JsonProperty("sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong SkuId { get; set; }

	/// <summary>
	///     Whether this button can be pressed.
	/// </summary>
	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; set; }

	/// <summary>
	///     Gets the style.
	/// </summary>
	[JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
	internal ButtonStyle Style { get; set; } = ButtonStyle.Premium;

	/// <summary>
	///     Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordPremiumButtonComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	///     Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordPremiumButtonComponent Disable()
	{
		this.Disabled = true;
		return this;
	}
}
