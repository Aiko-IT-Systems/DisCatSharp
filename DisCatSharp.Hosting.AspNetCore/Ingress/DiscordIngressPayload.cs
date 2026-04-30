using System;
using System.Text;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a raw ingress payload that can be reused across transport implementations.
/// </summary>
public sealed class DiscordIngressPayload
{
	private static readonly byte[] EmptyBuffer = [];

	/// <summary>
	///     Gets a reusable empty payload instance.
	/// </summary>
	public static DiscordIngressPayload Empty { get; } = new(EmptyBuffer);

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressPayload" /> class.
	/// </summary>
	/// <param name="bytes">The raw payload bytes.</param>
	public DiscordIngressPayload(ReadOnlyMemory<byte> bytes) => this.Bytes = bytes;

	/// <summary>
	///     Gets the raw payload bytes.
	/// </summary>
	public ReadOnlyMemory<byte> Bytes { get; }

	/// <summary>
	///     Gets the payload length in bytes.
	/// </summary>
	public int Length => this.Bytes.Length;

	/// <summary>
	///     Gets a value indicating whether the payload is empty.
	/// </summary>
	public bool IsEmpty => this.Bytes.IsEmpty;

	/// <summary>
	///     Creates a payload from text content.
	/// </summary>
	/// <param name="content">The text content to encode.</param>
	/// <param name="encoding">The encoding to use. UTF-8 is used when omitted.</param>
	/// <returns>A payload containing the encoded bytes.</returns>
	public static DiscordIngressPayload FromString(string content, Encoding? encoding = null)
	{
		ArgumentNullException.ThrowIfNull(content);

		return new DiscordIngressPayload((encoding ?? Encoding.UTF8).GetBytes(content));
	}

	/// <summary>
	///     Decodes the payload into a string.
	/// </summary>
	/// <param name="encoding">The encoding to use. UTF-8 is used when omitted.</param>
	/// <returns>The decoded text representation of the payload.</returns>
	public string GetString(Encoding? encoding = null) => (encoding ?? Encoding.UTF8).GetString(this.Bytes.Span);
}
