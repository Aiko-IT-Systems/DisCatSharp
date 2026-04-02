using System;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Tests for <see cref="RestAdvancedConfiguration" /> defaults and cloning.
/// </summary>
public class RestAdvancedConfigurationTests
{
	[Fact]
	public void Defaults_AreReasonable()
	{
		var config = new RestAdvancedConfiguration();

		Assert.Equal(TimeSpan.FromMinutes(5), config.QueueTimeout);
		Assert.Equal(TimeSpan.FromMinutes(2), config.QueueWarningThreshold);
		Assert.Equal(5, config.MaxRetries);
	}

	[Fact]
	public void Clone_CopiesAllProperties()
	{
		var original = new RestAdvancedConfiguration
		{
			QueueTimeout = TimeSpan.FromMinutes(10),
			QueueWarningThreshold = TimeSpan.FromMinutes(3),
			MaxRetries = 8
		};

		var clone = new RestAdvancedConfiguration(original);

		Assert.Equal(original.QueueTimeout, clone.QueueTimeout);
		Assert.Equal(original.QueueWarningThreshold, clone.QueueWarningThreshold);
		Assert.Equal(original.MaxRetries, clone.MaxRetries);
	}

	[Fact]
	public void Clone_IsIndependent()
	{
		var original = new RestAdvancedConfiguration();
		var clone = new RestAdvancedConfiguration(original);

		clone.QueueTimeout = TimeSpan.Zero;
		clone.MaxRetries = 0;

		Assert.Equal(TimeSpan.FromMinutes(5), original.QueueTimeout);
		Assert.Equal(5, original.MaxRetries);
	}

	[Fact]
	public void ZeroTimeout_DisablesTimeout()
	{
		var config = new RestAdvancedConfiguration
		{
			QueueTimeout = TimeSpan.Zero
		};

		Assert.Equal(TimeSpan.Zero, config.QueueTimeout);
	}

	[Fact]
	public void ZeroWarning_DisablesWarning()
	{
		var config = new RestAdvancedConfiguration
		{
			QueueWarningThreshold = TimeSpan.Zero
		};

		Assert.Equal(TimeSpan.Zero, config.QueueWarningThreshold);
	}
}
