using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;
using Xunit.Abstractions;

namespace DisCatSharp.CopilotTests.Telemetry;

/// <summary>
///     Tests for the missing-field type inference used by DiscordJson.
///     Verifies that InferFieldType / InferJTokenType produce the correct
///     scrubbed type schema from raw JSON values, with no user data leaked.
/// </summary>
public class FieldTypeInferenceTests
{
	private readonly ITestOutputHelper _output;

	public FieldTypeInferenceTests(ITestOutputHelper output)
	{
		this._output = output;
	}

	/// <summary>
	///     Simulates what DisCatSharp would report for a brand new nested field
	///     like tenant_metadata with deep structure.
	/// </summary>
	[Fact]
	public void NestedObject_ProducesFullTypeSchema()
	{
		var json = """
		           {
		             "id": "1234567890",
		             "name": "Test Guild",
		             "tenant_metadata": {
		               "guild_monetization": {
		                 "powerup": {
		                   "boost_price": 5,
		                   "enabled": true
		                 }
		               }
		             }
		           }
		           """;

		var parsed = JObject.Parse(json);
		var tenantMetadata = parsed["tenant_metadata"]!;

		var schema = InferFieldTypePublic(tenantMetadata);
		var serialized = JsonConvert.SerializeObject(schema, Formatting.Indented);

		this._output.WriteLine("=== tenant_metadata type schema ===");
		this._output.WriteLine(serialized);

		Assert.IsType<Dictionary<string, object>>(schema);
		var root = (Dictionary<string, object>)schema;
		Assert.True(root.ContainsKey("guild_monetization"));

		var guildMon = Assert.IsType<Dictionary<string, object>>(root["guild_monetization"]);
		var powerup = Assert.IsType<Dictionary<string, object>>(guildMon["powerup"]);
		Assert.Equal("integer", powerup["boost_price"]);
		Assert.Equal("boolean", powerup["enabled"]);
	}

	/// <summary>
	///     Tests all primitive JToken types are reported with their real type names.
	/// </summary>
	[Theory]
	[InlineData("42", "integer")]
	[InlineData("3.14", "float")]
	[InlineData("\"hello\"", "string")]
	[InlineData("true", "boolean")]
	[InlineData("false", "boolean")]
	[InlineData("null", "null")]
	public void Primitives_ReportCorrectType(string jsonValue, string expectedType)
	{
		var token = JToken.Parse(jsonValue);
		var result = InferFieldTypePublic(token);

		this._output.WriteLine($"JSON: {jsonValue} => Type: {result}");
		Assert.Equal(expectedType, result);
	}

	/// <summary>
	///     Tests that arrays report element types.
	/// </summary>
	[Fact]
	public void Array_ReportsElementType()
	{
		var intArray = JToken.Parse("[1, 2, 3]");
		var strArray = JToken.Parse("[\"a\", \"b\"]");
		var emptyArray = JToken.Parse("[]");

		var intResult = InferFieldTypePublic(intArray);
		var strResult = InferFieldTypePublic(strArray);
		var emptyResult = InferFieldTypePublic(emptyArray);

		this._output.WriteLine($"[1,2,3] => {intResult}");
		this._output.WriteLine($"[\"a\",\"b\"] => {strResult}");
		this._output.WriteLine($"[] => {emptyResult}");

		Assert.Equal("array<integer>", intResult);
		Assert.Equal("array<string>", strResult);
		Assert.Equal("array", emptyResult);
	}

	/// <summary>
	///     Tests that an array of objects returns the element schema.
	/// </summary>
	[Fact]
	public void ArrayOfObjects_ReportsElementSchema()
	{
		var json = """[{"id": 123, "name": "test"}, {"id": 456, "name": "other"}]""";
		var token = JToken.Parse(json);

		var result = InferFieldTypePublic(token);
		var serialized = JsonConvert.SerializeObject(result, Formatting.Indented);

		this._output.WriteLine("=== array of objects schema ===");
		this._output.WriteLine(serialized);

		Assert.IsType<Dictionary<string, object>>(result);
		var dict = (Dictionary<string, object>)result;
		Assert.True(dict.ContainsKey("array_element"));

		var element = Assert.IsType<Dictionary<string, object>>(dict["array_element"]);
		Assert.Equal("integer", element["id"]);
		Assert.Equal("string", element["name"]);
	}

