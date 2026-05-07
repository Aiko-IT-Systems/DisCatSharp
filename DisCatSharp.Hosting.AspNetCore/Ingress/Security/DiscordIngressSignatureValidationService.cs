using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.Security;

/// <summary>
///     Aggregates the registered ingress signature validators.
/// </summary>
/// <remarks>
///     This aggregator is intentionally order-sensitive so applications can prepend custom validators ahead of the built-in Discord
///     Ed25519 validator.
/// </remarks>
public sealed class DiscordIngressSignatureValidationService : IDiscordIngressSignatureValidationService
{
	private readonly IReadOnlyCollection<IDiscordIngressSignatureValidator> _validators;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressSignatureValidationService" /> class.
	/// </summary>
	/// <param name="validators">The signature validators to aggregate.</param>
	/// <exception cref="ArgumentNullException"><paramref name="validators" /> is <see langword="null" />.</exception>
	public DiscordIngressSignatureValidationService(IEnumerable<IDiscordIngressSignatureValidator> validators)
	{
		ArgumentNullException.ThrowIfNull(validators);

		this._validators = [.. validators];
	}

	/// <inheritdoc />
	public async ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		cancellationToken.ThrowIfCancellationRequested();

		DiscordIngressSignatureValidationResult? firstInvalidResult = null;
		foreach (var validator in this._validators)
		{
			var result = await validator.ValidateAsync(request, cancellationToken);

			if (result.Status == DiscordIngressSignatureValidationStatus.Valid)
				return result;

			if (result.Status == DiscordIngressSignatureValidationStatus.Invalid)
				firstInvalidResult ??= result;
		}

		return firstInvalidResult ?? DiscordIngressSignatureValidationResult.NotValidated();
	}
}
