using System;
using System.IO;
using System.Text;

using DisCatSharp.Entities;

namespace DisCatSharp;

/// <summary>
///     Tool to detect media formats and convert from binary data to base64 strings.
/// </summary>
public sealed class MediaTool : IDisposable
{
	/// <summary>
	///     The PNG magic number.
	/// </summary>
	private const ulong PNG_MAGIC = 0x0A1A_0A0D_474E_5089;

	/// <summary>
	///     The first JPEG magic number.
	/// </summary>
	private const ushort JPEG_MAGIC_1 = 0xD8FF;

	/// <summary>
	///     The second JPEG magic number.
	/// </summary>
	private const ushort JPEG_MAGIC_2 = 0xD9FF;

	/// <summary>
	///     The first GIF magic number.
	/// </summary>
	private const ulong GIF_MAGIC_1 = 0x0000_6139_3846_4947;

	/// <summary>
	///     The second GIF magic number.
	/// </summary>
	private const ulong GIF_MAGIC_2 = 0x0000_6137_3846_4947;

	/// <summary>
	///     The first WEBP magic number.
	/// </summary>
	private const uint WEBP_MAGIC_1 = 0x4646_4952;

	/// <summary>
	///     The second WEBP magic number.
	/// </summary>
	private const uint WEBP_MAGIC_2 = 0x5042_4557;

	/// <summary>
	///     The GIF mask.
	/// </summary>
	private const ulong GIF_MASK = 0x0000_FFFF_FFFF_FFFF;

	/// <summary>
	///     The 32-bit mask.
	/// </summary>
	private const ulong MASK32 = 0x0000_0000_FFFF_FFFF;

	/// <summary>
	///     The 16-bit mask.
	/// </summary>
	private const uint MASK16 = 0x0000_FFFF;

	/// <summary>
	///     The MP3 magic number.
	/// </summary>
	private const uint MP3_MAGIC = 0xFFF;

	/// <summary>
	///     The OGG magic number.
	/// </summary>
	private const uint OGG_MAGIC = 0x5367_4F67; // "OggS" in ASCII

	/// <summary>
	///     The base64 cache.
	/// </summary>
	private string? _b64Cache;

	/// <summary>
	///     The media format cache.
	/// </summary>
	private MediaFormat _mfCache = MediaFormat.Unknown;

	/// <summary>
	///     Creates a new media tool from given stream.
	/// </summary>
	/// <param name="stream">Stream to work with.</param>
	public MediaTool(Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream);

		if (!stream.CanRead || !stream.CanSeek)
			throw new ArgumentException("The stream needs to be both readable and seekable.", nameof(stream));

		this.SourceStream = stream;
		this.SourceStream.Seek(0, SeekOrigin.Begin);

