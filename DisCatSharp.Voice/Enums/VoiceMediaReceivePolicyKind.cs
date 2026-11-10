using DisCatSharp.Voice.Entities;

namespace DisCatSharp.Voice.Enums;

/// <summary>
///     Describes how a <see cref="VoiceMediaReceivePolicy" /> selects inbound media.
/// </summary>
public enum VoiceMediaReceivePolicyKind
{
	/// <summary>
	///     Receives media from every remote SSRC.
	/// </summary>
	ReceiveAll,

	/// <summary>
	///     Receives no remote media.
	/// </summary>
	ReceiveNone,

	/// <summary>
	///     Receives media only from the specified Discord users.
	/// </summary>
	OnlyUsers,

	/// <summary>
	///     Receives media from every remote user except the specified Discord users.
	/// </summary>
	ExceptUsers
}
