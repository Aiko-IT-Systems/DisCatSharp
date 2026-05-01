using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordWebhookEventIngressService
{
	private readonly IDiscordIngressSignatureValidationService _signatureValidationService;
	private readonly DiscordWebhookEventDispatcher _dispatcher;
	private readonly ILogger<DiscordWebhookEventIngressService> _logger;

	public DiscordWebhookEventIngressService(
		IDiscordIngressSignatureValidationService signatureValidationService,
		DiscordWebhookEventDispatcher dispatcher,
		ILogger<DiscordWebhookEventIngressService> logger
	)
	{
		this._signatureValidationService = signatureValidationService ?? throw new ArgumentNullException(nameof(signatureValidationService));
		this._dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask<DiscordWebhookEventIngressResult> HandleAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var signatureValidation = await this._signatureValidationService.ValidateAsync(request, cancellationToken);
		if (signatureValidation.Status != DiscordIngressSignatureValidationStatus.Valid)
		{
			this._logger.LogDebug(
				"Rejected Discord webhook event request for {RequestUri}. Validation status: {ValidationStatus}, Validator: {ValidatorName}, Failure: {FailureReason}, BodyLength: {BodyLength}.",
				request.RequestUri,
				signatureValidation.Status,
				signatureValidation.ValidatorName,
				signatureValidation.FailureReason,
				request.Body.Length);
			return DiscordWebhookEventIngressResult.Unauthorized(signatureValidation);
		}

		var parseResult = DiscordWebhookEventEnvelopeParser.Parse(request.Body);
		if (!parseResult.IsSuccess)
		{
			this._logger.LogWarning(
				"Rejected signed Discord webhook event request for {RequestUri}: {FailureReason}.",
				request.RequestUri,
				parseResult.FailureReason);
			return DiscordWebhookEventIngressResult.BadRequest(signatureValidation, parseResult.FailureReason!);
		}

		var envelope = parseResult.Envelope!;
		if (envelope.IsPing)
			this._logger.LogInformation("Acknowledged Discord webhook event PING for {RequestUri}.", request.RequestUri);
		else
			this._logger.LogDebug("Acknowledged Discord webhook event {EventType} for {RequestUri}.", envelope.EventType, request.RequestUri);
		this._dispatcher.EnqueueDispatch(request, envelope);

		return DiscordWebhookEventIngressResult.Acknowledged(signatureValidation, envelope);
	}
}
