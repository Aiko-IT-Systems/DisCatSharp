using DisCatSharp.Enums;

using Xunit;

namespace DisCatSharp.CopilotTests.Gateway;

/// <summary>
///     Tests for <see cref="GatewayAdvancedConfiguration" /> and the dispatch infrastructure.
/// </summary>
public sealed class GatewayDispatchConfigurationTests
{
	[Fact]
	public void DefaultDispatchMode_IsConcurrentHandlers()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Equal(GatewayDispatchMode.ConcurrentHandlers, config.DispatchMode);
	}

	[Fact]
	public void DefaultDispatchQueueCapacity_Is10000()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Equal(10_000, config.DispatchQueueCapacity);
	}

	[Fact]
	public void DispatchQueueCapacity_Zero_AllowsUnbounded()
	{
		var config = new GatewayAdvancedConfiguration
		{
			DispatchQueueCapacity = 0
		};

		Assert.Equal(0, config.DispatchQueueCapacity);
	}

	[Fact]
	public void DispatchQueueCapacity_Negative_Throws()
	{
		var config = new GatewayAdvancedConfiguration();

		Assert.Throws<System.ArgumentOutOfRangeException>(() => config.DispatchQueueCapacity = -1);
	}

	[Fact]
	public void DispatchQueueCapacity_PositiveValue_IsAccepted()
	{
		var config = new GatewayAdvancedConfiguration
		{
			DispatchQueueCapacity = 50_000
		};

		Assert.Equal(50_000, config.DispatchQueueCapacity);
	}

	[Fact]
	public void DispatchMode_SequentialHandlers_IsAccepted()
	{
		var config = new GatewayAdvancedConfiguration
		{
			DispatchMode = GatewayDispatchMode.SequentialHandlers
		};

		Assert.Equal(GatewayDispatchMode.SequentialHandlers, config.DispatchMode);
	}

	[Fact]
	public void CloneConstructor_CopiesDispatchSettings()
	{
		var original = new GatewayAdvancedConfiguration
		{
			DispatchMode = GatewayDispatchMode.SequentialHandlers,
			DispatchQueueCapacity = 5_000
		};

		var clone = new GatewayAdvancedConfiguration(original);

		Assert.Equal(GatewayDispatchMode.SequentialHandlers, clone.DispatchMode);
		Assert.Equal(5_000, clone.DispatchQueueCapacity);
	}

	[Fact]
	public void CloneConstructor_DoesNotShareReference()
	{
		var original = new GatewayAdvancedConfiguration
		{
			DispatchMode = GatewayDispatchMode.SequentialHandlers,
			DispatchQueueCapacity = 1_000
		};

		var clone = new GatewayAdvancedConfiguration(original);

		// Mutate clone and verify original is unchanged
		clone.DispatchMode = GatewayDispatchMode.ConcurrentHandlers;
		clone.DispatchQueueCapacity = 99_999;

		Assert.Equal(GatewayDispatchMode.SequentialHandlers, original.DispatchMode);
		Assert.Equal(1_000, original.DispatchQueueCapacity);
	}

	[Fact]
	public void GatewayConfiguration_Advanced_DefaultsToNewInstance()
	{
		var config = new GatewayConfiguration();
		Assert.NotNull(config.Advanced);
		Assert.Equal(GatewayDispatchMode.ConcurrentHandlers, config.Advanced.DispatchMode);
	}

	[Fact]
	public void GatewayConfiguration_Clone_CopiesAdvancedDispatchSettings()
	{
		var original = new GatewayConfiguration
		{
			Advanced = new()
			{
				DispatchMode = GatewayDispatchMode.SequentialHandlers,
				DispatchQueueCapacity = 2_500
			}
		};

		var clone = new GatewayConfiguration(original);

		Assert.Equal(GatewayDispatchMode.SequentialHandlers, clone.Advanced.DispatchMode);
		Assert.Equal(2_500, clone.Advanced.DispatchQueueCapacity);
	}

	[Fact]
	public void GatewayDispatchMode_EnumValues_AreCorrect()
	{
		Assert.Equal(0, (int)GatewayDispatchMode.ConcurrentHandlers);
		Assert.Equal(1, (int)GatewayDispatchMode.SequentialHandlers);
	}
}
