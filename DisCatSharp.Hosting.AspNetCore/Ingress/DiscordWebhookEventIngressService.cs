using System;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordWebhookEventIngressService
{
	private readonly IDiscordIngressSignatureValidationService _signatureValidationService;
	private readonly DiscordWebhookEventDispatcher _dispatcher;

	public DiscordWebhookEventIngressService(
		IDiscordIngressSignatureValidationService signatureValidationService,
		DiscordWebhookEventDispatcher dispatcher
	)
	{
		this._signatureValidationService = signatureValidationService ?? throw new ArgumentNullException(nameof(signatureValidationService));
		this._dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
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

		var envelope = parseResult.Envelope!;
		this._dispatcher.EnqueueDispatch(request, envelope);

		return DiscordWebhookEventIngressResult.Acknowledged(signatureValidation, envelope);
	}
}
