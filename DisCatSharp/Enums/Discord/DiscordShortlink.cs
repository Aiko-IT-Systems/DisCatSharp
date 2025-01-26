using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
///     Discord short links.
/// </summary>
public static class DiscordShortlink
{
	/// <summary>
	/// Gets the shortlink to the support site.
	/// </summary>
	public static readonly string Support = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/support";

	/// <summary>
	/// Gets the shortlink to the support site for trust and safety.
	/// </summary>
	[DiscordDeprecated]
	public static readonly string TrustAndSafety = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/request";

	/// <summary>
	/// Gets the shortlink to the contact form.
	/// </summary>
	public static readonly string Contact = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/contact";


	/// <summary>
	/// Gets the shortlink to the bug report form.
	/// </summary>
	public static readonly string BugReport = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/bugreport";

	/// <summary>
	/// Gets the shortlink to the translation error form.
	/// </summary>
	public static readonly string TranslationError = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/lang-feedback";

	/// <summary>
	/// Gets the shortlink to the stauts site.
	/// </summary>
	public static readonly string Status = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/status";

	/// <summary>
	/// Gets the shortlink to the terms of service.
	/// </summary>
	public static readonly string Terms = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/terms";

	/// <summary>
	/// Gets the shortlink to the guidelines.
	/// </summary>
	public static readonly string Guidelines = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/guidelines";

	/// <summary>
	/// Gets the shortlinkt to DMA.
	/// </summary>
	public static readonly string Moderation = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/moderation";
}
