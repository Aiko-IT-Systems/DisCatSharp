namespace DisCatSharp.Hosting.AspNetCore.LinkedRoles;

/// <summary>
///     Configures linked-roles support built on top of the ASP.NET Core ingress package.
/// </summary>
public sealed class DiscordLinkedRolesOptions
{
	/// <summary>
	///     The default application-relative verification path used for the linked-roles verification URL.
	/// </summary>
	public const string DefaultVerificationPath = "linked-role";

	/// <summary>
	///     Gets or sets the application-relative verification path exposed to Discord in the developer portal.
	/// </summary>
	/// <remarks>
	///     The path is combined with the configured ingress route prefix and public base URL when computing the verification URL.
	/// </remarks>
	public string VerificationPath { get; set; } = DefaultVerificationPath;
}