	/// <summary>
	///     Simulates a full "Found Fields" report for a realistic Discord API response
	///     containing multiple new fields of varying types.
	/// </summary>
	[Fact]
	public void RealisticDiscordPayload_ProducesReadableReport()
	{
		var additionalProps = new Dictionary<string, JToken>
		{
			["clan_tag"] = JToken.Parse("\"ABC\""),
			["max_stage_video_users"] = JToken.Parse("50"),
			["safety_alerts_enabled"] = JToken.Parse("true"),
			["incident_actions"] = JToken.Parse("null"),
			["soundboard_sounds"] = JToken.Parse("[{\"sound_id\": \"123\", \"volume\": 0.5, \"emoji_name\": \"smile\"}]"),
			["tenant_metadata"] = JToken.Parse("""
			{
			  "guild_monetization": {
			    "powerup": {
			      "boost_price": 5
			    }
			  }
			}
			""")
		};

		Dictionary<string, object> sentryFields = [];
		foreach (var ap in additionalProps)
			sentryFields[ap.Key] = InferFieldTypePublic(ap.Value);

		var sentryJson = JsonConvert.SerializeObject(sentryFields, Formatting.Indented);
		var message = "Found missing properties in api response for DiscordGuild\n\nNew fields: " + sentryJson;

		this._output.WriteLine("=== Full diagnostic report output ===");
		this._output.WriteLine(message);

		Assert.Equal("string", sentryFields["clan_tag"]);
		Assert.Equal("integer", sentryFields["max_stage_video_users"]);
		Assert.Equal("boolean", sentryFields["safety_alerts_enabled"]);
		Assert.Equal("null", sentryFields["incident_actions"]);

		var tenant = Assert.IsType<Dictionary<string, object>>(sentryFields["tenant_metadata"]);
		Assert.True(tenant.ContainsKey("guild_monetization"));

		var sounds = Assert.IsType<Dictionary<string, object>>(sentryFields["soundboard_sounds"]);
		Assert.True(sounds.ContainsKey("array_element"));
	}

	/// <summary>
	///     Verifies that no actual user data appears in the schema output —
	///     only type descriptors.
	/// </summary>
	[Fact]
	public void NoUserDataLeaked_OnlyTypeDescriptors()
	{
		var json = """
		           {
		             "secret_token": "mfa.very-secret-token-12345",
		             "user_id": 856780995629154305,
		             "nested": {
		               "password": "hunter2",
		               "score": 42
		             }
		           }
		           """;

		var parsed = JObject.Parse(json);
		Dictionary<string, object> schema = [];
		foreach (var prop in parsed.Properties())
			schema[prop.Name] = InferFieldTypePublic(prop.Value);

		var serialized = JsonConvert.SerializeObject(schema, Formatting.Indented);

		this._output.WriteLine("=== Privacy check ===");
		this._output.WriteLine(serialized);

		// Ensure no actual values appear — key names are preserved, only values are scrubbed
		Assert.DoesNotContain("mfa.very-secret-token-12345", serialized);
		Assert.DoesNotContain("hunter2", serialized);
		Assert.DoesNotContain("856780995629154305", serialized);

		Assert.Equal("string", schema["secret_token"]);
		Assert.Equal("integer", schema["user_id"]);
		var nested = Assert.IsType<Dictionary<string, object>>(schema["nested"]);
		Assert.Equal("string", nested["password"]);
		Assert.Equal("integer", nested["score"]);
	}

	/// <summary>
	///     Mirror of DiscordJson.InferFieldType for test verification.
	/// </summary>
	private static object InferFieldTypePublic(object? value)
		=> value switch
		{
			null => "null",
			JToken jt => InferJTokenType(jt),
			_ => value.GetType().Name.ToLowerInvariant()
		};

	private static object InferJTokenType(JToken token)
		=> token.Type switch
		{
			JTokenType.None => "none",
			JTokenType.Object => InferObjectSchema((JObject)token),
			JTokenType.Array => InferArrayType((JArray)token),
			JTokenType.Constructor => "constructor",
			JTokenType.Property => "property",
			JTokenType.Comment => "comment",
			JTokenType.Integer => "integer",
			JTokenType.Float => "float",
			JTokenType.String => "string",
			JTokenType.Boolean => "boolean",
			JTokenType.Null => "null",
			JTokenType.Undefined => "undefined",
			JTokenType.Date => "date",
			JTokenType.Guid => "guid",
			JTokenType.Uri => "uri",
			JTokenType.TimeSpan => "timespan",
			JTokenType.Bytes => "bytes",
			JTokenType.Raw => "raw",
			_ => "unknown"
		};

	private static Dictionary<string, object> InferObjectSchema(JObject obj)
	{
		Dictionary<string, object> schema = [];
		foreach (var prop in obj.Properties())
			schema[prop.Name] = InferJTokenType(prop.Value);
		return schema;
	}

	private static object InferArrayType(JArray arr)
	{
		if (arr.Count == 0)
			return "array";

		var elementType = InferJTokenType(arr[0]);
		return elementType is string typeName
			? $"array<{typeName}>"
			: new Dictionary<string, object> { ["array_element"] = elementType };
	}
}
