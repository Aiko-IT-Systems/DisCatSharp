using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Enums.Core;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace DisCatSharp.Copilot.Tests;

public class ApplicationCommandChecksEventArgsRegressionTests
{
	private sealed class TestCheckAttribute : ApplicationCommandCheckBaseAttribute
	{
		public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
			=> Task.FromResult(false);
	}

	[Fact]
	public void SlashCommandChecksFailedEventArgs_ExposeReadonlyFailedChecks()
	{
		var context = new InteractionContext();
		ApplicationCommandCheckBaseAttribute[] failedChecks = [new TestCheckAttribute()];

		var args = new SlashCommandChecksFailedEventArgs(new ServiceCollection().BuildServiceProvider())
		{
			Context = context,
			FailedChecks = new ReadOnlyCollection<ApplicationCommandCheckBaseAttribute>([.. failedChecks])
		};

		Assert.Same(context, args.Context);
		Assert.Same(failedChecks[0], Assert.Single(args.FailedChecks));
		Assert.Throws<NotSupportedException>(() => ((ICollection<ApplicationCommandCheckBaseAttribute>)args.FailedChecks).Add(new TestCheckAttribute()));
	}

	[Fact]
	public void ContextMenuChecksFailedEventArgs_ExposeReadonlyFailedChecks()
	{
		var context = new ContextMenuContext(DisCatSharpCommandType.UserCommand);
		ApplicationCommandCheckBaseAttribute[] failedChecks = [new TestCheckAttribute()];

		var args = new ContextMenuChecksFailedEventArgs(new ServiceCollection().BuildServiceProvider())
		{
			Context = context,
			FailedChecks = new ReadOnlyCollection<ApplicationCommandCheckBaseAttribute>([.. failedChecks])
		};

		Assert.Same(context, args.Context);
		Assert.Same(failedChecks[0], Assert.Single(args.FailedChecks));
		Assert.Throws<NotSupportedException>(() => ((ICollection<ApplicationCommandCheckBaseAttribute>)args.FailedChecks).Add(new TestCheckAttribute()));
	}
}
