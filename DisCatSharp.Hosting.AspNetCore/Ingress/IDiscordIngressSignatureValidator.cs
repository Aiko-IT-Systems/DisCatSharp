using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Validates an ingress request signature for a specific provider or scheme.
/// </summary>
public interface IDiscordIngressSignatureValidator
{
	/// <summary>
	///     Validates the supplied ingress request.
	/// </summary>
	/// <param name="request">The request to validate.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The validation outcome.</returns>
	ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default);
}
