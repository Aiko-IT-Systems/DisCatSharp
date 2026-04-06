using System.Collections.Generic;
using System.Threading;

namespace DisCatSharp.Voice.Interfaces;

/// <summary>
///     Provides a stream of pre-encoded Opus frames from an external audio source.
/// </summary>
/// <remarks>
///     Implementations produce Opus frames that bypass the internal Opus encoder in
///     <see cref="VoiceConnection" />, while still going through RTP framing,
///     DAVE E2EE encryption, and AEAD transport encryption.
/// </remarks>
public interface IExternalOpusSource
{
	/// <summary>
	///     Asynchronously reads Opus frames from the external source.
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the read operation.</param>
	/// <returns>An async enumerable of <see cref="Entities.ExternalOpusFrame" /> instances.</returns>
	IAsyncEnumerable<Entities.ExternalOpusFrame> ReadFramesAsync(CancellationToken cancellationToken = default);
}
