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
	///     The path is combined directly with the application's public base URL. It does not inherit
	///     <see cref="DiscordAspNetCoreIngressOptions.RoutePrefix" />, which lets linked-role verification live outside the signed ingress
	///     surface when desired.
	/// </remarks>
	public string VerificationPath { get; set; } = DefaultVerificationPath;
}
