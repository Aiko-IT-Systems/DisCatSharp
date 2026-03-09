namespace DisCatSharp.Voice.Enums;

/// <summary>
///     Specifies whether a DAVE opcode was sent or received.
/// </summary>
public enum DaveOpcodeDirection
{
	/// <summary>
	///     The opcode was sent by this client.
	/// </summary>
	Sent = 0,

	/// <summary>
	///     The opcode was received from Discord.
	/// </summary>
	Received = 1
}
