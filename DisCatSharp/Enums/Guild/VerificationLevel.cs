namespace DisCatSharp.Enums;

/// <summary>
/// Represents guild verification level.
/// </summary>
public enum VerificationLevel
{
	/// <summary>
	/// No verification. Anyone can join and chat right away.
	/// </summary>
	None = 0,

	/// <summary>
	/// Low verification level. Users are required to have a verified email attached to their account in order to be able to chat.
	/// </summary>
	Low = 1,

	/// <summary>
	/// Medium verification level. Users are required to have a verified email attached to their account, and account age need to be at least 5 minutes in order to be able to chat.
	/// </summary>
	Medium = 2,

	/// <summary>
	/// High verification level. Users are required to have a verified email attached to their account, account age need to be at least 5 minutes, and they need to be in the server for at least 10 minutes in order to be able to chat.
	/// </summary>
	High = 3,

	/// <summary>
	/// Highest verification level. Users are required to have a verified phone number attached to their account.
	/// </summary>
	Highest = 4
}
