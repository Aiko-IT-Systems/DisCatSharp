// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Text;

using DisCatSharp.Entities;

namespace DisCatSharp;

/// <summary>
/// Tool to detect image formats and convert from binary data to base64 strings.
/// </summary>
public sealed class ImageTool : IDisposable
{
	/// <summary>
	/// The png magic .
	/// </summary>
	private const ulong PNG_MAGIC = 0x0A1A_0A0D_474E_5089;
	/// <summary>
	/// The jpeg magic 1.
	/// </summary>
	private const ushort JPEG_MAGIC_1 = 0xD8FF;
	/// <summary>
	/// The jpeg magic 2.
	/// </summary>
	private const ushort JPEG_MAGIC_2 = 0xD9FF;
	/// <summary>
	/// The gif magic 1
	/// </summary>
	private const ulong GIF_MAGIC_1 = 0x0000_6139_3846_4947;
	/// <summary>
	/// The gif magic 2.
	/// </summary>
	private const ulong GIF_MAGIC_2 = 0x0000_6137_3846_4947;
	/// <summary>
	/// The webp magic 1.
	/// </summary>
	private const uint WEBP_MAGIC_1 = 0x4646_4952;
	/// <summary>
	/// The webp magic 2.
	/// </summary>
	private const uint WEBP_MAGIC_2 = 0x5042_4557;

	/// <summary>
	/// The gif mask.
	/// </summary>
	private const ulong GIF_MASK = 0x0000_FFFF_FFFF_FFFF;
	/// <summary>
	/// The mask 32.
	/// </summary>
	private const ulong MASK32 = 0x0000_0000_FFFF_FFFF;
	/// <summary>
	/// The mask 16.
	/// </summary>
	private const uint MASK16 = 0x0000_FFFF;

	/// <summary>
	/// Gets the stream this tool is operating on.
	/// </summary>
	public Stream SourceStream { get; }

	private ImageFormat _ifcache;
	private string _b64Cache;

	/// <summary>
	/// Creates a new image tool from given stream.
	/// </summary>
	/// <param name="stream">Stream to work with.</param>
	public ImageTool(Stream stream)
	{
		if (stream == null)
			throw new ArgumentNullException(nameof(stream));

		if (!stream.CanRead || !stream.CanSeek)
			throw new ArgumentException("The stream needs to be both readable and seekable.", nameof(stream));

		this.SourceStream = stream;
		this.SourceStream.Seek(0, SeekOrigin.Begin);

		this._ifcache = 0;
		this._b64Cache = null;
	}

	/// <summary>
	/// Detects the format of this image.
	/// </summary>
	/// <returns>Detected format.</returns>
	public ImageFormat GetFormat()
	{
		if (this._ifcache != ImageFormat.Unknown)
			return this._ifcache;

		using (var br = new BinaryReader(this.SourceStream, Utilities.UTF8, true))
		{
			var bgn64 = br.ReadUInt64();
			if (bgn64 == PNG_MAGIC)
				return this._ifcache = ImageFormat.Png;

			bgn64 &= GIF_MASK;
			if (bgn64 == GIF_MAGIC_1 || bgn64 == GIF_MAGIC_2)
				return this._ifcache = ImageFormat.Gif;

			var bgn32 = (uint)(bgn64 & MASK32);
			if (bgn32 == WEBP_MAGIC_1 && br.ReadUInt32() == WEBP_MAGIC_2)
				return this._ifcache = ImageFormat.WebP;

			var bgn16 = (ushort)(bgn32 & MASK16);
			if (bgn16 == JPEG_MAGIC_1)
			{
				this.SourceStream.Seek(-2, SeekOrigin.End);
				if (br.ReadUInt16() == JPEG_MAGIC_2)
					return this._ifcache = ImageFormat.Jpeg;
			}
		}

		throw new InvalidDataException("The data within the stream was not valid image data.");
	}

	/// <summary>
	/// Converts this image into base64 data format string.
	/// </summary>
	/// <returns>Data-scheme base64 string.</returns>
	public string GetBase64()
	{
		if (this._b64Cache != null)
			return this._b64Cache;

		var fmt = this.GetFormat();
		var sb = new StringBuilder();

		sb.Append("data:image/")
			.Append(fmt.ToString().ToLowerInvariant())
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
	/// Disposes this image tool.
	/// </summary>
	public void Dispose()
		=> this.SourceStream?.Dispose();

	/// <summary>
	/// Utility function to convert an image stream into a base 64 string.
	/// </summary>
	/// <param name="stream">The stream.</param>
	/// <returns>The base 64 string.</returns>
	public static string Base64FromStream(Stream stream)
	{
		using var imgtool = new ImageTool(stream);
		return imgtool.GetBase64();
	}

	/// <summary>
	/// Utility function to convert an optional image stream into an optional base 64 string.
	/// </summary>
	/// <param name="stream">The optional stream.</param>
	/// <returns>The optional base 64 string.</returns>
	public static Optional<string> Base64FromStream(Optional<Stream> stream)
	{
		if (stream.HasValue)
		{
			var val = stream.Value;
			return val != null ? Base64FromStream(val) : null;
		}

		return Optional.None;
	}
}

/// <summary>
/// Represents format of an image.
/// </summary>
public enum ImageFormat : int
{
	/// <summary>
	/// The format is unknown
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// The format is a jpeg
	/// </summary>
	Jpeg = 1,

	/// <summary>
	/// The format is a png
	/// </summary>
	Png = 2,

	/// <summary>
	/// The format is a gif
	/// </summary>
	Gif = 3,

	/// <summary>
	/// The format is a webp
	/// </summary>
	WebP = 4,

	/// <summary>
	/// The format will be automatically detected
	/// </summary>
	Auto = 5
}
