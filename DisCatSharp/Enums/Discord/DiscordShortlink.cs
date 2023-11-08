namespace DisCatSharp.Enums;

/// <summary>
/// Discord short links.
/// </summary>
public static class DiscordShortlink
{
	public static readonly string Support = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/support";
	public static readonly string TrustAndSafety = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/request";
	public static readonly string Contact = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/contact";
	public static readonly string BugReport = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/bugreport";
	public static readonly string TranslationError = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/lang-feedback";
	public static readonly string Status = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/status";
	public static readonly string Terms = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/terms";
	public static readonly string Guidelines = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/guidelines";
	public static readonly string Moderation = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/moderation";
}
