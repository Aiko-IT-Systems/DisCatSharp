using System;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.Security;

/// <summary>
///     Represents the outcome of an ingress signature validation attempt.
/// </summary>
/// <remarks>
///     <see cref="DiscordIngressSignatureValidationStatus.NotValidated" /> indicates that no registered validator claimed the request.
///     This is distinct from <see cref="DiscordIngressSignatureValidationStatus.Invalid" />, which indicates a validator did claim the
///     request and rejected it.
/// </remarks>
public sealed class DiscordIngressSignatureValidationResult
{
	private DiscordIngressSignatureValidationResult(
		DiscordIngressSignatureValidationStatus status,
		string? validatorName = null,
		string? failureReason = null
	)
	{
		this.Status = status;
		this.ValidatorName = validatorName;
		this.FailureReason = failureReason;
	}

	/// <summary>
	///     Gets the validation outcome.
	/// </summary>
	public DiscordIngressSignatureValidationStatus Status { get; }

	/// <summary>
	///     Gets the validator name that produced the result when one is available.
	/// </summary>
	public string? ValidatorName { get; }

	/// <summary>
	///     Gets a human-readable failure reason when validation failed.
	/// </summary>
	public string? FailureReason { get; }

	/// <summary>
	///     Creates a result that indicates no validator evaluated the request.
	/// </summary>
	/// <param name="validatorName">The validator name, when applicable.</param>
	/// <returns>A not-validated result.</returns>
	public static DiscordIngressSignatureValidationResult NotValidated(string? validatorName = null)
		=> new(DiscordIngressSignatureValidationStatus.NotValidated, validatorName);

	/// <summary>
	///     Creates a successful validation result.
	/// </summary>
	/// <param name="validatorName">The validator that accepted the request.</param>
	/// <returns>A successful validation result.</returns>
	public static DiscordIngressSignatureValidationResult Valid(string validatorName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(validatorName);

		return new DiscordIngressSignatureValidationResult(DiscordIngressSignatureValidationStatus.Valid, validatorName);
	}

	/// <summary>
	///     Creates a failed validation result.
	/// </summary>
	/// <param name="validatorName">The validator that rejected the request.</param>
	/// <param name="failureReason">The reason that validation failed.</param>
	/// <returns>A failed validation result.</returns>
	public static DiscordIngressSignatureValidationResult Invalid(string validatorName, string? failureReason = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(validatorName);

		return new DiscordIngressSignatureValidationResult(DiscordIngressSignatureValidationStatus.Invalid, validatorName, failureReason);
	}
}
