using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.Core;
using DisCatSharp.Enums.Core;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.CommandsNext;

/// <summary>
///     Represents a context in which a command is executed.
/// </summary>
public sealed class CommandContext : DisCatSharpCommandContext
{
	private readonly Lazy<DiscordMember> _lazyMember;

	/// <summary>
	///     Initializes a new instance of the <see cref="CommandContext" /> class.
	/// </summary>
	internal CommandContext()
		: base(DisCatSharpCommandType.TextCommand)
	{
		this._lazyMember = new(() => this.Guild is not null && this.Guild.Members.TryGetValue(this.User.Id, out var member) ? member : this.Guild?.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult());
	}

	/// <summary>
	///     Gets the message that triggered the execution.
	/// </summary>
	public DiscordMessage Message { get; internal set; }

	/// <summary>
	///     Gets the channel in which the execution was triggered,
	/// </summary>
	public DiscordChannel Channel
		=> this.Message.Channel;

	/// <summary>
	///     Gets the guild in which the execution was triggered. This property is null for commands sent over direct messages.
	/// </summary>
	public DiscordGuild? Guild
		=> this.Message.GuildId.HasValue ? this.Message.Guild : null;

	/// <summary>
	///     Gets the user who triggered the execution.
	/// </summary>
	public DiscordUser User
		=> this.Message.Author;

	/// <summary>
	///     Gets the member who triggered the execution. This property is null for commands sent over direct messages.
	/// </summary>
	public DiscordMember Member
		=> this._lazyMember.Value;

	/// <summary>
	///     Gets the CommandsNext service instance that handled this command.
	/// </summary>
	public CommandsNextExtension CommandsNext { get; internal set; }

	/// <summary>
	///     Gets the service provider for this CNext instance.
	/// </summary>
	public IServiceProvider Services { get; internal set; }

	/// <summary>
	///     Gets the command that is being executed.
	/// </summary>
	public Command Command { get; internal set; }

	/// <inheritdoc />
	public override string FullCommandName
		=> this.Command.QualifiedName;
	
	/// <inheritdoc />
	public override string CommandName
		=> this.Command.Name;

	/// <summary>
	///     Gets the overload of the command that is being executed.
	/// </summary>
	public CommandOverload Overload { get; internal set; }

	/// <summary>
	///     Gets the list of raw arguments passed to the command.
	/// </summary>
	public IReadOnlyList<string> RawArguments { get; internal set; }

	/// <summary>
	///     Gets the raw string from which the arguments were extracted.
	/// </summary>
	public string RawArgumentString { get; internal set; }

	/// <summary>
	///     Gets the prefix used to invoke the command.
	/// </summary>
	public string Prefix { get; internal set; }

	/// <summary>
	///     Gets or sets the config.
	/// </summary>
	internal CommandsNextConfiguration Config { get; set; }

	/// <summary>
	///     Gets or sets the service scope context.
	/// </summary>
	internal ServiceContext ServiceScopeContext { get; set; }

	/// <summary>
	///     Quickly respond to the message that triggered the command.
	/// </summary>
	/// <param name="content">Message to respond with.</param>
	/// <returns></returns>
	public Task<DiscordMessage> RespondAsync(string content)
		=> this.Message.RespondAsync(content);

	/// <summary>
	///     Quickly respond to the message that triggered the command.
	/// </summary>
	/// <param name="embed">Embed to attach.</param>
	/// <returns></returns>
	public Task<DiscordMessage> RespondAsync(DiscordEmbed embed)
		=> this.Message.RespondAsync(embed);

	/// <summary>
	///     Quickly respond to the message that triggered the command.
	/// </summary>
	/// <param name="content">Message to respond with.</param>
	/// <param name="embed">Embed to attach.</param>
	/// <returns></returns>
	public Task<DiscordMessage> RespondAsync(string content, DiscordEmbed embed)
		=> this.Message.RespondAsync(content, embed);

	/// <summary>
	///     Quickly respond to the message that triggered the command.
	/// </summary>
	/// <param name="builder">The Discord Message builder.</param>
	/// <returns></returns>
	public Task<DiscordMessage> RespondAsync(DiscordMessageBuilder builder)
		=> this.Message.RespondAsync(builder);

	/// <summary>
	///     Quickly respond to the message that triggered the command.
	/// </summary>
	/// <param name="action">The Discord Message builder.</param>
	/// <returns></returns>
	public Task<DiscordMessage> RespondAsync(Action<DiscordMessageBuilder> action)
		=> this.Message.RespondAsync(action);

	/// <summary>
	///     Triggers typing in the channel containing the message that triggered the command.
	/// </summary>
	/// <returns></returns>
	public Task TriggerTypingAsync()
		=> this.Channel.TriggerTypingAsync();

	internal readonly struct ServiceContext : IDisposable
	{
		/// <summary>
		///     Gets the provider.
		/// </summary>
		public IServiceProvider Provider { get; }

		/// <summary>
		///     Gets the scope.
		/// </summary>
		public IServiceScope Scope { get; }

		/// <summary>
		///     Gets a value indicating whether is initialized.
		/// </summary>
		public bool IsInitialized { get; }

		/// <summary>
		///     Initializes a new instance of the <see cref="ServiceContext" /> class.
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="scope">The scope.</param>
		public ServiceContext(IServiceProvider services, IServiceScope scope)
		{
			this.Provider = services;
			this.Scope = scope;
			this.IsInitialized = true;
		}

		/// <summary>
		///     Disposes the command context.
		/// </summary>
		public void Dispose()
			=> this.Scope?.Dispose();
	}
}
