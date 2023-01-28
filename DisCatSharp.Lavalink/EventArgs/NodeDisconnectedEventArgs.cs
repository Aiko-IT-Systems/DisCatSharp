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

using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for Lavalink node disconnection.
/// </summary>
public sealed class NodeDisconnectedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the node that was disconnected.
	/// </summary>
	public LavalinkNodeConnection LavalinkNode { get; }

	/// <summary>
	/// Gets whether disconnect was clean.
	/// </summary>
	public bool IsCleanClose { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NodeDisconnectedEventArgs"/> class.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <param name="isClean">If true, is clean.</param>
	internal NodeDisconnectedEventArgs(LavalinkNodeConnection node, bool isClean) : base(node.Discord.ServiceProvider)
	{
		this.LavalinkNode = node;
		this.IsCleanClose = isClean;
	}
}
