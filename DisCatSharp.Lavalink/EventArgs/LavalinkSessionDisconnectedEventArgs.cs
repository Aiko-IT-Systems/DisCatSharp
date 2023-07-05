// This file is part of the DisCatSharp project.
//
// Copyright (c) 2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink session disconnection.
/// </summary>
public sealed class LavalinkSessionDisconnectedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the session that was disconnected.
	/// </summary>
	public LavalinkSession Session { get; }

	/// <summary>
	/// Gets whether disconnect was clean.
	/// </summary>
	public bool IsCleanClose { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkSessionDisconnectedEventArgs"/> class.
	/// </summary>
	/// <param name="session">The session.</param>
	/// <param name="isClean">If true, is clean.</param>
	internal LavalinkSessionDisconnectedEventArgs(LavalinkSession session, bool isClean)
		: base(session.Discord.ServiceProvider)
	{
		this.Discord = session.Discord;
		this.Session = session;
		this.IsCleanClose = isClean;
	}
}
