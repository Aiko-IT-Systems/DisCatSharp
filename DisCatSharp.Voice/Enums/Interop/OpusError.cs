using System;

namespace DisCatSharp.Voice.Enums.Interop;

/// <summary>
///     The opus error.
/// </summary>
[Flags]
internal enum OpusError
{
	Ok = 0,
	BadArgument = -1,
	BufferTooSmall = -2,
	InternalError = -3,
	InvalidPacket = -4,
	Unimplemented = -5,
	InvalidState = -6,
	AllocationFailure = -7
}
