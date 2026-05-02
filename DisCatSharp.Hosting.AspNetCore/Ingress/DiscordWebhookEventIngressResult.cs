using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordWebhookEventIngressResult
{
	private DiscordWebhookEventIngressResult(
		DiscordIngressResponse response,
		DiscordIngressSignatureValidationResult signatureValidation,
		DiscordWebhookEventEnvelope? envelope = null,
		string? failureReason = null
	)
	{
		this.Response = response;
		this.SignatureValidation = signatureValidation;
		this.Envelope = envelope;
		this.FailureReason = failureReason;
	}

	public DiscordIngressResponse Response { get; }

	public DiscordIngressSignatureValidationResult SignatureValidation { get; }

	public DiscordWebhookEventEnvelope? Envelope { get; }

	public string? FailureReason { get; }

	public static DiscordWebhookEventIngressResult Acknowledged(
		DiscordIngressSignatureValidationResult signatureValidation,
		DiscordWebhookEventEnvelope envelope
	)
		=> new(DiscordIngressResponse.Acknowledge(), signatureValidation, envelope);

	public static DiscordWebhookEventIngressResult BadRequest(
		DiscordIngressSignatureValidationResult signatureValidation,
		string failureReason
	)
		=> new(DiscordIngressResponse.Empty(StatusCodes.Status400BadRequest), signatureValidation, failureReason: failureReason);

	public static DiscordWebhookEventIngressResult Unauthorized(DiscordIngressSignatureValidationResult signatureValidation)
		=> new(
			DiscordIngressResponse.Empty(StatusCodes.Status401Unauthorized),
			signatureValidation,
			failureReason: signatureValidation.FailureReason);
}
