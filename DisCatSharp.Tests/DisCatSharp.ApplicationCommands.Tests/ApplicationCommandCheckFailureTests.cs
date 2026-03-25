using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums.Core;

using Xunit;

namespace DisCatSharp.ApplicationCommands.Tests;

public class ApplicationCommandCheckFailureTests
{
	private sealed class AlwaysFailCheckAttribute : ApplicationCommandCheckBaseAttribute
	{
		public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
			=> Task.FromResult(false);
	}

	public sealed class TestModule : ApplicationCommandsModule
	{
		public static bool SlashExecuted { get; private set; }
		public static bool ContextMenuExecuted { get; private set; }

		public static void Reset()
		{
			SlashExecuted = false;
			ContextMenuExecuted = false;
		}

		[AlwaysFailCheck]
		public Task BlockedSlashAsync(InteractionContext ctx)
		{
			SlashExecuted = true;
			return Task.CompletedTask;
		}

		[AlwaysFailCheck]
		public Task BlockedContextMenuAsync(ContextMenuContext ctx)
		{
			ContextMenuExecuted = true;
			return Task.CompletedTask;
		}
	}

	private readonly DiscordClient _client = new(new()
	{
		Token = "1"
	});

	[Fact]
	public async Task FailingSlashChecks_AbortExecution_WithoutErrorEvent()
	{
		var extension = this._client.UseApplicationCommands(new()
		{
			UnitTestMode = true,
			EnableDefaultHelp = false
		});

		var context = new InteractionContext
		{
			Client = this._client,
			ApplicationCommandsExtension = extension,
			CommandName = "blocked"
		};

		var checksFailed = false;
		var errored = false;
		var executed = false;
		IReadOnlyList<ApplicationCommandCheckBaseAttribute> observedFailedChecks = [];

		extension.SlashCommandChecksFailed += (_, args) =>
		{
			checksFailed = true;
			observedFailedChecks = args.FailedChecks;
			Assert.Same(context, args.Context);
			return Task.CompletedTask;
		};

		extension.SlashCommandErrored += (_, _) =>
		{
			errored = true;
			return Task.CompletedTask;
		};

		extension.SlashCommandExecuted += (_, _) =>
		{
			executed = true;
			return Task.CompletedTask;
		};

		TestModule.Reset();
		var result = await extension.RunCommandAsync(context, typeof(TestModule).GetMethod(nameof(TestModule.BlockedSlashAsync), BindingFlags.Instance | BindingFlags.Public)!, [context]);

		Assert.False(result);
		Assert.True(checksFailed);
		Assert.False(errored);
		Assert.False(executed);
		Assert.False(TestModule.SlashExecuted);
		Assert.Single(observedFailedChecks);
	}

	[Fact]
	public async Task FailingContextMenuChecks_AbortExecution_WithoutErrorEvent()
	{
		var extension = this._client.UseApplicationCommands(new()
		{
			UnitTestMode = true,
			EnableDefaultHelp = false
		});

		var context = new ContextMenuContext(DisCatSharpCommandType.UserCommand)
		{
			Client = this._client,
			ApplicationCommandsExtension = extension,
			CommandName = "blocked"
		};

		var checksFailed = false;
		var errored = false;
		var executed = false;
		IReadOnlyList<ApplicationCommandCheckBaseAttribute> observedFailedChecks = [];

		extension.ContextMenuChecksFailed += (_, args) =>
		{
			checksFailed = true;
			observedFailedChecks = args.FailedChecks;
			Assert.Same(context, args.Context);
			return Task.CompletedTask;
		};

		extension.ContextMenuErrored += (_, _) =>
		{
			errored = true;
			return Task.CompletedTask;
		};

		extension.ContextMenuExecuted += (_, _) =>
		{
			executed = true;
			return Task.CompletedTask;
		};

		TestModule.Reset();
		var result = await extension.RunCommandAsync(context, typeof(TestModule).GetMethod(nameof(TestModule.BlockedContextMenuAsync), BindingFlags.Instance | BindingFlags.Public)!, [context]);

		Assert.False(result);
		Assert.True(checksFailed);
		Assert.False(errored);
		Assert.False(executed);
		Assert.False(TestModule.ContextMenuExecuted);
		Assert.Single(observedFailedChecks);
	}
}
