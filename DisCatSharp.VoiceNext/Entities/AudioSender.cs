// This file is part of the DisCatSharp project.
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
using DisCatSharp.Entities;
using DisCatSharp.VoiceNext.Codec;

namespace DisCatSharp.VoiceNext.Entities
{
    /// <summary>
    /// The audio sender.
    /// </summary>
    internal class AudioSender : IDisposable
    {
        /// <summary>
        /// Gets the s s r c.
        /// </summary>
        public uint SSRC { get; }
        /// <summary>
        /// Gets the id.
        /// </summary>
        public ulong Id => this.User?.Id ?? 0;
        /// <summary>
        /// Gets the decoder.
        /// </summary>
        public OpusDecoder Decoder { get; }
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public DiscordUser User { get; set; } = null;
        /// <summary>
        /// Gets or sets the last sequence.
        /// </summary>
        public ushort LastSequence { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioSender"/> class.
        /// </summary>
        /// <param name="ssrc">The ssrc.</param>
        /// <param name="decoder">The decoder.</param>
        public AudioSender(uint ssrc, OpusDecoder decoder)
        {
            this.SSRC = ssrc;
            this.Decoder = decoder;
        }

        /// <summary>
        /// Disposes .
        /// </summary>
        public void Dispose() => this.Decoder?.Dispose();
    }
}
