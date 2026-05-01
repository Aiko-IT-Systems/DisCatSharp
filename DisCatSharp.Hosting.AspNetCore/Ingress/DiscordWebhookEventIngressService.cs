using System;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordWebhookEventIngressService
{
	private readonly IDiscordIngressSignatureValidationService _signatureValidationService;

	public DiscordWebhookEventIngressService(IDiscordIngressSignatureValidationService signatureValidationService)
	{
		this._signatureValidationService = signatureValidationService ?? throw new ArgumentNullException(nameof(signatureValidationService));
	}

	public async ValueTask<DiscordWebhookEventIngressResult> HandleAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var signatureValidation = await this._signatureValidationService.ValidateAsync(request, cancellationToken);
		if (signatureValidation.Status != DiscordIngressSignatureValidationStatus.Valid)
			return DiscordWebhookEventIngressResult.Unauthorized(signatureValidation);

		var parseResult = DiscordWebhookEventEnvelopeParser.Parse(request.Body);
		if (!parseResult.IsSuccess)
			return DiscordWebhookEventIngressResult.BadRequest(signatureValidation, parseResult.FailureReason!);

		return DiscordWebhookEventIngressResult.Acknowledged(signatureValidation, parseResult.Envelope!);
	}
}
