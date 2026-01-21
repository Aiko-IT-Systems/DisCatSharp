using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a radio group component. Modal-only.
/// </summary>
public sealed class DiscordRadioGroupComponent : DiscordComponent, ILabelComponent
{
	/// <summary>
	///     Creates a new empty radio group component.
	/// </summary>
	internal DiscordRadioGroupComponent()
	{
		this.Type = ComponentType.RadioGroup;
	}

	/// <summary>
	///     Creates a new radio group component with the provided options.
	/// </summary>
	/// <param name="options">The selectable options. Must contain between 2 and 10 entries.</param>
	/// <param name="customId">The custom id for this component.</param>
	/// <param name="required">Whether a selection is required.</param>
	public DiscordRadioGroupComponent(IEnumerable<DiscordRadioGroupComponentOption> options, string? customId = null, bool? required = null)
		: this()
	{
		ArgumentNullException.ThrowIfNull(options);
		var optionList = options.ToList();
		if (optionList.Count is < 2 or > 10)
			throw new ArgumentException("Radio groups must include between 2 and 10 options.");
		if (optionList.Count(x => x.Default) > 1)
			throw new ArgumentException("Only one radio option can be marked as default.");

		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.Options = optionList;
		this.Required = required;
	}

	/// <summary>
	///     The custom id for this component.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public override string? CustomId { get; internal set; } = Guid.NewGuid().ToString();

	/// <summary>
	///     The available options.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordRadioGroupComponentOption> Options { get; internal set; } = Array.Empty<DiscordRadioGroupComponentOption>();

	/// <summary>
	///     Whether the component requires a selection.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Required { get; internal set; }

	/// <summary>
	///     The submitted value. Present on modal submit interactions.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string? SelectedValue { get; internal set; }

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordRadioGroupComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
