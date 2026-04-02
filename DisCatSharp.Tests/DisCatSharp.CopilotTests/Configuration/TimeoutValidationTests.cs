using System;
using System.Threading;

using DisCatSharp;

using Xunit;

namespace DisCatSharp.CopilotTests.Configuration;

/// <summary>
///     Tests for timeout validation across all configuration classes.
/// </summary>
public sealed class TimeoutValidationTests
{
	#region RestConfiguration.RequestTimeout

	[Fact]
	public void RestConfiguration_RequestTimeout_Default_IsPositive()
	{
		var config = new RestConfiguration();
		Assert.True(config.RequestTimeout > TimeSpan.Zero);
		Assert.Equal(TimeSpan.FromSeconds(20), config.RequestTimeout);
	}

	[Fact]
	public void RestConfiguration_RequestTimeout_PositiveValue_Accepted()
	{
		var config = new RestConfiguration { RequestTimeout = TimeSpan.FromSeconds(10) };
		Assert.Equal(TimeSpan.FromSeconds(10), config.RequestTimeout);
	}

	[Fact]
	public void RestConfiguration_RequestTimeout_InfiniteTimeSpan_Accepted()
	{
		var config = new RestConfiguration { RequestTimeout = Timeout.InfiniteTimeSpan };
		Assert.Equal(Timeout.InfiniteTimeSpan, config.RequestTimeout);
	}

