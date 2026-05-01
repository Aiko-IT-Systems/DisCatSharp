using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Aggregates the registered ingress signature validators.
/// </summary>
public sealed class DiscordIngressSignatureValidationService : IDiscordIngressSignatureValidationService
{
	private readonly IReadOnlyCollection<IDiscordIngressSignatureValidator> _validators;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressSignatureValidationService" /> class.
	/// </summary>
	/// <param name="validators">The signature validators to aggregate.</param>
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
