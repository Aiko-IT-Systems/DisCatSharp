using System;

using DisCatSharp.EventArgs;

namespace DisCatSharp.Voice.EventArgs;

/// <summary>
///     Represents arguments for observed DAVE opcode traffic.
/// </summary>
public sealed class DaveOpcodeEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DaveOpcodeEventArgs"/> class.
	/// </summary>
	internal DaveOpcodeEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the DAVE opcode number.
	/// </summary>
	public byte Opcode { get; internal set; }

	/// <summary>
	///     Gets whether the opcode was sent or received.
	/// </summary>
	public DaveOpcodeDirection Direction { get; internal set; }

	/// <summary>
	///     Gets the payload length in bytes.
	/// </summary>
	public int PayloadLength { get; internal set; }

	/// <summary>
	///     Gets the gateway sequence, when available for inbound binary opcodes.
	/// </summary>
	public ushort? Sequence { get; internal set; }

	/// <summary>
	///     Gets whether this opcode was carried via binary voice-gateway framing.
	/// </summary>
	public bool IsBinary { get; internal set; }
}
