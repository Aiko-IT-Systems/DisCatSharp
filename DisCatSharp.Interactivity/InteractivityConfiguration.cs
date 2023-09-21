using System;

using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Interactivity;

/// <summary>
/// Configuration class for your Interactivity extension
/// </summary>
public sealed class InteractivityConfiguration
{
	/// <summary>
	/// <para>Sets the default interactivity action timeout.</para>
	/// <para>Defaults to 1 minute.</para>
	/// </summary>
	public TimeSpan Timeout { internal get; set; } = TimeSpan.FromMinutes(1);

	/// <summary>
	/// What to do after the poll ends
	/// </summary>
	public PollBehaviour PollBehaviour { internal get; set; } = PollBehaviour.DeleteEmojis;

	/// <summary>
	/// Emojis to use for pagination
	/// </summary>
	public PaginationEmojis PaginationEmojis { internal get; set; } = new();

	/// <summary>
	/// Buttons to use for pagination.
	/// </summary>
	public PaginationButtons PaginationButtons { internal get; set; } = new();

	/// <summary>
	/// Whether interactivity should ACK buttons that are pushed. Setting this to <see langword="true"/> will also prevent subsequent event handlers from running.
	/// </summary>
	public bool AckPaginationButtons { internal get; set; }

	/// <summary>
	/// How to handle buttons after pagination ends.
	/// </summary>
	public ButtonPaginationBehavior ButtonBehavior { internal get; set; }

	/// <summary>
	/// How to handle pagination. Defaults to WrapAround.
	/// </summary>
	public PaginationBehaviour PaginationBehaviour { internal get; set; } = PaginationBehaviour.WrapAround;

	/// <summary>
	/// How to handle pagination deletion. Defaults to DeleteEmojis.
	/// </summary>
	public PaginationDeletion PaginationDeletion { internal get; set; } = PaginationDeletion.DeleteEmojis;

	/// <summary>
	/// How to handle invalid interactions. Defaults to Ignore.
	/// </summary>
	public InteractionResponseBehavior ResponseBehavior { internal get; set; } = InteractionResponseBehavior.Ignore;

	/// <summary>
	/// The message to send to the user when processing invalid interactions. Ignored if <see cref="ResponseBehavior"/> is not set to <see cref="DisCatSharp.Interactivity.Enums.InteractionResponseBehavior.Respond"/>.
	/// </summary>
	public string ResponseMessage { internal get; set; }

	/// <summary>
	/// Creates a new instance of <see cref="InteractivityConfiguration"/>.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public InteractivityConfiguration()
	{ }

	/// <summary>
	/// Creates a new instance of <see cref="InteractivityConfiguration"/>, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public InteractivityConfiguration(InteractivityConfiguration other)
	{
		this.AckPaginationButtons = other.AckPaginationButtons;
		this.PaginationButtons = other.PaginationButtons;
		this.ButtonBehavior = other.ButtonBehavior;
		this.PaginationBehaviour = other.PaginationBehaviour;
		this.PaginationDeletion = other.PaginationDeletion;
		this.ResponseBehavior = other.ResponseBehavior;
		this.PaginationEmojis = other.PaginationEmojis;
		this.ResponseMessage = other.ResponseMessage;
		this.PollBehaviour = other.PollBehaviour;
		this.Timeout = other.Timeout;

		if (this.ResponseBehavior is InteractionResponseBehavior.Respond && string.IsNullOrWhiteSpace(this.ResponseMessage))
			throw new ArgumentException($"{nameof(this.ResponseMessage)} cannot be null, empty, or whitespace when {nameof(this.ResponseBehavior)} is set to respond.");
	}
}
