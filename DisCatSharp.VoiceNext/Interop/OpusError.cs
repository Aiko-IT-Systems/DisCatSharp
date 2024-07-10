namespace DisCatSharp.VoiceNext.Interop;

internal enum OpusError
{
	Ok = 0,
	InvalidArgument = -1,
	BufferTooSmall = -2,
	InternalError = -3,
	CorruptedStream = -4,
	RequestNotImplemented = -5,
	InvalidState = -6,
	MemoryAllocationFailed = -7
}
