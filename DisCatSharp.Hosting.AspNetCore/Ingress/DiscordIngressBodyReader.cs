using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Default implementation that buffers an ingress request body for downstream processing.
/// </summary>
public sealed class DiscordIngressBodyReader : IDiscordIngressBodyReader
{
	private readonly DiscordWebIngressOptions _options;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressBodyReader" /> class.
	/// </summary>
	/// <param name="options">The ingress options.</param>
	public DiscordIngressBodyReader(IOptions<DiscordWebIngressOptions> options)
	{
		ArgumentNullException.ThrowIfNull(options);

		this._options = options.Value;
	}

	/// <inheritdoc />
	public async ValueTask<DiscordIngressPayload> ReadAsync(Stream body, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(body);

		if (!body.CanRead)
			throw new InvalidOperationException("The ingress request body stream must be readable.");

		if (body.CanSeek)
		{
			if (body.Length == 0)
				return DiscordIngressPayload.Empty;

			if (body.Length > this._options.MaxRequestBodySize)
				throw new InvalidOperationException($"The ingress request body exceeded the configured limit of {this._options.MaxRequestBodySize} bytes.");
		}

		using MemoryStream buffer = new();
		var chunkSize = Math.Max(1, Math.Min(this._options.MaxRequestBodySize, 81920));
		var rentedBuffer = ArrayPool<byte>.Shared.Rent(chunkSize);
		var totalRead = 0;

		try
		{
			while (true)
			{
				var read = await body.ReadAsync(rentedBuffer.AsMemory(0, chunkSize), cancellationToken);
				if (read == 0)
					break;

				totalRead += read;
				if (totalRead > this._options.MaxRequestBodySize)
					throw new InvalidOperationException($"The ingress request body exceeded the configured limit of {this._options.MaxRequestBodySize} bytes.");

				await buffer.WriteAsync(rentedBuffer.AsMemory(0, read), cancellationToken);
			}

			return buffer.Length == 0
				? DiscordIngressPayload.Empty
				: new DiscordIngressPayload(buffer.ToArray());
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rentedBuffer);
		}
	}
}
