using System;
using System.Text.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Parses signed Discord webhook JSON payloads into transport-neutral webhook envelopes.
/// </summary>
/// <remarks>
///     The parser preserves raw JSON elements for later typed deserialization while validating the minimal envelope contract required
///     by the dispatcher.
/// </remarks>
internal static class DiscordWebhookEventEnvelopeParser
{
	/// <summary>
	///     Parses a signed webhook payload.
	/// </summary>
	/// <param name="payload">The raw ingress payload.</param>
	/// <returns>The parse result.</returns>
	public static DiscordWebhookEventEnvelopeParseResult Parse(DiscordIngressPayload payload)
	{
		ArgumentNullException.ThrowIfNull(payload);

		if (payload.IsEmpty)
			return DiscordWebhookEventEnvelopeParseResult.Invalid("The Discord webhook event request body was empty.");

		try
		{
			using var document = JsonDocument.Parse(payload.Bytes);
			if (document.RootElement.ValueKind != JsonValueKind.Object)
				return DiscordWebhookEventEnvelopeParseResult.Invalid("The Discord webhook event payload must be a JSON object.");

			var root = document.RootElement;
			if (!root.TryGetProperty("type", out var typeProperty))
				return DiscordWebhookEventEnvelopeParseResult.Invalid("The Discord webhook event payload is missing the required 'type' field.");

			if (typeProperty.ValueKind != JsonValueKind.Number || !typeProperty.TryGetInt32(out var type))
				return DiscordWebhookEventEnvelopeParseResult.Invalid("The Discord webhook event payload 'type' field must be an integer.");

			var version = TryGetInt32(root, "version");
			var applicationId = TryGetString(root, "application_id");
			var eventBody = TryGetObject(root, "event");
			var eventType = TryGetString(eventBody, "type") ?? TryGetString(root, "event_type");
			var timestamp = TryGetString(eventBody, "timestamp");
			var data = TryGetObject(eventBody, "data");

			return DiscordWebhookEventEnvelopeParseResult.Parsed(
				new DiscordWebhookEventEnvelope(type, payload, root.Clone(), version, applicationId, eventType, timestamp, eventBody, data));
		}
		catch (JsonException exception)
		{
			return DiscordWebhookEventEnvelopeParseResult.Invalid($"The Discord webhook event payload was not valid JSON: {exception.Message}");
		}
	}

	private static int? TryGetInt32(JsonElement root, string propertyName)
	{
		return !root.TryGetProperty(propertyName, out var property) || property.ValueKind is not JsonValueKind.Number || !property.TryGetInt32(out var value)
			? null
			: value;
	}

	private static JsonElement TryGetObject(JsonElement root, string propertyName)
	{
		return root.ValueKind is not JsonValueKind.Object || !root.TryGetProperty(propertyName, out var property) || property.ValueKind is not JsonValueKind.Object
			? default
			: property.Clone();
	}

	private static string? TryGetString(JsonElement root, string propertyName)
	{
		return root.ValueKind is not JsonValueKind.Object || !root.TryGetProperty(propertyName, out var property) || property.ValueKind is not JsonValueKind.String
			? null
			: property.GetString();
	}
}

internal readonly record struct DiscordWebhookEventEnvelopeParseResult(DiscordWebhookEventEnvelope? Envelope, string? FailureReason)
{
	public bool IsSuccess => this.Envelope is not null;

	public static DiscordWebhookEventEnvelopeParseResult Parsed(DiscordWebhookEventEnvelope envelope)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		return new DiscordWebhookEventEnvelopeParseResult(envelope, null);
	}

	public static DiscordWebhookEventEnvelopeParseResult Invalid(string failureReason)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(failureReason);

		return new DiscordWebhookEventEnvelopeParseResult(null, failureReason);
	}
}
