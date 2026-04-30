using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Coordinates transport-agnostic ingress signature validation.
/// </summary>
public interface IDiscordIngressSignatureValidationService
{
	/// <summary>
	///     Validates the supplied ingress request against the registered signature validators.
	/// </summary>
	/// <param name="request">The request to validate.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The aggregated validation outcome.</returns>
	ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default);
}
