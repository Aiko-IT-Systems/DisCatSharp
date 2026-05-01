using System;
using System.Text.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal static class DiscordWebhookEventEnvelopeParser
{
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

			var applicationId = TryGetString(root, "application_id");
			var eventType = TryGetNestedString(root, "event", "type") ?? TryGetString(root, "event_type");

			return DiscordWebhookEventEnvelopeParseResult.Parsed(
				new DiscordWebhookEventEnvelope(type, payload, root.Clone(), applicationId, eventType));
		}
		catch (JsonException exception)
		{
			return DiscordWebhookEventEnvelopeParseResult.Invalid($"The Discord webhook event payload was not valid JSON: {exception.Message}");
		}
	}

	private static string? TryGetNestedString(JsonElement root, string parentPropertyName, string childPropertyName)
	{
		if (!root.TryGetProperty(parentPropertyName, out var parentProperty) || parentProperty.ValueKind != JsonValueKind.Object)
			return null;

		return TryGetString(parentProperty, childPropertyName);
	}

	private static string? TryGetString(JsonElement root, string propertyName)
	{
		if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
			return null;

		return property.GetString();
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
