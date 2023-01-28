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

using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

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
	/// <param name="code">The code.</param>
	/// <param name="reason">The reason.</param>
	/// <param name="remote">If true, remote.</param>
	/// <param name="provider">Service provider.</param>
	internal WebSocketCloseEventArgs(int code, string reason, bool remote, IServiceProvider provider) : base(provider)
	{
		this.Code = code;
		this.Reason = reason;
		this.Remote = remote;
	}
}
