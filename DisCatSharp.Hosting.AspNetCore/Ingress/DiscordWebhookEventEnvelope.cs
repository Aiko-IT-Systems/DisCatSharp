using System;
using System.Text.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a parsed inbound Discord webhook event envelope.
/// </summary>
public sealed class DiscordWebhookEventEnvelope
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordWebhookEventEnvelope" /> class.
	/// </summary>
	/// <param name="type">The envelope type discriminator.</param>
	/// <param name="payload">The raw payload bytes.</param>
	/// <param name="root">The parsed JSON root element.</param>
	/// <param name="applicationId">The application identifier when present.</param>
	/// <param name="eventType">The event type when present.</param>
	public DiscordWebhookEventEnvelope(
		int type,
		DiscordIngressPayload payload,
		JsonElement root,
		string? applicationId = null,
		string? eventType = null
	)
	{
		ArgumentNullException.ThrowIfNull(payload);

		if (root.ValueKind != JsonValueKind.Object)
			throw new ArgumentException("The webhook event envelope root must be a JSON object.", nameof(root));

		this.Type = type;
		this.Payload = payload;
		this.Root = root;
		this.ApplicationId = applicationId;
		this.EventType = eventType;
	}

	/// <summary>
	///     Gets the webhook envelope type discriminator.
	/// </summary>
	public int Type { get; }

	/// <summary>
	///     Gets a value indicating whether the envelope represents a Discord webhook ping.
	/// </summary>
	public bool IsPing => this.Type == DiscordWebhookEventTypes.Ping;

	/// <summary>
	///     Gets the raw request payload.
	/// </summary>
	public DiscordIngressPayload Payload { get; }

	/// <summary>
	///     Gets the parsed JSON root object.
	/// </summary>
	public JsonElement Root { get; }

	/// <summary>
	///     Gets the application identifier when present on the payload.
	/// </summary>
	public string? ApplicationId { get; }

	/// <summary>
	///     Gets the event type when present on the payload.
	/// </summary>
	public string? EventType { get; }

	/// <summary>
	///     Attempts to resolve a top-level property from the envelope.
	/// </summary>
	/// <param name="name">The property name to resolve.</param>
	/// <param name="value">The resolved JSON value.</param>
	/// <returns><see langword="true" /> when the property exists.</returns>
	public bool TryGetProperty(string name, out JsonElement value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return this.Root.TryGetProperty(name, out value);
	}
}
