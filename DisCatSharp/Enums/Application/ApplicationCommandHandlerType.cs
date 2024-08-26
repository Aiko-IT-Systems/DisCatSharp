namespace DisCatSharp.Enums;

/// <summary>
///     Represents the application command handler types.
/// </summary>
public enum ApplicationCommandHandlerType
{
	/// <summary>
	///     The app handles the interaction using an interaction token.
	/// </summary>
	AppHandler = 1,

	/// <summary>
	///     Discord handles the interaction by launching an Activity and sending a follow-up message without coordinating with the app.
	/// </summary>
	DiscordLaunchActivity = 2
}
