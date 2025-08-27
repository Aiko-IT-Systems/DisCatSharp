namespace DisCatSharp.Enums.Core;

/// <summary>
///    Represents the mode used by a <see cref="Entities.Core.DisCatSharpBuilder"/> for updating a message.
/// </summary>
public enum ModifyMode
{
	/// <summary>
	///    Denotes that the message will only update explicitly set fields, leaving others unchanged.
	/// </summary>
	Update = 0,

	/// <summary>
	///    Denotes that the entire message will be replaced as if it were a new message.
	/// </summary>
	Replace = 1
}
