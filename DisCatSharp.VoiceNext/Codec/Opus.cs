// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Collections.Generic;

namespace DisCatSharp.VoiceNext.Codec
{
    /// <summary>
    /// The opus.
    /// </summary>
    internal sealed class Opus : IDisposable
    {
        /// <summary>
        /// Gets the audio format.
        /// </summary>
        public AudioFormat AudioFormat { get; }

        /// <summary>
        /// Gets the encoder.
        /// </summary>
        private IntPtr Encoder { get; }

        /// <summary>
        /// Gets the managed decoders.
        /// </summary>
        private List<OpusDecoder> ManagedDecoders { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opus"/> class.
        /// </summary>
        /// <param name="AudioFormat">The audio format.</param>
        public Opus(AudioFormat AudioFormat)
        {
            if (!AudioFormat.IsValid())
                throw new ArgumentException("Invalid audio format specified.", nameof(AudioFormat));

            this.AudioFormat = AudioFormat;
            this.Encoder = Interop.OpusCreateEncoder(this.AudioFormat);

            // Set appropriate encoder options
            var sig = OpusSignal.Auto;
            switch (this.AudioFormat.VoiceApplication)
            {
                case VoiceApplication.Music:
                    sig = OpusSignal.Music;
                    break;

                case VoiceApplication.Voice:
                    sig = OpusSignal.Voice;
                    break;
            }
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetSignal, (int)sig);
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetPacketLossPercent, 15);
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetInBandFec, 1);
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetBitrate, 131072);

            this.ManagedDecoders = new List<OpusDecoder>();
        }

        /// <summary>
        /// Encodes the Opus.
        /// </summary>
        /// <param name="Pcm">The pcm.</param>
        /// <param name="Target">The target.</param>
        public void Encode(ReadOnlySpan<byte> Pcm, ref Span<byte> Target)
        {
            if (Pcm.Length != Target.Length)
                throw new ArgumentException("PCM and Opus buffer lengths need to be equal.", nameof(Target));

            var duration = this.AudioFormat.CalculateSampleDuration(Pcm.Length);
            var frameSize = this.AudioFormat.CalculateFrameSize(duration);
            var sampleSize = this.AudioFormat.CalculateSampleSize(duration);

            if (Pcm.Length != sampleSize)
                throw new ArgumentException("Invalid PCM sample size.", nameof(Target));

            Interop.OpusEncode(this.Encoder, Pcm, frameSize, ref Target);
        }

        /// <summary>
        /// Decodes the Opus.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <param name="Opus">The opus.</param>
        /// <param name="Target">The target.</param>
        /// <param name="UseFec">If true, use fec.</param>
        /// <param name="OutputFormat">The output format.</param>
        public void Decode(OpusDecoder Decoder, ReadOnlySpan<byte> Opus, ref Span<byte> Target, bool UseFec, out AudioFormat OutputFormat)
        {
            //if (target.Length != this.AudioFormat.CalculateMaximumFrameSize())
            //    throw new ArgumentException("PCM target buffer size needs to be equal to maximum buffer size for specified audio format.", nameof(target));

            Interop.OpusGetPacketMetrics(Opus, this.AudioFormat.SampleRate, out var channels, out var frames, out var samplesPerFrame, out var frameSize);
            OutputFormat = this.AudioFormat.ChannelCount != channels ? new AudioFormat(this.AudioFormat.SampleRate, channels, this.AudioFormat.VoiceApplication) : this.AudioFormat;

            if (Decoder.AudioFormat.ChannelCount != channels)
                Decoder.Initialize(OutputFormat);

            var sampleCount = Interop.OpusDecode(Decoder.Decoder, Opus, frameSize, Target, UseFec);

            var sampleSize = OutputFormat.SampleCountToSampleSize(sampleCount);
            Target = Target[..sampleSize];
        }

        /// <summary>
        /// Processes the packet loss.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <param name="FrameSize">The frame size.</param>
        /// <param name="Target">The target.</param>
        public void ProcessPacketLoss(OpusDecoder Decoder, int FrameSize, ref Span<byte> Target) => Interop.OpusDecode(Decoder.Decoder, FrameSize, Target);

        /// <summary>
        /// Gets the last packet sample count.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <returns>An int.</returns>
        public int GetLastPacketSampleCount(OpusDecoder Decoder)
        {
            Interop.OpusGetLastPacketDuration(Decoder.Decoder, out var sampleCount);
            return sampleCount;
        }

        /// <summary>
        /// Creates the decoder.
        /// </summary>
        /// <returns>An OpusDecoder.</returns>
        public OpusDecoder CreateDecoder()
        {
            lock (this.ManagedDecoders)
            {
                var managedDecoder = new OpusDecoder(this);
                this.ManagedDecoders.Add(managedDecoder);
                return managedDecoder;
            }
        }

        /// <summary>
        /// Destroys the decoder.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        public void DestroyDecoder(OpusDecoder Decoder)
        {
            lock (this.ManagedDecoders)
            {
                if (!this.ManagedDecoders.Contains(Decoder))
                    return;

                this.ManagedDecoders.Remove(Decoder);
                Decoder.Dispose();
            }
        }

        /// <summary>
        /// Disposes the Opus.
        /// </summary>
        public void Dispose()
        {
            Interop.OpusDestroyEncoder(this.Encoder);

            lock (this.ManagedDecoders)
            {
                foreach (var decoder in this.ManagedDecoders)
                    decoder.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents an Opus decoder.
    /// </summary>
    public class OpusDecoder : IDisposable
    {
        /// <summary>
        /// Gets the audio format produced by this decoder.
        /// </summary>
        public AudioFormat AudioFormat { get; private set; }

        /// <summary>
        /// Gets the opus.
        /// </summary>
        internal Opus Opus { get; }
        /// <summary>
        /// Gets the decoder.
        /// </summary>
        internal IntPtr Decoder { get; private set; }

        private volatile bool _isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpusDecoder"/> class.
        /// </summary>
        /// <param name="ManagedOpus">The managed opus.</param>
        internal OpusDecoder(Opus ManagedOpus)
        {
            this.Opus = ManagedOpus;
        }

        /// <summary>
        /// Used to lazily initialize the decoder to make sure we're
        /// using the correct output format, this way we don't end up
        /// creating more decoders than we need.
        /// </summary>
        /// <param name="OutputFormat"></param>
        internal void Initialize(AudioFormat OutputFormat)
        {
            if (this.Decoder != IntPtr.Zero)
                Interop.OpusDestroyDecoder(this.Decoder);

            this.AudioFormat = OutputFormat;

            this.Decoder = Interop.OpusCreateDecoder(OutputFormat);
        }

        /// <summary>
        /// Disposes of this Opus decoder.
        /// </summary>
        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            if (this.Decoder != IntPtr.Zero)
                Interop.OpusDestroyDecoder(this.Decoder);
        }
    }

    /// <summary>
    /// The opus error.
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

    /// <summary>
    /// The opus control.
    /// </summary>
    internal enum OpusControl : int
    {
        SetBitrate = 4002,
        SetBandwidth = 4008,
        SetInBandFec = 4012,
        SetPacketLossPercent = 4014,
        SetSignal = 4024,
        ResetState = 4028,
        GetLastPacketDuration = 4039
    }

    /// <summary>
    /// The opus signal.
    /// </summary>
    internal enum OpusSignal : int
    {
        Auto = -1000,
        Voice = 3001,
        Music = 3002,
    }
}