		this._b64Cache = null;
	}

	/// <summary>
	///     Gets the stream this tool is operating on.
	/// </summary>
	public Stream SourceStream { get; }

	/// <summary>
	///     Disposes this media tool.
	/// </summary>
	public void Dispose()
		=> this.SourceStream?.Dispose();

	/// <summary>
	///     Detects the format of this media.
	/// </summary>
	/// <returns>Detected format.</returns>
	public MediaFormat GetFormat()
	{
		if (this._mfCache is not MediaFormat.Unknown)
			return this._mfCache;

		using var br = new BinaryReader(this.SourceStream, Utilities.UTF8, true);
		var bgn64 = br.ReadUInt64();
		if (bgn64 is PNG_MAGIC)
			return this._mfCache = MediaFormat.Png;

		bgn64 &= GIF_MASK;
		if (bgn64 is GIF_MAGIC_1 or GIF_MAGIC_2)
			return this._mfCache = MediaFormat.Gif;

		var bgn32 = (uint)(bgn64 & MASK32);
		if (bgn32 is WEBP_MAGIC_1 && br.ReadUInt32() is WEBP_MAGIC_2)
			return this._mfCache = MediaFormat.WebP;

		var bgn16 = (ushort)(bgn32 & MASK16);
		if (bgn16 is JPEG_MAGIC_1)
		{
			this.SourceStream.Seek(-2, SeekOrigin.End);
			if (br.ReadUInt16() is JPEG_MAGIC_2)
				return this._mfCache = MediaFormat.Jpeg;
		}

		// Check for MP3 magic number
		if ((bgn32 & 0xFFE0) == MP3_MAGIC) // MP3 has a 11-bit sync word: 0xFFE
			return this._mfCache = MediaFormat.Mp3;

		// Check for OGG magic number
		if (bgn32 == OGG_MAGIC)
			return this._mfCache = MediaFormat.Ogg;

		throw new InvalidDataException("The data within the stream was not valid media data.");
	}

	/// <summary>
	///     Converts this media into base64 data format string.
	/// </summary>
	/// <returns>Data-scheme base64 string.</returns>
	public string GetBase64()
	{
		if (this._b64Cache != null)
			return this._b64Cache;

		var fmt = this.GetFormat();
		var sb = new StringBuilder();

		sb.Append("data:")
			.Append(fmt.ToMimeType())
			.Append(";base64,");

		this.SourceStream.Seek(0, SeekOrigin.Begin);
		var buff = new byte[this.SourceStream.Length];
		var br = 0;
		while (br < buff.Length)
			br += this.SourceStream.Read(buff, br, (int)this.SourceStream.Length - br);

		sb.Append(Convert.ToBase64String(buff));

		return this._b64Cache = sb.ToString();
	}

	/// <summary>
	///     Utility function to convert a media stream into a base 64 string.
	/// </summary>
	/// <param name="stream">The stream.</param>
	/// <returns>The base 64 string.</returns>
	public static string Base64FromStream(Stream stream)
	{
		using var mediatool = new MediaTool(stream);
		return mediatool.GetBase64();
	}

	/// <summary>
	///     Utility function to convert an optional media stream into an optional base 64 string.
	/// </summary>
	/// <param name="stream">The optional stream.</param>
	/// <returns>The optional base 64 string.</returns>
	public static Optional<string?> Base64FromStream(Optional<Stream?> stream)
	{
		if (!stream.HasValue)
			return Optional.None;

		var val = stream.Value;
		return val is not null ? Base64FromStream(val) : null;
	}
}

/// <summary>
///     Represents format of a media.
/// </summary>
public enum MediaFormat
{
	/// <summary>
	///     The format is unknown
	/// </summary>
	Unknown = 0,

	/// <summary>
	///     The format is a jpeg
	/// </summary>
	Jpeg = 1,

	/// <summary>
	///     The format is a png
	/// </summary>
	Png = 2,

	/// <summary>
	///     The format is a gif
	/// </summary>
	Gif = 3,

	/// <summary>
	///     The format is a webp
	/// </summary>
	WebP = 4,

	/// <summary>
	///     The format will be automatically detected
	/// </summary>
	Auto = 5,

	/// <summary>
	///     The format is an mp3
	/// </summary>
	Mp3 = 6,

	/// <summary>
	///     The format is an ogg
	/// </summary>
	Ogg = 7
}

/// <summary>
///     Extension methods for MediaFormatt
/// </summary>
public static class MediaFormattExtensions
{
	/// <summary>
	/// Detects the mime type.
	/// </summary>
	/// <param name="format">The media format</param>
	/// <returns>The mime type.</returns>
	/// <exception cref="InvalidOperationException"></exception>
	public static string ToMimeType(this MediaFormat format)
	{
		return format switch
		{
			MediaFormat.Jpeg => "image/jpeg",
			MediaFormat.Png => "image/png",
			MediaFormat.Gif => "image/gif",
			MediaFormat.WebP => "image/webp",
			MediaFormat.Mp3 => "audio/mpeg",
			MediaFormat.Ogg => "audio/ogg",
			_ => throw new InvalidOperationException("Unknown media format.")
		};
	}
}