	[Fact]
	public void RestConfiguration_RequestTimeout_Zero_Throws()
	{
		var config = new RestConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.RequestTimeout = TimeSpan.Zero);
	}

	[Fact]
	public void RestConfiguration_RequestTimeout_Negative_Throws()
	{
		var config = new RestConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.RequestTimeout = TimeSpan.FromSeconds(-1));
	}

	#endregion

	#region RestAdvancedConfiguration.QueueTimeout

	[Fact]
	public void RestAdvanced_QueueTimeout_Default_IsPositive()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Equal(TimeSpan.FromMinutes(5), config.QueueTimeout);
	}

	[Fact]
	public void RestAdvanced_QueueTimeout_Zero_DisablesTimeout()
	{
		var config = new RestAdvancedConfiguration { QueueTimeout = TimeSpan.Zero };
		Assert.Equal(TimeSpan.Zero, config.QueueTimeout);
	}

	[Fact]
	public void RestAdvanced_QueueTimeout_Negative_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.QueueTimeout = TimeSpan.FromSeconds(-1));
	}

	#endregion

	#region RestAdvancedConfiguration.QueueWarningThreshold

	[Fact]
	public void RestAdvanced_QueueWarningThreshold_Default_IsPositive()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Equal(TimeSpan.FromMinutes(2), config.QueueWarningThreshold);
	}

	[Fact]
	public void RestAdvanced_QueueWarningThreshold_Zero_DisablesWarning()
	{
		var config = new RestAdvancedConfiguration { QueueWarningThreshold = TimeSpan.Zero };
		Assert.Equal(TimeSpan.Zero, config.QueueWarningThreshold);
	}

	[Fact]
	public void RestAdvanced_QueueWarningThreshold_Negative_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.QueueWarningThreshold = TimeSpan.FromMilliseconds(-1));
	}

	#endregion

	#region RestAdvancedConfiguration.MaxRetries

	[Fact]
	public void RestAdvanced_MaxRetries_Default_IsPositive()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Equal(5, config.MaxRetries);
	}

	[Fact]
	public void RestAdvanced_MaxRetries_Zero_DisablesRetries()
	{
		var config = new RestAdvancedConfiguration { MaxRetries = 0 };
		Assert.Equal(0, config.MaxRetries);
	}

	[Fact]
	public void RestAdvanced_MaxRetries_Negative_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.MaxRetries = -1);
	}

	#endregion

	#region RestAdvancedConfiguration.MaxQueueDepthPerBucket

	[Fact]
	public void RestAdvanced_MaxQueueDepthPerBucket_Default_IsPositive()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Equal(1000, config.MaxQueueDepthPerBucket);
	}

	[Fact]
	public void RestAdvanced_MaxQueueDepthPerBucket_Zero_IsUnbounded()
	{
		var config = new RestAdvancedConfiguration { MaxQueueDepthPerBucket = 0 };
		Assert.Equal(0, config.MaxQueueDepthPerBucket);
	}

	[Fact]
	public void RestAdvanced_MaxQueueDepthPerBucket_Negative_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.MaxQueueDepthPerBucket = -1);
	}

	#endregion

	#region RestAdvancedConfiguration.CircuitBreakerThreshold

	[Fact]
	public void RestAdvanced_CircuitBreakerThreshold_Default_IsPositive()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Equal(10, config.CircuitBreakerThreshold);
	}

	[Fact]
	public void RestAdvanced_CircuitBreakerThreshold_Zero_Disables()
	{
		var config = new RestAdvancedConfiguration { CircuitBreakerThreshold = 0 };
		Assert.Equal(0, config.CircuitBreakerThreshold);
	}

	[Fact]
	public void RestAdvanced_CircuitBreakerThreshold_Negative_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.CircuitBreakerThreshold = -1);
	}

	#endregion

	#region RestAdvancedConfiguration.CircuitBreakerResetTimeout

	[Fact]
	public void RestAdvanced_CircuitBreakerResetTimeout_Default_IsPositive()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Equal(TimeSpan.FromSeconds(30), config.CircuitBreakerResetTimeout);
	}

	[Fact]
	public void RestAdvanced_CircuitBreakerResetTimeout_Zero_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.CircuitBreakerResetTimeout = TimeSpan.Zero);
	}

	[Fact]
	public void RestAdvanced_CircuitBreakerResetTimeout_Negative_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.CircuitBreakerResetTimeout = TimeSpan.FromSeconds(-5));
	}

	#endregion

	#region GatewayAdvancedConfiguration.SocketLockTimeout

	[Fact]
	public void GatewayAdvanced_SocketLockTimeout_Default_IsPositive()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Equal(TimeSpan.FromSeconds(30), config.SocketLockTimeout);
	}

	[Fact]
	public void GatewayAdvanced_SocketLockTimeout_PositiveValue_Accepted()
	{
		var config = new GatewayAdvancedConfiguration { SocketLockTimeout = TimeSpan.FromMinutes(1) };
		Assert.Equal(TimeSpan.FromMinutes(1), config.SocketLockTimeout);
	}

	[Fact]
	public void GatewayAdvanced_SocketLockTimeout_Zero_Throws()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.SocketLockTimeout = TimeSpan.Zero);
	}

	[Fact]
	public void GatewayAdvanced_SocketLockTimeout_Negative_Throws()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.SocketLockTimeout = TimeSpan.FromSeconds(-1));
	}

	#endregion

	#region GatewayAdvancedConfiguration.ReconnectDelay

	[Fact]
	public void GatewayAdvanced_ReconnectDelay_Default_IsPositive()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Equal(TimeSpan.FromSeconds(6), config.ReconnectDelay);
	}

	[Fact]
	public void GatewayAdvanced_ReconnectDelay_PositiveValue_Accepted()
	{
		var config = new GatewayAdvancedConfiguration { ReconnectDelay = TimeSpan.FromSeconds(10) };
		Assert.Equal(TimeSpan.FromSeconds(10), config.ReconnectDelay);
	}

	[Fact]
	public void GatewayAdvanced_ReconnectDelay_Zero_Throws()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.ReconnectDelay = TimeSpan.Zero);
	}

	[Fact]
	public void GatewayAdvanced_ReconnectDelay_Negative_Throws()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.ReconnectDelay = TimeSpan.FromMilliseconds(-100));
	}

	#endregion

	#region GatewayAdvancedConfiguration.HeartbeatZombieThreshold

	[Fact]
	public void GatewayAdvanced_HeartbeatZombieThreshold_Default_IsAtLeastOne()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.True(config.HeartbeatZombieThreshold >= 1);
		Assert.Equal(5, config.HeartbeatZombieThreshold);
	}

	[Fact]
	public void GatewayAdvanced_HeartbeatZombieThreshold_One_Accepted()
	{
		var config = new GatewayAdvancedConfiguration { HeartbeatZombieThreshold = 1 };
		Assert.Equal(1, config.HeartbeatZombieThreshold);
	}

	[Fact]
	public void GatewayAdvanced_HeartbeatZombieThreshold_Zero_Throws()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.HeartbeatZombieThreshold = 0);
	}

	[Fact]
	public void GatewayAdvanced_HeartbeatZombieThreshold_Negative_Throws()
	{
		var config = new GatewayAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.HeartbeatZombieThreshold = -1);
	}

	#endregion

	#region Clone constructors preserve timeout values

	[Fact]
	public void RestConfiguration_Clone_PreservesRequestTimeout()
	{
		var original = new RestConfiguration { RequestTimeout = TimeSpan.FromSeconds(45) };
		var clone = new RestConfiguration(original);
		Assert.Equal(TimeSpan.FromSeconds(45), clone.RequestTimeout);
	}

	[Fact]
	public void RestAdvanced_Clone_PreservesAllTimeouts()
	{
		var original = new RestAdvancedConfiguration
		{
			QueueTimeout = TimeSpan.FromMinutes(10),
			QueueWarningThreshold = TimeSpan.FromMinutes(3),
			MaxRetries = 8,
			MaxQueueDepthPerBucket = 500,
			CircuitBreakerThreshold = 20,
			CircuitBreakerResetTimeout = TimeSpan.FromMinutes(1)
		};

		var clone = new RestAdvancedConfiguration(original);

		Assert.Equal(TimeSpan.FromMinutes(10), clone.QueueTimeout);
		Assert.Equal(TimeSpan.FromMinutes(3), clone.QueueWarningThreshold);
		Assert.Equal(8, clone.MaxRetries);
		Assert.Equal(500, clone.MaxQueueDepthPerBucket);
		Assert.Equal(20, clone.CircuitBreakerThreshold);
		Assert.Equal(TimeSpan.FromMinutes(1), clone.CircuitBreakerResetTimeout);
	}

	[Fact]
	public void GatewayAdvanced_Clone_PreservesAllTimeouts()
	{
		var original = new GatewayAdvancedConfiguration
		{
			SocketLockTimeout = TimeSpan.FromSeconds(60),
			ReconnectDelay = TimeSpan.FromSeconds(10),
			HeartbeatZombieThreshold = 3
		};

		var clone = new GatewayAdvancedConfiguration(original);

		Assert.Equal(TimeSpan.FromSeconds(60), clone.SocketLockTimeout);
		Assert.Equal(TimeSpan.FromSeconds(10), clone.ReconnectDelay);
		Assert.Equal(3, clone.HeartbeatZombieThreshold);
	}

	[Fact]
	public void GatewayConfiguration_Clone_PreservesNestedAdvancedTimeouts()
	{
		var original = new GatewayConfiguration
		{
			Advanced = new()
			{
				SocketLockTimeout = TimeSpan.FromSeconds(45),
				ReconnectDelay = TimeSpan.FromSeconds(8),
				HeartbeatZombieThreshold = 4
			}
		};

		var clone = new GatewayConfiguration(original);

		Assert.Equal(TimeSpan.FromSeconds(45), clone.Advanced.SocketLockTimeout);
		Assert.Equal(TimeSpan.FromSeconds(8), clone.Advanced.ReconnectDelay);
		Assert.Equal(4, clone.Advanced.HeartbeatZombieThreshold);
	}

	[Fact]
	public void RestConfiguration_Clone_PreservesNestedAdvancedTimeouts()
	{
		var original = new RestConfiguration
		{
			RequestTimeout = TimeSpan.FromSeconds(30),
		};
		original.Advanced = new()
		{
			QueueTimeout = TimeSpan.FromMinutes(8),
			CircuitBreakerResetTimeout = TimeSpan.FromSeconds(45)
		};

		var clone = new RestConfiguration(original);

		Assert.Equal(TimeSpan.FromSeconds(30), clone.RequestTimeout);
		Assert.Equal(TimeSpan.FromMinutes(8), clone.Advanced.QueueTimeout);
		Assert.Equal(TimeSpan.FromSeconds(45), clone.Advanced.CircuitBreakerResetTimeout);
	}

	#endregion
}
