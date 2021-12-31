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
using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="LavalinkGuildConnection.DiscordWebSocketClosed"/> event.
    /// </summary>
    public sealed class WebSocketCloseEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the WebSocket close code.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// Gets the WebSocket close reason.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets whether the termination was initiated by the remote party (i.e. Discord).
        /// </summary>
        public bool Remote { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCloseEventArgs"/> class.
        /// </summary>
        /// <param name="Code">The code.</param>
        /// <param name="Reason">The reason.</param>
        /// <param name="Remote">If true, remote.</param>
        /// <param name="Provider">Service provider.</param>
        internal WebSocketCloseEventArgs(int Code, string Reason, bool Remote, IServiceProvider Provider) : base(Provider)
        {
            this.Code = Code;
            this.Reason = Reason;
            this.Remote = Remote;
        }
    }
}
