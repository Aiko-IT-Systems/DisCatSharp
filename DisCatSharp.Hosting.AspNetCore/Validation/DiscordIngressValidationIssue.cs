using System;

namespace DisCatSharp.Hosting.AspNetCore.Validation;

/// <summary>
///     Represents a single ingress configuration validation finding.
/// </summary>
public sealed class DiscordIngressValidationIssue
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressValidationIssue" /> class.
	/// </summary>
	/// <param name="code">A stable code for the finding.</param>
	/// <param name="severity">The issue severity.</param>
	/// <param name="message">The human-readable issue message.</param>
	public DiscordIngressValidationIssue(string code, DiscordIngressValidationSeverity severity, string message)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ArgumentException.ThrowIfNullOrWhiteSpace(message);

		this.Code = code;
		this.Severity = severity;
		this.Message = message;
	}

	/// <summary>
	///     Gets the stable issue code.
	/// </summary>
	public string Code { get; }

	/// <summary>
	///     Gets the issue severity.
	/// </summary>
	public DiscordIngressValidationSeverity Severity { get; }

	/// <summary>
	///     Gets the human-readable issue message.
	/// </summary>
	public string Message { get; }
}
