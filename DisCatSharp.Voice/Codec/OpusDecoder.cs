using System;

using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Interop;

namespace DisCatSharp.Voice.Codec;

/// <summary>
///     Represents an Opus decoder.
/// </summary>
public sealed class OpusDecoder : IDisposable
{
	/// <summary>
	///     Gets a value indicating whether this <see cref="OpusDecoder" /> is disposed.
	/// </summary>
	private volatile bool _isDisposed;

	/// <summary>
	///     Initializes a new instance of the <see cref="OpusDecoder" /> class.
	/// </summary>
	/// <param name="managedOpus">The managed opus.</param>
	internal OpusDecoder(Opus managedOpus)
	{
		this.Opus = managedOpus;
	}

	/// <summary>
	///     Gets the audio format produced by this decoder.
	/// </summary>
	public AudioFormat AudioFormat { get; private set; }

	/// <summary>
	///     Gets the opus.
	/// </summary>
	internal Opus Opus { get; }

	/// <summary>
	///     Gets the decoder.
	/// </summary>
	internal IntPtr Decoder { get; private set; }

	/// <summary>
	///     Disposes of this Opus decoder.
	/// </summary>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		this._isDisposed = true;
		if (this.Decoder != IntPtr.Zero)
			OpusNative.OpusDestroyDecoder(this.Decoder);

		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Used to lazily initialize the decoder to make sure we're
	///     using the correct output format, this way we don't end up
	///     creating more decoders than we need.
	/// </summary>
	/// <param name="outputFormat"></param>
	internal void Initialize(AudioFormat outputFormat)
	{
		if (this.Decoder != IntPtr.Zero)
			OpusNative.OpusDestroyDecoder(this.Decoder);

		this.AudioFormat = outputFormat;

		this.Decoder = OpusNative.OpusCreateDecoder(outputFormat);
	}

	/// <summary>
	///     Disposes of this Opus decoder.
	/// </summary>
	~OpusDecoder()
	{
		this.Dispose();
	}
}
