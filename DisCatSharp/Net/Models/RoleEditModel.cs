using System.IO;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
///     Represents a role edit model.
/// </summary>
public sealed class RoleEditModel : BaseEditModel
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RoleEditModel" /> class.
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
		this.Colors = null;
	}

	/// <summary>
	///     The new role name.
	/// </summary>
	public string Name { internal get; set; }

	/// <summary>
	///     The new role permissions.
	/// </summary>
	public Permissions? Permissions { internal get; set; }

	/// <summary>
	///     The new role color.
	/// </summary>
	public DiscordColor? Color { internal get; set; }

	/// <summary>
	///     The new role colors.
	/// </summary>
	[DiscordInExperiment("This feature is currently in experiment and not available for most guilds."), RequiresFeature(Features.Experiment, "Requires  2025-02_skill_trees")]
	public DiscordRoleColors? Colors { internal get; set; }

	/// <summary>
	///     Whether new role should be hoisted (Shown in the sidebar)
	/// </summary>
	public bool? Hoist { internal get; set; }

	/// <summary>
	///     Whether new role should be mentionable.
	/// </summary>
	public bool? Mentionable { internal get; set; }

	/// <summary>
	///     The new role icon.
	/// </summary>
	public Optional<Stream> Icon { internal get; set; }

	/// <summary>
	///     The new role icon from unicode emoji.
	/// </summary>
	public Optional<DiscordEmoji> UnicodeEmoji { internal get; set; }
}
