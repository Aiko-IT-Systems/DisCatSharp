using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisCatSharp.VoiceNext.Codec;

/// <summary>
/// The helpers.
/// </summary>
internal static class Helpers
{
	/// <summary>
	/// Fills the buffer with 0.
	/// </summary>
	/// <param name="buff">The buffer.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ZeroFill(Span<byte> buff)
	{
		var zero = 0;
		var i = 0;
		for (; i < buff.Length / 4; i++)
		{
#if NET8_0_OR_GREATER
			MemoryMarshal.Write(buff, in zero);
#else
			MemoryMarshal.Write(buff, ref zero);
#endif
		}

		var remainder = buff.Length % 4;
		if (remainder == 0)
			return;

		for (; i < buff.Length; i++)
			buff[i] = 0;
	}
}
