using System.Collections.Generic;

using DisCatSharp.Enums;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a label component.
/// </summary>
public sealed class DiscordLabelComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordLabelComponent" />.
	/// </summary>
	internal DiscordLabelComponent()
	{
		this.Type = ComponentType.Label;
	}

	/// <summary>
	///     Constructs a new label component based on another label component.
	/// </summary>
	/// <param name="other">The label component to copy.</param>
	public DiscordLabelComponent(DiscordLabelComponent other)
		: this()
	{
		this.Label = other.Label;
		this.Description = other.Description;
		this.Component = other.Component;
		this.Id = other.Id;
	}

	/// <summary>
	///     Constructs a new label component field with the specified options.
	/// </summary>
	/// <param name="label">The label for the modal field.</param>
	/// <param name="description">The description for the modal field.</param>
	/// <param name="component">The component to attach to the label. This parameter is added for future-proof support. Please use either <see cref="WithTextComponent(DiscordTextInputComponent)"/> or <see cref="WithSelectComponent(DiscordBaseSelectComponent)"/>.</param>
	public DiscordLabelComponent(string label, string? description = null, ILabelComponent? component = null)
		: this()
	{
		this.Label = label;
		this.Description = description;
		this.Component = component;
	}

	/// <summary>
	///     Sets the text component for the label.
	/// </summary>
	/// <param name="component">The text component to attach to the label.</param>
	public DiscordLabelComponent WithTextComponent(DiscordTextInputComponent component)
	{
		this.Component = component;
		return this;
	}

	/// <summary>
	/// 	Sets the string select component for the label.
	/// </summary>
	/// <param name="component">The string select component to attach to the label.</param>
	public DiscordLabelComponent WithSelectComponent(DiscordBaseSelectComponent component)
	{
		this.Component = component;
		return this;
	}

	/// <summary>
	/// 	Sets the file upload component for the label.
	/// </summary>
	/// <param name="component">The file upload component to attach to the label.</param>
	public DiscordLabelComponent WithFileUploadComponent(DiscordFileUploadComponent component)
	{
		this.Component = component;
		return this;
	}

	/// <summary>
	///     Sets the radio group component for the label.
	/// </summary>
	/// <param name="component">The radio group component to attach to the label.</param>
	public DiscordLabelComponent WithRadioGroupComponent(DiscordRadioGroupComponent component)
	{
		this.Component = component;
		return this;
	}

	/// <summary>
	///     Sets the checkbox group component for the label.
	/// </summary>
	/// <param name="component">The checkbox group component to attach to the label.</param>
	public DiscordLabelComponent WithCheckboxGroupComponent(DiscordCheckboxGroupComponent component)
	{
		this.Component = component;
		return this;
	}

	/// <summary>
	///     Sets the checkbox component for the label.
	/// </summary>
	/// <param name="component">The checkbox component to attach to the label.</param>
	public DiscordLabelComponent WithCheckboxComponent(DiscordCheckboxComponent component)
	{
		this.Component = component;
		return this;
	}

	/// <summary>
	///     The label.
	/// </summary>
	[JsonProperty("label")]
	public string Label { get; internal set; }

	/// <summary>
	///     The description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; internal set; }

	/// <summary>
	/// 	The attached component.
	/// </summary>
	[JsonProperty("component"), JsonConverter(typeof(ILabelComponentJsonConverter))]
	public ILabelComponent Component { get; internal set; }

	/// <summary>
	///     Helper to determine the type of component attached to the label (e.g., <see cref="DiscordTextInputComponent"/>, <see cref="DiscordBaseSelectComponent"/>, <see cref="DiscordRadioGroupComponent"/>, <see cref="DiscordCheckboxGroupComponent"/>, <see cref="DiscordCheckboxComponent"/>, or <see cref="DiscordFileUploadComponent"/>).
	/// </summary>
	[JsonIgnore]
	public ComponentType SubComponentType
		=> (this.Component as DiscordComponent).Type;

	/// <inheritdoc />
	public override IEnumerable<DiscordComponent> GetChildren()
	{
		if (this.Component is DiscordComponent component)
			yield return component;
	}

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordLabelComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
