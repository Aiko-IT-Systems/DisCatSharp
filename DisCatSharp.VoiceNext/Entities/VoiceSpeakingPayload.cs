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

using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities
{
    /// <summary>
    /// The voice speaking payload.
    /// </summary>
    internal sealed class VoiceSpeakingPayload
    {
        /// <summary>
        /// Gets or sets a value indicating whether speaking.
        /// </summary>
        [JsonProperty("speaking")]
        public bool Speaking { get; set; }

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        [JsonProperty("delay", NullValueHandling = NullValueHandling.Ignore)]
        public int? Delay { get; set; }

        /// <summary>
        /// Gets or sets the s s r c.
        /// </summary>
        [JsonProperty("ssrc", NullValueHandling = NullValueHandling.Ignore)]
        public uint? SSRC { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? UserId { get; set; }
    }
}
