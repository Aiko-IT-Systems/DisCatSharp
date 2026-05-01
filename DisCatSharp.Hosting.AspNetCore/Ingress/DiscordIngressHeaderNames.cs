namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Contains the HTTP header names used by Discord signed ingress requests.
/// </summary>
public static class DiscordIngressHeaderNames
{
	/// <summary>
	///     The Discord Ed25519 signature header.
	/// </summary>
	public const string SignatureEd25519 = "X-Signature-Ed25519";

	/// <summary>
	///     The Discord signature timestamp header.
	/// </summary>
	public const string SignatureTimestamp = "X-Signature-Timestamp";
}
