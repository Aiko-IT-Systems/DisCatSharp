namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Represents the severity of an ingress configuration validation issue.
/// </summary>
public enum DiscordIngressValidationSeverity
{
	/// <summary>
	///     Informational finding.
	/// </summary>
	Info,

	/// <summary>
	///     Warning that should be reviewed before going live.
	/// </summary>
	Warning,

	/// <summary>
	///     Error that is likely to break the ingress flow.
	/// </summary>
	Error
}
