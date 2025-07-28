using System;
using Newtonsoft.Json;
using Xunit;
using DisCatSharp.Net.Serialization;

namespace DisCatSharp.Copilot.Tests;

public class NullValueHandlingTests
{
	private class TestClass
	{
		[JsonProperty("should_serialize", NullValueHandling = NullValueHandling.Ignore)]
		public string ShouldSerialize { get; set; }

		[JsonProperty("should_always_serialize", NullValueHandling = NullValueHandling.Include)]
		public string ShouldAlwaysSerialize { get; set; }
	}

	[Fact]
	public void Test_NullValueHandling_Respected()
	{
		var obj = new TestClass
		{
			ShouldSerialize = null,
			ShouldAlwaysSerialize = null
		};

		var json = DiscordJson.SerializeObject(obj);

		Assert.DoesNotContain("should_serialize", json);
		Assert.Contains("should_always_serialize", json);
	}
}
