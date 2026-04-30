using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Reads raw request bodies for Discord ingress handlers.
/// </summary>
public interface IDiscordIngressBodyReader
{
	/// <summary>
	///     Reads the full raw request body into an ingress payload.
	/// </summary>
	/// <param name="body">The body stream to read.</param>
	/// <param name="cancellationToken">A token used to cancel the read operation.</param>
	/// <returns>The fully buffered payload.</returns>
	ValueTask<DiscordIngressPayload> ReadAsync(Stream body, CancellationToken cancellationToken = default);
}
