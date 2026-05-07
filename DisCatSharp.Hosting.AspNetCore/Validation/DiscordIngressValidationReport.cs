using System;
using System.Collections.Generic;
using System.Linq;

namespace DisCatSharp.Hosting.AspNetCore.Validation;

/// <summary>
///     Contains the computed ingress URLs and validation findings for a configuration snapshot.
/// </summary>
public sealed class DiscordIngressValidationReport
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressValidationReport" /> class.
	/// </summary>
	/// <param name="publicUrls">The computed public URLs, if the public base URL could be resolved.</param>
	/// <param name="issues">The collected validation issues.</param>
	public DiscordIngressValidationReport(DiscordIngressPublicUrls? publicUrls, IEnumerable<DiscordIngressValidationIssue> issues)
	{
		ArgumentNullException.ThrowIfNull(issues);

		this.PublicUrls = publicUrls;
		this.Issues = [.. issues];
	}

	/// <summary>
	///     Gets the computed public URLs, if available.
	/// </summary>
	public DiscordIngressPublicUrls? PublicUrls { get; }

	/// <summary>
	///     Gets the collected validation issues.
	/// </summary>
	public IReadOnlyList<DiscordIngressValidationIssue> Issues { get; }

	/// <summary>
	///     Gets a value indicating whether the configuration has no errors.
	/// </summary>
	public bool IsValid
		=> !this.Issues.Any(static issue => issue.Severity is DiscordIngressValidationSeverity.Error);

	/// <summary>
	///     Gets a value indicating whether the configuration produced any warnings.
	/// </summary>
	public bool HasWarnings
		=> this.Issues.Any(static issue => issue.Severity is DiscordIngressValidationSeverity.Warning);
}
