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

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Contains well-defined event IDs used by the Lavalink extension.
/// </summary>
public static class LavalinkEvents
{
	/// <summary>
	/// Miscellaneous events, that do not fit in any other category.
	/// </summary>
	public static EventId Misc { get; } = new(400, "Lavalink");

	/// <summary>
	/// Events pertaining to Lavalink node connection errors.
	/// </summary>
	public static EventId LavalinkConnectionError { get; } = new(401, nameof(LavalinkConnectionError));

	/// <summary>
	/// Events emitted for clean disconnects from Lavalink.
	/// </summary>
	public static EventId LavalinkSessionConnectionClosed { get; } = new(402, nameof(LavalinkSessionConnectionClosed));

	/// <summary>
	/// Events emitted for successful connections made to Lavalink.
	/// </summary>
	public static EventId LavalinkSessionConnected { get; } = new(403, nameof(LavalinkSessionConnected));

	/// <summary>
	/// Events emitted when the Lavalink REST API responds with an error.
	/// </summary>
	public static EventId LavalinkRestError { get; } = new(404, nameof(LavalinkRestError));

	/// <summary>
	/// Events containing raw payloads, received from Lavalink nodes.
	/// </summary>
	public static EventId LavalinkWsRx { get; } = new(405, "Lavalink ↓");

	/// <summary>
	/// Events emitted when the Lavalink WebSocket connection sends invalid data.
	/// </summary>
	public static EventId LavalinkWsException { get; } = new(407, nameof(LavalinkWsException));

	/// <summary>
	/// Events pertaining to Gateway Intents. Typically diagnostic information.
	/// </summary>
	public static EventId Intents { get; } = new(499, nameof(Intents));
}
