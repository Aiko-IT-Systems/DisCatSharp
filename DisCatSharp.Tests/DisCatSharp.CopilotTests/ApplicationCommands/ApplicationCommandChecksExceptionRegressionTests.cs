using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.Exceptions;

using Xunit;

namespace DisCatSharp.Copilot.Tests;

public class ApplicationCommandChecksExceptionRegressionTests
{
	private sealed class TestCheckAttribute : ApplicationCommandCheckBaseAttribute
	{
		public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
			=> Task.FromResult(false);
	}

	[Fact]
	public void SlashExecutionChecksFailedException_ExposesStableMessageAndReadonlyFailedChecks()
	{
		ApplicationCommandCheckBaseAttribute[] failedChecks = [new TestCheckAttribute()];

		var exception = new SlashExecutionChecksFailedException(failedChecks);

		Assert.Equal("One or more pre-execution checks failed.", exception.Message);
		Assert.Same(failedChecks[0], Assert.Single(exception.FailedChecks));
		Assert.Throws<NotSupportedException>(() => ((ICollection<ApplicationCommandCheckBaseAttribute>)exception.FailedChecks).Add(new TestCheckAttribute()));
	}

	[Fact]
	public void ContextMenuExecutionChecksFailedException_ExposesStableMessageAndReadonlyFailedChecks()
	{
		ApplicationCommandCheckBaseAttribute[] failedChecks = [new TestCheckAttribute()];

		var exception = new ContextMenuExecutionChecksFailedException(failedChecks);

		Assert.Equal("One or more pre-execution checks failed.", exception.Message);
		Assert.Same(failedChecks[0], Assert.Single(exception.FailedChecks));
		Assert.Throws<NotSupportedException>(() => ((ICollection<ApplicationCommandCheckBaseAttribute>)exception.FailedChecks).Add(new TestCheckAttribute()));
	}
}
