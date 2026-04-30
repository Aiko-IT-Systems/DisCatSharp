using System;

using DisCatSharp.Enums;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents the parsed top-level Discord interaction payload received over HTTP ingress.
/// </summary>
public sealed class DiscordInteractionIngressEnvelope
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordInteractionIngressEnvelope" /> class.
	/// </summary>
	/// <param name="type">The interaction type.</param>
	/// <param name="payload">The parsed interaction payload.</param>
	/// <param name="interactionId">The interaction identifier when present.</param>
	/// <param name="applicationId">The application identifier when present.</param>
	/// <param name="token">The interaction continuation token when present.</param>
	public DiscordInteractionIngressEnvelope(
		InteractionType type,
		JObject payload,
		ulong? interactionId = null,
		ulong? applicationId = null,
		string? token = null
	)
	{
		this.Type = type;
		this.Payload = payload ?? throw new ArgumentNullException(nameof(payload));
		this.InteractionId = interactionId;
		this.ApplicationId = applicationId;
		this.Token = token;
	}

	/// <summary>
	///     Gets the interaction type.
	/// </summary>
	public InteractionType Type { get; }

	/// <summary>
	///     Gets the raw parsed JSON payload.
	/// </summary>
	public JObject Payload { get; }

	/// <summary>
	///     Gets the interaction identifier when available.
	/// </summary>
	public ulong? InteractionId { get; }

	/// <summary>
	///     Gets the application identifier when available.
	/// </summary>
	public ulong? ApplicationId { get; }

	/// <summary>
	///     Gets the interaction token when available.
	/// </summary>
	public string? Token { get; }

	/// <summary>
	///     Gets a value indicating whether this payload is a Discord ping challenge.
	/// </summary>
	public bool IsPing => this.Type == InteractionType.Ping;
}
