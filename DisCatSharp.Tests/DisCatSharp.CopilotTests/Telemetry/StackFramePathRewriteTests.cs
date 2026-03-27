using System;
using System.IO;
using System.Linq;
using System.Reflection;

using DisCatSharp.Telemetry;

using Sentry;
using Sentry.Protocol;

using Xunit;

namespace DisCatSharp.CopilotTests.Telemetry;

public class StackFramePathRewriteTests
{
	[Fact]
	public void TryRewriteFramePath_PrefixesCoreRelativePath()
	{
		var rewritten = TelemetryBootstrap.TryRewriteFramePath("Clients\\DiscordClient.Dispatch.cs", "DisCatSharp", out var rewrittenPath);

		Assert.True(rewritten);
		Assert.Equal("DisCatSharp\\Clients\\DiscordClient.Dispatch.cs", rewrittenPath);
	}

	[Fact]
	public void TryRewriteFramePath_UsesPackageSegmentFromAbsolutePath()
	{
		var rewritten = TelemetryBootstrap.TryRewriteFramePath(@"C:\agent\_work\1\s\DisCatSharp.Hosting\BaseHostedService.cs", "DisCatSharp.Hosting", out var rewrittenPath);

		Assert.True(rewritten);
		Assert.Equal("DisCatSharp.Hosting\\BaseHostedService.cs", rewrittenPath);
	}

	[Fact]
	public void TryRewriteFramePath_DoesNotRewriteUnknownAbsolutePath()
	{
		var absolutePath = Path.Combine(Path.GetTempPath(), "SomeOtherProject", "BaseHostedService.cs");
		var rewritten = TelemetryBootstrap.TryRewriteFramePath(absolutePath, "DisCatSharp.Hosting", out var rewrittenPath);

		Assert.False(rewritten);
		Assert.Equal(string.Empty, rewrittenPath);
	}

	[Fact]
	public void TryRewriteFramePath_UsesPackageSegmentFromUnixAbsolutePath()
	{
		var absolutePath = "/home/runner/work/DisCatSharp/DisCatSharp/DisCatSharp.Hosting/BaseHostedService.cs";
		var rewritten = TelemetryBootstrap.TryRewriteFramePath(absolutePath, "DisCatSharp.Hosting", out var rewrittenPath);

		Assert.True(rewritten);
		Assert.Equal("DisCatSharp.Hosting\\BaseHostedService.cs", rewrittenPath);
	}

	[Fact]
	public void TryRewriteFramePath_DoesNotRewriteSourceLinkUrl()
	{
		var path = "https://raw.githubusercontent.com/Aiko-IT-Systems/DisCatSharp/main/DisCatSharp.Hosting/BaseHostedService.cs";
		var rewritten = TelemetryBootstrap.TryRewriteFramePath(path, "DisCatSharp.Hosting", out var rewrittenPath);

		Assert.False(rewritten);
		Assert.Equal(string.Empty, rewrittenPath);
	}

	[Fact]
	public void RewriteStackFramePaths_UsesSourceTagToRewriteFrames()
	{
		var frame = new SentryStackFrame
		{
			FileName = "BaseHostedService.cs",
			AbsolutePath = "BaseHostedService.cs"
		};
		var evt = new SentryEvent();
		evt.SetTag(DiagnosticTags.Source, "DisCatSharp.Hosting");
		SetSentryExceptions(evt, [new SentryException
		{
			Stacktrace = new()
			{
				Frames = [frame]
			}
		}]);

		TelemetryBootstrap.RewriteStackFramePaths(evt);

		Assert.Equal("DisCatSharp.Hosting\\BaseHostedService.cs", frame.FileName);
		Assert.Equal("DisCatSharp.Hosting\\BaseHostedService.cs", frame.AbsolutePath);
	}

	private static void SetSentryExceptions(SentryEvent evt, SentryException[] exceptions)
	{
		var property = typeof(SentryEvent).GetProperty("SentryExceptions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		Assert.NotNull(property);
		property!.SetValue(evt, exceptions);
	}
}
