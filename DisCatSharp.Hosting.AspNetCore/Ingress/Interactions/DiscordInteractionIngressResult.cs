using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.Interactions;

internal sealed class DiscordInteractionIngressResult
{
	private DiscordInteractionIngressResult(
		DiscordIngressResponse response,
		DiscordIngressSignatureValidationResult signatureValidation,
		DiscordInteractionIngressEnvelope? envelope = null,
		string? failureReason = null)
	{
		this.Response = response;
		this.SignatureValidation = signatureValidation;
		this.Envelope = envelope;
		this.FailureReason = failureReason;
	}

	public DiscordIngressResponse Response { get; }

	public DiscordIngressSignatureValidationResult SignatureValidation { get; }

	public DiscordInteractionIngressEnvelope? Envelope { get; }

	public string? FailureReason { get; }

	public static DiscordInteractionIngressResult Handled(
		DiscordIngressSignatureValidationResult signatureValidation,
		DiscordInteractionIngressEnvelope envelope,
		DiscordInteractionIngressResponse response)
		=> new(response.Response, signatureValidation, envelope);

	public static DiscordInteractionIngressResult BadRequest(
		DiscordIngressSignatureValidationResult signatureValidation,
		string failureReason)
		=> new(DiscordIngressResponse.Empty(StatusCodes.Status400BadRequest), signatureValidation, failureReason: failureReason);

	public static DiscordInteractionIngressResult Unauthorized(DiscordIngressSignatureValidationResult signatureValidation)
		=> new(
			DiscordIngressResponse.Empty(StatusCodes.Status401Unauthorized),
			signatureValidation,
			failureReason: signatureValidation.FailureReason);

	public static DiscordInteractionIngressResult NotImplemented(
		DiscordIngressSignatureValidationResult signatureValidation,
		DiscordInteractionIngressEnvelope envelope)
		=> new(
			DiscordIngressResponse.Empty(StatusCodes.Status501NotImplemented),
			signatureValidation,
			envelope,
			"No registered Discord interaction ingress handler produced a response.");
}
