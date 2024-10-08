using System;
using System.Threading.Tasks;

using DisCatSharp.Enums;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
///     Defines that usage of this command is restricted to members with specified permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireUserPermissionsAttribute : CheckBaseAttribute
{
	/// <summary>
	///     Defines that usage of this command is restricted to members with specified permissions.
	/// </summary>
	/// <param name="permissions">Permissions required to execute this command.</param>
	/// <param name="ignoreDms">
	///     Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false
	///     means that it will always fail.
	/// </param>
	public RequireUserPermissionsAttribute(Permissions permissions, bool ignoreDms = true)
	{
		this.Permissions = permissions;
		this.IgnoreDms = ignoreDms;
	}

	/// <summary>
	///     Gets the permissions required by this attribute.
	/// </summary>
	public Permissions Permissions { get; }

	/// <summary>
	///     Gets or sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that
	///     it will always fail.
	/// </summary>
	public bool IgnoreDms { get; } = true;

	/// <summary>
	///     Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		if (ctx.Guild == null)
			return Task.FromResult(this.IgnoreDms);

		var usr = ctx.Member;
		if (usr == null)
			return Task.FromResult(false);

		if (usr.Id == ctx.Guild.OwnerId)
			return Task.FromResult(true);

		var pusr = ctx.Channel.PermissionsFor(usr);

		if ((pusr & Permissions.Administrator) != 0)
			return Task.FromResult(true);

		return (pusr & this.Permissions) == this.Permissions ? Task.FromResult(true) : Task.FromResult(false);
	}
}
