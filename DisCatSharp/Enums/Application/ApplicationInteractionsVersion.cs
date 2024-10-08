namespace DisCatSharp.Enums;

/// <summary>
///     Represents the application interactions version.
/// </summary>
public enum ApplicationInteractionsVersion
{
	/// <summary>
	///     Only Interaction Create events are sent as documented (default)
	/// </summary>
	Version1 = 1,

	/// <summary>
	///     A selection of chosen events are sent
	/// </summary>
	Version2 = 2
}
