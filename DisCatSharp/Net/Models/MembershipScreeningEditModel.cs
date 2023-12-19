using DisCatSharp.Entities;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a membership screening edit model.
/// </summary>
public class MembershipScreeningEditModel : BaseEditModel
{
	/// <summary>
	/// Sets whether membership screening should be enabled for this guild
	/// </summary>
	public Optional<bool> Enabled { internal get; set; }

	/// <summary>
	/// Sets the server description shown in the membership screening form
	/// </summary>
	public Optional<string> Description { internal get; set; }

	/// <summary>
	/// Sets the fields in this membership screening form
	/// </summary>
	public Optional<DiscordGuildMembershipScreeningField[]> Fields { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MembershipScreeningEditModel"/> class.
	/// </summary>
	internal MembershipScreeningEditModel()
	{ }
}
