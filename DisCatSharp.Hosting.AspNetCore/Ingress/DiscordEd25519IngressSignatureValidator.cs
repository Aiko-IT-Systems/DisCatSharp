using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
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
	private readonly ILogger<DiscordEd25519IngressSignatureValidator> _logger;
	private readonly Ed25519PublicKeyParameters? _publicKey;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordEd25519IngressSignatureValidator" /> class.
	/// </summary>
	/// <param name="options">The ingress options.</param>
	/// <param name="logger">The diagnostic logger.</param>
	public DiscordEd25519IngressSignatureValidator(
		IOptions<DiscordWebIngressOptions> options,
		ILogger<DiscordEd25519IngressSignatureValidator> logger
	)
	{
		ArgumentNullException.ThrowIfNull(options);
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

		if (string.IsNullOrWhiteSpace(options.Value.ApplicationVerifyKey))
			return;

		this._publicKey = new Ed25519PublicKeyParameters(Convert.FromHexString(options.Value.ApplicationVerifyKey.Trim()), 0);
	}

	/// <inheritdoc />
	public ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		cancellationToken.ThrowIfCancellationRequested();

		var timestampHeaderCount = GetHeaderCount(request, DiscordIngressHeaderNames.SignatureTimestamp);
		var signatureHeaderCount = GetHeaderCount(request, DiscordIngressHeaderNames.SignatureEd25519);
		if (this._publicKey is null)
		{
			this._logger.LogDebug(
				"Skipped Discord Ed25519 signature validation for {RequestUri} because no application verify key was configured.",
				request.RequestUri);
			return new ValueTask<DiscordIngressSignatureValidationResult>(DiscordIngressSignatureValidationResult.NotValidated(ValidatorName));
		}

		if (!request.TryGetSingleHeaderValue(DiscordIngressHeaderNames.SignatureTimestamp, out var timestamp) || string.IsNullOrWhiteSpace(timestamp))
			return Reject(request, timestampHeaderCount, signatureHeaderCount, timestamp?.Length, null, "Missing Discord signature timestamp header.");

		if (!request.TryGetSingleHeaderValue(DiscordIngressHeaderNames.SignatureEd25519, out var signatureHex) || string.IsNullOrWhiteSpace(signatureHex))
			return Reject(request, timestampHeaderCount, signatureHeaderCount, timestamp.Length, signatureHex?.Length, "Missing Discord signature header.");

		if (!TryDecodeHex(signatureHex, 64, out var signature))
			return Reject(request, timestampHeaderCount, signatureHeaderCount, timestamp.Length, signatureHex.Length, "The Discord signature header was not a 64-byte hex string.");

		var timestampBytes = TimestampEncoding.GetBytes(timestamp);
		var message = new byte[timestampBytes.Length + request.Body.Length];
		timestampBytes.CopyTo(message, 0);
		request.Body.Bytes.Span.CopyTo(message.AsSpan(timestampBytes.Length));

		Ed25519Signer verifier = new();
		verifier.Init(false, this._publicKey);
		verifier.BlockUpdate(message, 0, message.Length);

		if (verifier.VerifySignature(signature))
		{
			this._logger.LogDebug(
				"Validated Discord Ed25519 signature for {RequestUri}. BodyLength: {BodyLength}, TimestampLength: {TimestampLength}, SignatureLength: {SignatureLength}, TimestampHeaderCount: {TimestampHeaderCount}, SignatureHeaderCount: {SignatureHeaderCount}.",
				request.RequestUri,
				request.Body.Length,
				timestamp.Length,
				signatureHex.Length,
				timestampHeaderCount,
				signatureHeaderCount);
			return new ValueTask<DiscordIngressSignatureValidationResult>(DiscordIngressSignatureValidationResult.Valid(ValidatorName));
		}

		return Reject(request, timestampHeaderCount, signatureHeaderCount, timestamp.Length, signatureHex.Length, "Discord Ed25519 signature verification failed.");
	}

	private ValueTask<DiscordIngressSignatureValidationResult> Reject(
		DiscordIngressRequest request,
		int timestampHeaderCount,
		int signatureHeaderCount,
		int? timestampLength,
		int? signatureLength,
		string failureReason
	)
	{
		this._logger.LogWarning(
			"Rejected Discord signed ingress request for {RequestUri}: {FailureReason}.",
			request.RequestUri,
			failureReason);

		this._logger.LogDebug(
			"Discord signed ingress request details for {RequestUri}. BodyLength: {BodyLength}, TimestampLength: {TimestampLength}, SignatureLength: {SignatureLength}, TimestampHeaderCount: {TimestampHeaderCount}, SignatureHeaderCount: {SignatureHeaderCount}.",
			request.RequestUri,
			request.Body.Length,
			timestampLength,
			signatureLength,
			timestampHeaderCount,
			signatureHeaderCount);

		return new ValueTask<DiscordIngressSignatureValidationResult>(DiscordIngressSignatureValidationResult.Invalid(ValidatorName, failureReason));
	}

	private static int GetHeaderCount(DiscordIngressRequest request, string headerName)
		=> request.TryGetHeader(headerName, out var headerValues) ? headerValues.Count : 0;

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
