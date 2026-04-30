using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Validates Discord ingress requests signed with the application's Ed25519 verify key.
/// </summary>
public sealed class DiscordEd25519IngressSignatureValidator : IDiscordIngressSignatureValidator
{
	private const string ValidatorName = "discord-ed25519";
	private static readonly Encoding TimestampEncoding = Encoding.ASCII;
	private readonly Ed25519PublicKeyParameters? _publicKey;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordEd25519IngressSignatureValidator" /> class.
	/// </summary>
	/// <param name="options">The ingress options.</param>
	public DiscordEd25519IngressSignatureValidator(IOptions<DiscordWebIngressOptions> options)
	{
		ArgumentNullException.ThrowIfNull(options);

		if (string.IsNullOrWhiteSpace(options.Value.ApplicationVerifyKey))
			return;

		this._publicKey = new Ed25519PublicKeyParameters(Convert.FromHexString(options.Value.ApplicationVerifyKey.Trim()), 0);
	}

	/// <inheritdoc />
	public ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		cancellationToken.ThrowIfCancellationRequested();

		if (this._publicKey is null)
			return new ValueTask<DiscordIngressSignatureValidationResult>(DiscordIngressSignatureValidationResult.NotValidated(ValidatorName));

		if (!request.TryGetSingleHeaderValue(DiscordIngressHeaderNames.SignatureTimestamp, out var timestamp) || string.IsNullOrWhiteSpace(timestamp))
			return new ValueTask<DiscordIngressSignatureValidationResult>(DiscordIngressSignatureValidationResult.Invalid(ValidatorName, "Missing Discord signature timestamp header."));

		if (!request.TryGetSingleHeaderValue(DiscordIngressHeaderNames.SignatureEd25519, out var signatureHex) || string.IsNullOrWhiteSpace(signatureHex))
			return new ValueTask<DiscordIngressSignatureValidationResult>(DiscordIngressSignatureValidationResult.Invalid(ValidatorName, "Missing Discord signature header."));

		if (!TryDecodeHex(signatureHex, 64, out var signature))
			return new ValueTask<DiscordIngressSignatureValidationResult>(DiscordIngressSignatureValidationResult.Invalid(ValidatorName, "The Discord signature header was not a 64-byte hex string."));

		var timestampBytes = TimestampEncoding.GetBytes(timestamp);
		var message = new byte[timestampBytes.Length + request.Body.Length];
		timestampBytes.CopyTo(message, 0);
		request.Body.Bytes.Span.CopyTo(message.AsSpan(timestampBytes.Length));

		Ed25519Signer verifier = new();
		verifier.Init(false, this._publicKey);
		verifier.BlockUpdate(message, 0, message.Length);

		return new ValueTask<DiscordIngressSignatureValidationResult>(
			verifier.VerifySignature(signature)
				? DiscordIngressSignatureValidationResult.Valid(ValidatorName)
				: DiscordIngressSignatureValidationResult.Invalid(ValidatorName, "Discord Ed25519 signature verification failed."));
	}

	private static bool TryDecodeHex(string value, int expectedByteLength, out byte[]? bytes)
	{
		bytes = null;

		var normalizedValue = value.Trim();
		if (normalizedValue.Length != expectedByteLength * 2)
			return false;

		try
		{
			bytes = Convert.FromHexString(normalizedValue);
			return bytes.Length == expectedByteLength;
		}
		catch (FormatException)
		{
			return false;
		}
	}
}
