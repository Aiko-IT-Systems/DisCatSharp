using System;

using DisCatSharp.EventArgs;
using DisCatSharp.Voice.Enums;

namespace DisCatSharp.Voice.EventArgs;

/// <summary>
///     Represents arguments for DAVE state-transition notifications.
/// </summary>
public sealed class DaveStateChangedEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DaveStateChangedEventArgs"/> class.
	/// </summary>
	internal DaveStateChangedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the previous DAVE state.
	/// </summary>
	public DaveConnectionState OldState { get; internal set; }

	/// <summary>
	///     Gets the new DAVE state.
	/// </summary>
	public DaveConnectionState NewState { get; internal set; }

	/// <summary>
	///     Gets the handler that triggered this transition.
	/// </summary>
	public string Handler { get; internal set; } = string.Empty;

	/// <summary>
	///     Gets a short transition reason.
	/// </summary>
	public string Reason { get; internal set; } = string.Empty;

	/// <summary>
	///     Gets the DAVE protocol version associated with this transition.
	/// </summary>
	public int ProtocolVersion { get; internal set; }

	/// <summary>
	///     Gets whether the new state is <see cref="DaveConnectionState.Active"/>.
	/// </summary>
	public bool IsActive => this.NewState == DaveConnectionState.Active;
}
