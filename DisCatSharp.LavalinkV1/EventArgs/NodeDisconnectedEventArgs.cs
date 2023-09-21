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
