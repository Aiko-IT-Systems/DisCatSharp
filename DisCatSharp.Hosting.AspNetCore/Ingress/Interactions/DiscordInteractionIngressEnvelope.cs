using System;

using DisCatSharp.Enums;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.Interactions;

/// <summary>
///     Represents the parsed top-level Discord interaction payload received over HTTP ingress.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DiscordInteractionIngressEnvelope" /> class.
/// </remarks>
/// <param name="type">The interaction type.</param>
/// <param name="payload">The parsed interaction payload.</param>
/// <param name="interactionId">The interaction identifier when present.</param>
/// <param name="applicationId">The application identifier when present.</param>
/// <param name="token">The interaction continuation token when present.</param>
public sealed class DiscordInteractionIngressEnvelope(
	InteractionType type,
	JObject payload,
	ulong? interactionId = null,
	ulong? applicationId = null,
	string? token = null
	)
{

	/// <summary>
	///     Gets the interaction type.
	/// </summary>
	public InteractionType Type { get; } = type;

	/// <summary>
	///     Gets the raw parsed JSON payload.
	/// </summary>
	public JObject Payload { get; } = payload ?? throw new ArgumentNullException(nameof(payload));

	/// <summary>
	///     Gets the interaction identifier when available.
	/// </summary>
	public ulong? InteractionId { get; } = interactionId;

	/// <summary>
	///     Gets the application identifier when available.
	/// </summary>
	public ulong? ApplicationId { get; } = applicationId;

	/// <summary>
	///     Gets the interaction token when available.
	/// </summary>
	public string? Token { get; } = token;

	/// <summary>
	///     Gets a value indicating whether this payload is a Discord ping challenge.
	/// </summary>
	public bool IsPing => this.Type == InteractionType.Ping;
}
