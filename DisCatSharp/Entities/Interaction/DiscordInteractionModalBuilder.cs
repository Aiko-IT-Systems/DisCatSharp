using System;
using System.Collections.Generic;
using System.Linq;

namespace DisCatSharp.Entities;

/// <summary>
/// Constructs an interaction modal response.
/// </summary>
public sealed class DiscordInteractionModalBuilder
{
	/// <summary>
	/// Title of modal.
	/// </summary>
	public string Title
	{
		get => this._title;
		set
		{
			if (value != null && value.Length > 128)
				throw new ArgumentException("Title length cannot exceed 128 characters.", nameof(value));

			this._title = value;
		}
	}

	private string _title;

	/// <summary>
	/// Custom id of modal.
	/// </summary>
	public string CustomId { get; set; }

	/// <summary>
	/// Components to send on this interaction response.
	/// </summary>
	public IReadOnlyList<DiscordActionRowComponent> ModalComponents => this._components;

	private readonly List<DiscordActionRowComponent> _components = new();

	/// <summary>
	/// The hints to send on this interaction response.
	/// </summary>
	public IReadOnlyList<DiscordInteractionCallbackHint> CallbackHints => this._callbackHints;

	private readonly List<DiscordInteractionCallbackHint> _callbackHints = new();

	/// <summary>
	/// Constructs a new empty interaction modal builder.
	/// </summary>
	public DiscordInteractionModalBuilder(string title = null, string customId = null)
	{
		this.Title = title ?? "Title";
		this.CustomId = customId ?? Guid.NewGuid().ToString();
	}

	/// <summary>
	/// Sets the title of the modal.
	/// </summary>
	/// <param name="title">The title to set.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionModalBuilder WithTitle(string title)
	{
		this.Title = title;
		return this;
	}

	/// <summary>
	/// Sets the custom id of the modal.
	/// </summary>
	/// <param name="customId">The custom id to set.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionModalBuilder WithCustomId(string customId)
	{
		this.CustomId = customId;
		return this;
	}

	/// <summary>
	/// Provides the interaction response with <see cref="DiscordInteractionCallbackHint"/>s.
	/// </summary>
	/// <param name="hintBuilder">The hint builder.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="hintBuilder"/> is <see langword="null"/>.</exception>
	public DiscordInteractionModalBuilder WithCallbackHints(DiscordCallbackHintBuilder hintBuilder)
	{
		if (hintBuilder == null)
			throw new ArgumentNullException(nameof(hintBuilder), "Callback hint builder cannot be null.");

		if (!hintBuilder.CallbackHints.Any())
			return this;

		this._callbackHints.Clear();
		this._callbackHints.AddRange(hintBuilder.CallbackHints);
		return this;
	}

	/// <summary>
	/// Appends a collection of text components to the builder. Each call will append to a new row.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionModalBuilder AddTextComponents(params DiscordTextComponent[] components)
		=> this.AddModalComponents(components);

	/// <summary>
	/// Appends a collection of select components to the builder. Each call will append to a new row.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionModalBuilder AddSelectComponents(params DiscordBaseSelectComponent[] components)
		=> this.AddModalComponents(components);

	/// <summary>
	/// Appends a text component to the builder.
	/// </summary>
	/// <param name="component">The component to append.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionModalBuilder AddTextComponent(DiscordTextComponent component)
		=> this.AddModalComponents(component);

	/// <summary>
	/// Appends a select component to the builder.
	/// </summary>
	/// <param name="component">The component to append.</param>
	/// <returns>The current builder to chain calls with.</returns>
	public DiscordInteractionModalBuilder AddSelectComponent(DiscordBaseSelectComponent component)
		=> this.AddModalComponents(component);

	/// <summary>
	/// Appends a collection of components to the builder.
	/// </summary>
	/// <param name="components">The components to append. Up to five.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionModalBuilder AddModalComponents(params DiscordComponent[] components)
	{
		var ara = components.ToArray();
		if (ara.Length > 5)

			throw new ArgumentException("You can only add 5 components to modals.");

		if (this._components.Count + ara.Length > 5)
			throw new ArgumentException($"You try to add too many components. We already have {this._components.Count}.");

		foreach (var ar in ara)
			this._components.Add(new(new List<DiscordComponent>()
			{
				ar
			}));

		return this;
	}

	/// <summary>
	/// Appends several rows of components to the message
	/// </summary>
	/// <param name="components">The rows of components to add, holding up to five each.</param>
	/// <returns>The current builder to chain calls with.</returns>
	/// <exception cref="ArgumentException">Thrown when passing more than 5 components.</exception>
	public DiscordInteractionModalBuilder AddModalComponents(IEnumerable<DiscordActionRowComponent> components)
	{
		var ara = components.ToArray();

		if (ara.Length + this._components.Count > 5)
			throw new ArgumentException("ActionRow count exceeds maximum of five.");

		foreach (var ar in ara)
			this._components.Add(ar);

		return this;
	}

	/// <summary>
	/// Appends a collection of components to the builder. Each call will append to a new row.
	/// </summary>
	/// <param name="component">The component to append.</param>
	/// <returns>The current builder to chain calls with.</returns>
	internal DiscordInteractionModalBuilder AddModalComponents(DiscordComponent component)
	{
		this._components.Add(new(new List<DiscordComponent>()
		{
			component
		}));

		return this;
	}

	/// <summary>
	/// Clears all message components on this builder.
	/// </summary>
	public void ClearComponents()
		=> this._components.Clear();

	/// <summary>
	/// Allows for clearing the Interaction Response Builder so that it can be used again to send a new response.
	/// </summary>
	public void Clear()
	{
		this._components.Clear();
		this.Title = null!;
		this.CustomId = null!;
	}
}
