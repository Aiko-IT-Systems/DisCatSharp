namespace DisCatSharp.Hosting.AspNetCore.Ingress.Security;

/// <summary>
///     Describes the outcome of a signature validation attempt.
/// </summary>
public enum DiscordIngressSignatureValidationStatus
{
	/// <summary>
	///     No validator was able to evaluate the request.
	/// </summary>
	NotValidated,

	/// <summary>
	///     The request signature was valid.
	/// </summary>
	Valid,

	/// <summary>
	///     The request signature was invalid.
	/// </summary>
	Invalid
}
