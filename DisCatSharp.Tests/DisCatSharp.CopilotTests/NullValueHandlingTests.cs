using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using DisCatSharp.Net.Serialization;
using Xunit.Abstractions;

namespace DisCatSharp.Copilot.Tests;

public class NullValueHandlingTests
{
	private readonly ITestOutputHelper _output;

	public NullValueHandlingTests(ITestOutputHelper output)
	{
		this._output = output;
	}

	public class TestClass
	{
		[JsonProperty("should_serialize", NullValueHandling = NullValueHandling.Ignore)]
		public string? ShouldSerialize { get; set; }

		[JsonProperty("should_always_serialize", NullValueHandling = NullValueHandling.Include)]
		public string? ShouldAlwaysSerialize { get; set; }
	}

	public class ShouldSerializeTestClass
	{
		[JsonProperty("value", NullValueHandling = NullValueHandling.Include)]
		public string? Value { get; set; }

		[JsonIgnore]
		public bool ShouldSerializeValueFlag { get; set; } = true;

		public bool ShouldSerializeValue() => this.ShouldSerializeValueFlag;
	}

	public class ComponentTestClass
	{
		[JsonProperty("components", NullValueHandling = NullValueHandling.Include)]
		public List<string>? Components { get; set; }

		public bool ShouldSerializeComponents() => this.HasComponents;

		[JsonIgnore]
		public bool HasComponents { get; set; }
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

	[Fact]
	public void Test_ShouldSerialize_Method_Respected()
	{
		var obj = new ShouldSerializeTestClass
		{
			Value = "test",
			ShouldSerializeValueFlag = false
		};

		var json = DiscordJson.SerializeObject(obj);
		this._output.WriteLine($"ShouldSerializeValueFlag = false: {json}");
		Assert.DoesNotContain("value", json);

		obj.ShouldSerializeValueFlag = true;
		json = DiscordJson.SerializeObject(obj);
		this._output.WriteLine($"ShouldSerializeValueFlag = true: {json}");
		Assert.Contains("value", json);
	}

	[Fact]
	public void Test_Components_Serialization_Cases()
	{
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new DisCatSharpContractResolver()
		};

		// Case 1: Components = null, HasComponents = false
		// TLDR: If you use ShouldSerializeXy(), set NullValueHandling.Include for that property (or globally) to ensure it is serialized even when null.
		var obj = new ComponentTestClass { Components = null, HasComponents = false };
		DumpCase("Case 1 (null, HasComponents=false)", obj);
		Assert.DoesNotContain("components", JsonConvert.SerializeObject(obj, settings));

		// Case 2: Components = null, HasComponents = true
		obj = new ComponentTestClass { Components = null, HasComponents = true };
		DumpCase("Case 2 (null, HasComponents=true)", obj);
		var json = JsonConvert.SerializeObject(obj, settings);
		Assert.Contains("components", json); // Should be present as null
		Assert.Contains("\"components\":null", json);

		// Case 3: Components = [], HasComponents = true
		obj = new ComponentTestClass { Components = new List<string>(), HasComponents = true };
		DumpCase("Case 3 (empty list, HasComponents=true)", obj);
		json = JsonConvert.SerializeObject(obj, settings);
		Assert.Contains("components", json);
		Assert.Contains("[]", json);

		// Case 4: Components = ["foo"], HasComponents = true
		obj = new ComponentTestClass { Components = new List<string> { "foo" }, HasComponents = true };
		DumpCase("Case 4 (non-empty, HasComponents=true)", obj);
		json = JsonConvert.SerializeObject(obj, settings);
		Assert.Contains("components", json);
		Assert.Contains("foo", json);

		void DumpCase(string label, object obj)
		{
			var json = JsonConvert.SerializeObject(obj, settings);
			this._output.WriteLine($"{label}:\n  HasComponents: {((ComponentTestClass)obj).HasComponents}\n  Components: {((ComponentTestClass)obj).Components?.Count.ToString() ?? "null"}\n  JSON: {json}\n");
		}
	}
}
