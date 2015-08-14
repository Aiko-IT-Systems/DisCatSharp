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
