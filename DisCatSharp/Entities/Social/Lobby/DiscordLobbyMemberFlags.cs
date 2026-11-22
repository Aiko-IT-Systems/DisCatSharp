using System;

namespace DisCatSharp.Entities;

/// <summary>Represents flags granted to a lobby member.</summary>
[Flags]
public enum DiscordLobbyMemberFlags
{
	/// <summary>No lobby permissions.</summary>
	None = 0,
	/// <summary>The member may link a guild channel to the lobby.</summary>
	CanLinkLobby = 1 << 0
}
