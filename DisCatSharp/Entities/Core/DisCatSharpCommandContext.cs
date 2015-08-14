using DisCatSharp.Enums.Core;

namespace DisCatSharp.Entities.Core;

/// <summary>
/// Interface for various command types like slash commands, user commands, message commands, text commands, etc.
/// </summary>
public class DisCatSharpCommandContext
{
	/// <summary>
	/// Gets the client.
	/// </summary>
	public DiscordClient Client { get; internal init; }

	/// <summary>
	/// Gets the id of the user who executes this command.
	/// </summary>
	public ulong UserId { get; internal set; }

	/// <summary>
	/// Gets the id of the channel this command gets executed in.
	/// </summary>
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the id of the guild this command gets executed in.
	/// </summary>
	public ulong? GuildId { get; internal set; }

	/// <summary>
	/// Gets the id of the member who executes this command.
	/// </summary>
	public ulong? MemberId { get; internal set; }

	/// <summary>
	/// Gets the id of the command.
	/// </summary>
	public ulong? CommandId { get; internal set; }

	/// <summary>
	/// Gets the name of the command.
	/// </summary>
	public string CommandName { get; internal set; } = string.Empty;

	/// <summary>
	/// Gets the name of the sub command.
	/// </summary>
	public string? SubCommandName { get; internal set; }

	/// <summary>
	/// Gets the name of the sub command within a sub group.
	/// </summary>
	public string? SubSubCommandName { get; internal set; }

	/// <summary>
	/// Gets the fully qualified name of the command.
	/// </summary>
	public virtual string FullCommandName { get; internal set; } = string.Empty;

	/// <summary>
	/// Gets the type of the command.
	/// </summary>
	public DisCatSharpCommandType CommandType { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DisCatSharpCommandContext"/> class.
	/// </summary>
	/// <param name="type">The command type.</param>
	internal DisCatSharpCommandContext(DisCatSharpCommandType type)
	{
		this.CommandType = type;
	}
}
