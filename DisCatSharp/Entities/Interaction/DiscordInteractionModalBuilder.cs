using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Constructs an interaction modal response.
/// </summary>
public sealed class DiscordInteractionModalBuilder
{
	/// <summary>
	///     Represents the title associated with the current instance.
	/// </summary>
	/// <remarks>This field is private and is used internally to store the title value.</remarks>
	private string _title;

	/// <summary>
	///     Constructs a new empty interaction modal builder.
	/// </summary>
	public DiscordInteractionModalBuilder(string? title = null, string? customId = null)
	{
		this.Title = title ?? "Title";
		this.CustomId = customId ?? Guid.NewGuid().ToString();
	}

	/// <summary>
	///     The components.
	/// </summary>
	internal List<DiscordComponent>? ComponentsInternal { get; set; } = null;

	/// <summary>
	///     Components to send on this interaction response.
	/// </summary>
	public IReadOnlyList<DiscordComponent>? Components
		=> this.ComponentsInternal;

	/// <summary>
	///     Title of modal.
	/// </summary>
	public string Title
	{
		get => this._title;
		set
		{
			if (value is { Length: > 128 })
				throw new ArgumentException("Title length cannot exceed 128 characters.", nameof(value));

			this._title = value;
		}
	}

	/// <summary>
	///     Custom id of modal.
	/// </summary>
	public string CustomId { get; set; }

	/// <summary>
	///     Components to send on this interaction response.
	/// </summary>
	[DiscordDeprecated("Use Components instead")]
	public IReadOnlyList<DiscordActionRowComponent>? ModalComponents
		=> this.Components?.Select(c => c as DiscordActionRowComponent).ToList()!;

	/// <summary>
	///     Sets the title of the modal.
	/// </summary>
	/// <param name="title">The title to set.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionModalBuilder WithTitle(string title)
	{
		this.Title = title;
		return this;
	}

	/// <summary>
	///     Sets the custom id of the modal.
	/// </summary>
	/// <param name="customId">The custom id to set.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionModalBuilder WithCustomId(string customId)
	{
		this.CustomId = customId;
		return this;
	}

	/// <summary>
	///     Appends a collection of text components to the builder. Each call will append to a new row.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>'
	[DiscordDeprecated("Use AddLabelComponents")]
	public DiscordInteractionModalBuilder AddTextComponents(params DiscordTextInputComponent[] components)
		=> this.AddModalComponents(components);

	/// <summary>
	///     Appends a text component to the builder.
	/// </summary>
	/// <param name="component">The component to append.</param>
	/// <returns>The current builder to chain calls with.</returns>
	[DiscordDeprecated("Use AddLabelComponent")]
	public DiscordInteractionModalBuilder AddTextComponent(DiscordTextInputComponent component)
		=> this.AddModalComponents(component);

	/// <summary>
	///     Appends a collection of label components to the builder.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>'
	[DiscordDeprecated("Use AddLabelComponents")]
	public DiscordInteractionModalBuilder AddLabelComponents(params DiscordLabelComponent[] components)
		=> this.AddModalComponents(components);

	/// <summary>
	///     Appends a label component to the builder.
	/// </summary>
	/// <param name="component">The component to append.</param>
	/// <returns>The current builder to chain calls with.</returns>
	[DiscordDeprecated("Use AddLabelComponent")]
	public DiscordInteractionModalBuilder AddLabelComponent(DiscordLabelComponent component)
		=> this.AddModalComponents(component);

	/// <summary>
	///     Appends a collection of components to the builder.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionModalBuilder AddModalComponents(params DiscordComponent[] components)
	{
		this.ComponentsInternal ??= [];
		var ara = components.ToArray();
		if (ara.Length > 5)

			throw new ArgumentException("You can only add 5 components to modals.");

		if (this.ComponentsInternal.Count + ara.Length > 5)
			throw new ArgumentException($"You try to add too many components. We already have {this.ComponentsInternal.Count}.");

		if (components.All(x => x.Type is ComponentType.TextInput))
			foreach (var ar in ara)
				this.ComponentsInternal.Add(new DiscordActionRowComponent(
					[ar]));
		else
			this.ComponentsInternal.AddRange(ara);

		return this;
	}

	/// <summary>
	///     Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	[DiscordDeprecated("Use AddModalComponents with DiscordLabelComponents instead")]
	public DiscordInteractionModalBuilder AddModalComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		this.ComponentsInternal ??= [];
		var ara = components.ToArray();

		if (ara.Length + this.ComponentsInternal.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this.ComponentsInternal.Add(ar);

		return this;
	}

	/// <summary>
	///     Allows for clearing the Interaction Response Builder so that it can be used again to send a new response.
	/// </summary>
	public void Clear()
	{
		this.ComponentsInternal?.Clear();
		this.ComponentsInternal = null;
		this.Title = null!;
		this.CustomId = null!;
	}
}
