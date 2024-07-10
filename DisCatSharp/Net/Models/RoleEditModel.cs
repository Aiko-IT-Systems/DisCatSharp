using System.IO;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a role edit model.
/// </summary>
public class RoleEditModel : BaseEditModel
{
	/// <summary>
	/// New role name
	/// </summary>
	public string Name { internal get; set; }

	/// <summary>
	/// New role permissions
	/// </summary>
	public Permissions? Permissions { internal get; set; }

	/// <summary>
	/// New role color
	/// </summary>
	public DiscordColor? Color { internal get; set; }

	/// <summary>
	/// Whether new role should be hoisted (Shown in the sidebar)
	/// </summary>
	public bool? Hoist { internal get; set; }

	/// <summary>
	/// Whether new role should be mentionable
	/// </summary>
	public bool? Mentionable { internal get; set; }

	/// <summary>
	/// The new role icon.
	/// </summary>
	public Optional<Stream> Icon { internal get; set; }

	/// <summary>
	/// The new role icon from unicode emoji.
	/// </summary>
	public Optional<DiscordEmoji> UnicodeEmoji { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RoleEditModel"/> class.
	/// </summary>
	internal RoleEditModel()
	{
		this.Name = null;
		this.Permissions = null;
		this.Color = null;
		this.Hoist = null;
		this.Mentionable = null;
		this.Icon = Optional.None;
		this.UnicodeEmoji = Optional.None;
	}
}
