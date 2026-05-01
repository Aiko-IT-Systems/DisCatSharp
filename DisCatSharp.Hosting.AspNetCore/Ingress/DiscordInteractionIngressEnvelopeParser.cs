using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal static class DiscordInteractionIngressEnvelopeParser
{
	public static DiscordInteractionIngressEnvelopeParseResult Parse(DiscordIngressPayload payload)
	{
		ArgumentNullException.ThrowIfNull(payload);

		try
		{
			var json = payload.GetString();
			var document = JObject.Parse(json);
			var rawType = document["type"]?.Value<int?>();
			if (rawType is null)
				return DiscordInteractionIngressEnvelopeParseResult.Failure("The interaction payload is missing the required type field.");

			var envelope = new DiscordInteractionIngressEnvelope(
				(InteractionType)rawType.Value,
				document,
				TryReadSnowflake(document["id"]),
				TryReadSnowflake(document["application_id"]),
				document["token"]?.Value<string>());

			return DiscordInteractionIngressEnvelopeParseResult.Success(envelope);
		}
		catch (JsonException ex)
		{
			return DiscordInteractionIngressEnvelopeParseResult.Failure(ex.Message);
		}
	}

	private static ulong? TryReadSnowflake(JToken? token)
	{
		if (token is null)
			return null;

		if (token.Type == JTokenType.Integer)
			return token.Value<ulong?>();

		var value = token.Value<string>();
		return ulong.TryParse(value, out var snowflake) ? snowflake : null;
	}
}

internal readonly record struct DiscordInteractionIngressEnvelopeParseResult(
	bool IsSuccess,
	DiscordInteractionIngressEnvelope? Envelope,
	string? FailureReason)
{
	public static DiscordInteractionIngressEnvelopeParseResult Success(DiscordInteractionIngressEnvelope envelope)
		=> new(true, envelope, null);

	public static DiscordInteractionIngressEnvelopeParseResult Failure(string failureReason)
		=> new(false, null, failureReason);
}
