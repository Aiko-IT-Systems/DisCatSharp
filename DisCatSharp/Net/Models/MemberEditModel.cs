using System.Collections.Generic;
using System.IO;

using DisCatSharp.Entities;

namespace DisCatSharp.Net.Models;

/// <summary>
///     Represents a member edit model.
/// </summary>
public class MemberEditModel : BaseEditModel
{
	/// <summary>
	///     Initializes a new instance of the <see cref="MemberEditModel" /> class.
	/// </summary>
	internal MemberEditModel()
	{ }

	/// <summary>
	///     New nickname
	/// </summary>
	public Optional<string> Nickname { internal get; set; }

	/// <summary>
	///     New roles
	/// </summary>
	public Optional<List<DiscordRole>> Roles { internal get; set; }

	/// <summary>
	///     Whether this user should be muted
	/// </summary>
	public Optional<bool> Muted { internal get; set; }

	/// <summary>
	///     Whether this user should be deafened
	/// </summary>
	public Optional<bool> Deafened { internal get; set; }

	/// <summary>
	///     Voice channel to move this user to, set to null to kick
	/// </summary>
	public Optional<DiscordChannel> VoiceChannel { internal get; set; }
}

/// <summary>
///     Represents a current guild member edit model.
/// </summary>
public class CurrentMemberEditModel : BaseEditModel
{
	/// <summary>
	///     Initializes a new instance of the <see cref="CurrentMemberEditModel" /> class.
	/// </summary>
	internal CurrentMemberEditModel()
	{ }

	/// <summary>
	///     The new nickname.
	/// </summary>
	public Optional<string?> Nickname { internal get; set; }

	/// <summary>
	///     The new bio.
	/// </summary>
	public Optional<string?> Bio { internal get; set; }

	/// <summary>
	///     The new role avatar.
	/// </summary>
	public Optional<Stream?> Avatar { internal get; set; }

	/// <summary>
	///     The new role banner.
	/// </summary>
	public Optional<Stream?> Banner { internal get; set; }
}
