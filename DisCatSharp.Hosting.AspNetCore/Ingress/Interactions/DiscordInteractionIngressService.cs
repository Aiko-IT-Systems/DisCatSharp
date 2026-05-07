using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.Interactions;

internal sealed class DiscordInteractionIngressService
{
	private readonly IReadOnlyCollection<IDiscordInteractionIngressHandler> _handlers;
	private readonly IDiscordIngressSignatureValidationService _signatureValidationService;

	public DiscordInteractionIngressService(
		IDiscordIngressSignatureValidationService signatureValidationService,
		IEnumerable<IDiscordInteractionIngressHandler> handlers)
	{
		this._signatureValidationService = signatureValidationService ?? throw new ArgumentNullException(nameof(signatureValidationService));
		ArgumentNullException.ThrowIfNull(handlers);

		this._handlers = [.. handlers];
	}

	public async ValueTask<DiscordInteractionIngressResult> HandleAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var signatureValidation = await this._signatureValidationService.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
		if (signatureValidation.Status != DiscordIngressSignatureValidationStatus.Valid)
			return DiscordInteractionIngressResult.Unauthorized(signatureValidation);

		var parseResult = DiscordInteractionIngressEnvelopeParser.Parse(request.Body);
		if (!parseResult.IsSuccess)
			return DiscordInteractionIngressResult.BadRequest(signatureValidation, parseResult.FailureReason!);

		var envelope = parseResult.Envelope!;
		if (envelope.IsPing)
			return DiscordInteractionIngressResult.Handled(signatureValidation, envelope, DiscordInteractionIngressResponse.Pong());

		var context = new DiscordInteractionIngressContext(request, envelope);
		foreach (var handler in this._handlers)
		{
			var response = await handler.HandleAsync(context, cancellationToken).ConfigureAwait(false);
			if (response is not null)
				return DiscordInteractionIngressResult.Handled(signatureValidation, envelope, response);
		}

		return DiscordInteractionIngressResult.NotImplemented(signatureValidation, envelope);
	}
}
