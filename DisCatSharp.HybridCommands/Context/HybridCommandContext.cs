// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.Entities;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.HybridCommands.Enums;

namespace DisCatSharp.HybridCommands.Context;
public class HybridCommandContext
{
	/// <summary>
	/// Do not use, required by RunTime-compiled classes.
	/// </summary>
	public HybridCommandContext(CommandContext ctx)
	{
		this.ExcutionType = HybridExecutionType.PrefixCommand;

		this.Member = ctx.Member;
		this.User = ctx.User;
		this.Guild = ctx.Guild;
		this.Channel = ctx.Channel;
		this.Client = ctx.Client;

		this.CurrentMember = ctx.Guild?.CurrentMember;
		this.CurrentUser = ctx.Client.CurrentUser;

		this.OriginalCommandContext = ctx;

		this.Prefix = ctx.Prefix;
		this.CommandName = ctx.Command.Name;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		if (ctx.Command.Parent != null)
			this.CommandName = this.CommandName.Insert(0, $"{ctx.Command.Parent.Name} ");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	/// <summary>
	/// Do not use, required by RunTime-compiled classes.
	/// </summary>
	public HybridCommandContext(InteractionContext ctx)
	{
		this.ExcutionType = HybridExecutionType.SlashCommand;

		this.Member = ctx.Member;
		this.User = ctx.User;
		this.Guild = ctx.Guild;
		this.Channel = ctx.Channel;
		this.Client = ctx.Client;

		this.CurrentMember = ctx.Guild?.CurrentMember;
		this.CurrentUser = ctx.Client.CurrentUser;

		this.OriginalInteractionContext = ctx;

		this.Prefix = "/";
		this.CommandName = ctx.FullCommandName;
		this.ParentCommandName = ctx.CommandName;
	}

	/// <summary>
	/// Do not use, required by RunTime-compiled classes.
	/// </summary>
	public HybridCommandContext(ContextMenuContext ctx)
	{
		this.ExcutionType = HybridExecutionType.ContextMenuCommand;

		this.Member = ctx.Member;
		this.User = ctx.User;
		this.Guild = ctx.Guild;
		this.Channel = ctx.Channel;
		this.Client = ctx.Client;

		this.CurrentMember = ctx.Guild?.CurrentMember;
		this.CurrentUser = ctx.Client.CurrentUser;

		this.OriginalContextMenuContext = ctx;

		this.Prefix = "";
		this.CommandName = ctx.FullCommandName;
		this.ParentCommandName = ctx.CommandName;
	}

	/// <summary>
	/// From what kind of source this command originated from.
	/// </summary>
	public HybridExecutionType ExcutionType { get; internal set; }

	/// <summary>
	/// The <see cref="DiscordClient"/> that execution originated from.
	/// </summary>
	public DiscordClient Client { get; internal set; }

	/// <summary>
	/// The prefix that was used to execute this command.
	/// </summary>
	public string Prefix { get; internal set; }

	/// <summary>
	/// The name of the command used.
	/// </summary>
	public string CommandName { get; internal set; }

	/// <summary>
	/// <para>The name of the parent command used, if command is a subcommand.</para>
	/// <para>Null if command is not subcommand.</para>
	/// </summary>
	public string? ParentCommandName { get; internal set; }

	/// <summary>
	/// The <see cref="DiscordUser"/> that executed this command.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// <para>The <see cref="DiscordMember"/> that executed this command.</para>
	/// <para>Null if command was not ran on guild.</para>
	/// </summary>
	public DiscordMember? Member { get; internal set; }

	/// <summary>
	/// The <see cref="DiscordUser"/> that the <see cref="DiscordClient"/> uses.
	/// </summary>
	public DiscordUser CurrentUser { get; internal set; }

	/// <summary>
	/// <para>The <see cref="DiscordMember"/> that the <see cref="DiscordClient"/> uses.</para>
	/// <para>Null if command was not ran on guild.</para>
	/// </summary>
	public DiscordMember? CurrentMember { get; internal set; }

	/// <summary>
	/// The channel this command was executed in.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// <para>The guild this command was executed on.</para>
	/// <para>Null if command was not ran on guild.</para>
	/// </summary>
	public DiscordGuild? Guild { get; internal set; }


	/// <summary>
	/// <para>The message that's being used to interact with the user.</para>
	/// <para>Null if RespondOrEditAsync was not used.</para>
	/// </summary>
	public DiscordMessage? ResponseMessage { get; internal set; }

	/// <summary>
	/// <para>The original <see cref="CommandContext"/>.</para>
	/// <para>Null if <see cref="ExcutionType"/> is not <see cref="HybridExecutionType.PrefixCommand"/>.</para>
	/// </summary>
	public CommandContext? OriginalCommandContext { get; internal set; }

	/// <summary>
	/// <para>The original <see cref="InteractionContext"/>.</para>
	/// <para>Null if <see cref="ExcutionType"/> is not <see cref="HybridExecutionType.SlashCommand"/>.</para>
	/// </summary>
	public InteractionContext? OriginalInteractionContext { get; internal set; }

	/// <summary>
	/// <para>The original <see cref="ContextMenuContext"/>.</para>
	/// <para>Null if <see cref="ExcutionType"/> is not <see cref="HybridExecutionType.ContextMenuCommand"/>.</para>
	/// </summary>
	public ContextMenuContext? OriginalContextMenuContext { get; internal set; }

	/// <summary>
	/// The original interaction that started this command.
	/// </summary>
	public DiscordInteraction? Interaction
		=> this.ExcutionType switch
		{
			HybridExecutionType.SlashCommand => this.OriginalInteractionContext?.Interaction,
			HybridExecutionType.ContextMenuCommand => this.OriginalContextMenuContext?.Interaction,
			_ => null
		};

	#region Methods

	/// <summary>
	/// <inheritdoc cref="RespondOrEditAsync(DiscordMessageBuilder)"/>
	/// </summary>
	/// <param name="content">The message content to send.</param>
	public async Task<DiscordMessage> RespondOrEditAsync(string content)
		=> await this.RespondOrEditAsync(new DiscordMessageBuilder().WithContent(content));

	/// <summary>
	/// <inheritdoc cref="RespondOrEditAsync(DiscordMessageBuilder)"/>
	/// </summary>
	/// <param name="embed">The <see cref="DiscordEmbed"/> to send.</param>
	public async Task<DiscordMessage> RespondOrEditAsync(DiscordEmbed embed)
		=> await this.RespondOrEditAsync(new DiscordMessageBuilder().WithEmbed(embed));

	/// <inheritdoc cref="RespondOrEditAsync(DiscordEmbed)"/>
	public async Task<DiscordMessage> RespondOrEditAsync(DiscordEmbedBuilder embed)
		=> await this.RespondOrEditAsync(new DiscordMessageBuilder().WithEmbed(embed.Build()));

	/// <summary>
	/// <para>Responds or edits the response to this command.</para>
	/// <para>Automatically adjusts how to respond or edit the response based on the <see cref="HybridExecutionType"/>.</para>
	/// </summary>
	/// <param name="discordMessageBuilder">The <see cref="DiscordMessageBuilder"/> to send.</param>
	/// <returns>The <see cref="DiscordMessage"/> that contains the reply.</returns>
	/// <exception cref="InvalidOperationException"></exception>
	public async Task<DiscordMessage> RespondOrEditAsync(DiscordMessageBuilder discordMessageBuilder)
	{
		switch (this.ExcutionType)
		{
			case HybridExecutionType.SlashCommand:
			{
				DiscordWebhookBuilder discordWebhookBuilder = new();

				var files = new Dictionary<string, Stream>();

				foreach (var b in discordMessageBuilder.Files)
					files.Add(b.FileName, b.Stream);

				discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
				discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
				discordWebhookBuilder.AddFiles(files);
				discordWebhookBuilder.Content = discordMessageBuilder.Content;

				if (this.OriginalInteractionContext is null)
					throw new InvalidOperationException("The OriginalInteractionContext is null but ExcutionType is set to SlashCommand.");

				var msg = await this.OriginalInteractionContext.EditResponseAsync(discordWebhookBuilder);
				this.ResponseMessage = msg;
				return msg;
			}

			case HybridExecutionType.ContextMenuCommand:
			{
				DiscordWebhookBuilder discordWebhookBuilder = new();

				var files = new Dictionary<string, Stream>();

				foreach (var b in discordMessageBuilder.Files)
					files.Add(b.FileName, b.Stream);

				discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
				discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
				discordWebhookBuilder.AddFiles(files);
				discordWebhookBuilder.Content = discordMessageBuilder.Content;

				if (this.OriginalContextMenuContext is null)
					throw new InvalidOperationException("The OriginalContextMenuContext is null but ExcutionType is set to SlashCommand.");

				var msg = await this.OriginalContextMenuContext.EditResponseAsync(discordWebhookBuilder);
				this.ResponseMessage = msg;
				return msg;
			}

			case HybridExecutionType.Unknown:
			case HybridExecutionType.PrefixCommand:
			{
				if (this.ResponseMessage is not null)
				{
					if (discordMessageBuilder.Files?.Any() ?? false)
					{
						await this.ResponseMessage.DeleteAsync();
						var msg1 = await this.Channel.SendMessageAsync(discordMessageBuilder);
						this.ResponseMessage = msg1;
						return this.ResponseMessage;
					}

					await this.ResponseMessage.ModifyAsync(discordMessageBuilder);
					this.ResponseMessage = await this.ResponseMessage.Channel.GetMessageAsync(this.ResponseMessage.Id, true);

					return this.ResponseMessage;
				}

				var msg = await this.Channel.SendMessageAsync(discordMessageBuilder);

				this.ResponseMessage = msg;
				return this.ResponseMessage;
			}

			default:
				throw new NotImplementedException();
		}
	}

	/// <summary>
	/// <para>Deletes the response to this command.</para>
	/// <para>Automatically adjusts how to delete the response based on the <see cref="HybridExecutionType"/>.</para>
	/// <para>Only works when RespondOrEditAsync was used.</para>
	/// </summary>
	public async Task DeleteResponseAsync()
	{
		switch (this.ExcutionType)
		{
			case HybridExecutionType.SlashCommand:
			{
				if (this.OriginalInteractionContext is null)
					throw new InvalidOperationException("The OriginalInteractionContext is null but ExcutionType is set to SlashCommand.");

				_ = this.OriginalInteractionContext.DeleteResponseAsync();
				return;
			}
			case HybridExecutionType.ContextMenuCommand:
			{
				if (this.OriginalContextMenuContext is null)
					throw new InvalidOperationException("The OriginalContextMenuContext is null but ExcutionType is set to SlashCommand.");

				await this.OriginalContextMenuContext.DeleteResponseAsync();
				return;
			}
			default:
			{
				if (this.ResponseMessage is null)
					return;

				await this.ResponseMessage.DeleteAsync();
				return;
			}
		}
	}

	#endregion Methods
}
