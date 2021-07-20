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
using System.Collections.Generic;
using System.Text;

namespace DisCatSharp.VoiceNext
{
    internal readonly struct RawVoicePacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawVoicePacket"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="silence">If true, silence.</param>
        public RawVoicePacket(Memory<byte> bytes, int duration, bool silence)
        {
            this.Bytes = bytes;
            this.Duration = duration;
            this.Silence = silence;
            this.RentedBuffer = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawVoicePacket"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="silence">If true, silence.</param>
        /// <param name="rentedBuffer">The rented buffer.</param>
        public RawVoicePacket(Memory<byte> bytes, int duration, bool silence, byte[] rentedBuffer)
            : this(bytes, duration, silence)
        {
            this.RentedBuffer = rentedBuffer;
        }

        public readonly Memory<byte> Bytes;
        public readonly int Duration;
        public readonly bool Silence;

        public readonly byte[] RentedBuffer;
    }
}
