using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Exceptions;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace DisCatSharp.Net.Serialization;

/// <summary>
///     Represents discord json.
/// </summary>
public static class DiscordJson
{
	/// <summary>
	///     Gets the serializer.
	/// </summary>
	private static readonly JsonSerializer s_serializer = JsonSerializer.CreateDefault(new()
	{
		ContractResolver = new DisCatSharpContractResolver()
	});

	/// <summary>
	///     Serializes the specified object to a JSON string.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <returns>A JSON string representation of the object.</returns>
	public static string SerializeObject(object value)
		=> SerializeObjectInternal(value, null!, s_serializer);

	/// <summary>
	///     Deserializes the specified JSON string to an object.
	/// </summary>
	/// <typeparam name="T">The type</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	public static T DeserializeObject<T>(string? json, BaseDiscordClient? discord) where T : ObservableApiObject
		=> DeserializeObjectInternal<T>(json, discord);

	/// <summary>
	///     Deserializes the specified JSON string to an object of the type <see cref="IEnumerable{T}" />.
	/// </summary>
	/// <typeparam name="T">The enumerable type.</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	public static T DeserializeIEnumerableObject<T>(string? json, BaseDiscordClient? discord) where T : IEnumerable<ObservableApiObject>
		=> DeserializeIEnumerableObjectInternal<T>(json, discord);

	/// <summary>
	///    Deserializes the specified JSON string to a dictionary object.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	public static Dictionary<TKey, TValue> DeserializeDictionaryObject<TKey, TValue>(string? json, BaseDiscordClient? discord)
		=> DeserializeDictionaryObjectInternal<TKey, TValue>(json, discord);

	/// <summary>
	///     Populates an object with the values from a JSON node.
	/// </summary>
	/// <param name="value">The token to populate the object with.</param>
	/// <param name="target">The object to populate.</param>
	public static void PopulateObject(JToken value, object target)
	{
		using var reader = value.CreateReader();
		s_serializer.Populate(reader, target);
	}

	/// <summary>
	///     Converts this token into an object, passing any properties through extra
	///     <see cref="Newtonsoft.Json.JsonConverter" />s if needed.
	/// </summary>
	/// <param name="token">The token to convert</param>
	/// <typeparam name="T">Type to convert to</typeparam>
	/// <returns>The converted token</returns>
	public static T ToDiscordObject<T>(this JToken token)
		=> token.ToObject<T>(s_serializer)!;

	/// <summary>
	///     Serializes the object.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="type">The type.</param>
	/// <param name="jsonSerializer">The json serializer.</param>
	private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
	{
		var stringWriter = new StringWriter(new(), CultureInfo.InvariantCulture);
		using (var jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonTextWriter, value, type);
		}

		return stringWriter.ToString();
	}

	/// <summary>
	///     Handles <see cref="DiscordJson" /> errors.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The error event args.</param>
	/// <param name="discord">The discord client.</param>
	private static void DiscordJsonErrorHandler(object? sender, ErrorEventArgs e, BaseDiscordClient? discord)
	{
		if (discord is null || e.ErrorContext.Error is not JsonReaderException jre)
			return;

		var sentryMessage = "DiscordJson error on deserialization (" + (sender?.GetType().Name ?? "x") + ")\n\n" +
							"Path: " + e.ErrorContext.Path + "\n" +
							"Original Object" + e.ErrorContext.OriginalObject + "\n" +
							"Current Object" + e.CurrentObject + "\n\n" +
							"JRE Message:" + jre.Message + "\n" +
							"JRE Line Number: " + jre.LineNumber + "\n" +
							"JRE Line Position" + jre.LinePosition + "\n" +
							"JRE Path" + jre.Path;

		if (discord.Configuration.EnableLibraryDeveloperMode)
			discord.Logger.LogError(e.ErrorContext.Error, "{msg}\n\n{raw}", sentryMessage, e.ErrorContext.OriginalObject);

		if (!discord.DiagnosticsSink.IsEnabled)
			return;

		discord.DiagnosticsSink.CaptureReport(new()
		{
			Source = "DisCatSharp",
			Severity = Telemetry.DiagnosticSeverity.Error,
			Logger = nameof(DiscordJson),
			Message = Utilities.StripTokensAndOptIds(sentryMessage, discord.Configuration.EnableDiscordIdScrubber)!,
			Exception = new DiscordJsonException(jre),
			UserInfo = Telemetry.TelemetryBootstrap.BuildUserInfo(discord.Configuration, discord.CurrentUser)
		});

		if (discord.Configuration.EnableLibraryDeveloperMode)
			discord.Logger.LogInformation("DiscordJson exception reported to diagnostics sink");
	}

	/// <summary>
	///     Deserializes the specified JSON string to an object.
	/// </summary>
	/// <typeparam name="T">The type</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	private static T DeserializeObjectInternal<T>(string? json, BaseDiscordClient? discord) where T : ObservableApiObject
	{
		ArgumentNullException.ThrowIfNull(json, nameof(json));

		var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
		{
			ContractResolver = new DisCatSharpContractResolver(),
			Error = (s, e) => DiscordJsonErrorHandler(s, e, discord)
		})!;

		if (discord is null)
			return obj;

		obj.Discord = discord;

		if (!obj.AdditionalProperties.Any())
			return obj;

		var sentryMessage = "Found missing properties in api response for " + obj.GetType().Name;
		Dictionary<string, object> sentryFields = [];
		var vals = 0;
		foreach (var ap in obj.AdditionalProperties)
		{
			vals++;
			if (obj.IgnoredJsonKeys.Count is not 0 && obj.IgnoredJsonKeys.Any(x => x == ap.Key))
				continue;

			if (vals is 1)
				if (discord.Configuration.EnableLibraryDeveloperMode)
				{
					discord.Logger.LogInformation("{sentry}", sentryMessage);
					discord.Logger.LogDebug("{json}", json);
				}

			var fieldType = InferFieldType(ap.Value);
			sentryFields[ap.Key] = fieldType;
			if (discord.Configuration.EnableLibraryDeveloperMode)
				discord.Logger.LogInformation("Found field {field}: {type} on {object}", ap.Key, fieldType, obj.GetType().Name);
		}

		if (!discord.Configuration.EnableSentry || sentryFields.Count is 0)
			return obj;

		var sentryJson = JsonConvert.SerializeObject(sentryFields, Formatting.Indented);
		sentryMessage += "\n\nNew fields: " + sentryJson;

		// Scrub the raw payload for safe inclusion — strip tokens/IDs
		var scrubbedPayload = Utilities.StripTokensAndOptIdsInJson(json, discord.Configuration.EnableDiscordIdScrubber);

		// Large payloads: truncate inline extra but include full version as file payload
		byte[]? filePayload = null;
		string? filePayloadName = null;
		if (scrubbedPayload is not null && scrubbedPayload.Length > 8192)
		{
			filePayload = System.Text.Encoding.UTF8.GetBytes(scrubbedPayload);
			filePayloadName = $"scrubbed-payload-{obj.GetType().Name}.json";
			scrubbedPayload = scrubbedPayload[..8192] + "... (truncated, full payload in file)";
		}

		discord.DiagnosticsSink.CaptureReport(new()
		{
			Source = "DisCatSharp",
			Severity = Telemetry.DiagnosticSeverity.Warning,
			Logger = nameof(DiscordJson),
			Message = sentryMessage,
			Extra = new Dictionary<string, object>
			{
				{ "Found Fields", sentryJson },
				{ "Scrubbed Payload", scrubbedPayload ?? "null" }
			},
			Tags = new Dictionary<string, string> { ["dcs.entity_type"] = obj.GetType().Name },
			FilePayload = filePayload,
			FilePayloadName = filePayloadName,
			UserInfo = Telemetry.TelemetryBootstrap.BuildUserInfo(discord.Configuration, discord.CurrentUser)
		});

		if (discord.Configuration.EnableLibraryDeveloperMode)
			discord.Logger.LogInformation("Missing fields reported to diagnostics sink");

		return obj;
	}

	/// <summary>
	///     Deserializes the specified JSON string to a dictionary object.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	private static Dictionary<TKey, TValue> DeserializeDictionaryObjectInternal<TKey, TValue>(string? json, BaseDiscordClient? discord)
	{
		ArgumentNullException.ThrowIfNull(json, nameof(json));

		var obj = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json, new JsonSerializerSettings
		{
			ContractResolver = new DisCatSharpContractResolver(),
			Error = (s, e) => DiscordJsonErrorHandler(s, e, discord)
		})!;

		return obj;
	}

	/// <summary>
	///     Deserializes the specified JSON string to an object of the type <see cref="IEnumerable{T}" />.
	/// </summary>
	/// <typeparam name="T">The enumerable type.</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	private static T DeserializeIEnumerableObjectInternal<T>(string? json, BaseDiscordClient? discord) where T : IEnumerable<ObservableApiObject>
	{
		ArgumentNullException.ThrowIfNull(json, nameof(json));

		var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
		{
			ContractResolver = new DisCatSharpContractResolver(),
			Error = (s, e) => DiscordJsonErrorHandler(s, e, discord)
		})!;

		if (discord is null)
			return obj;

		foreach (var ob in obj)
			ob.Discord = discord;

		if (!obj.Any(x => x.AdditionalProperties.Any()))
			return obj;

		var first = obj.First();
		var sentryMessage = "Found missing properties in api response for " + first.GetType().Name;
		Dictionary<string, object> sentryFields = [];
		var vals = 0;
		foreach (var ap in first.AdditionalProperties)
		{
			vals++;
			if (first.IgnoredJsonKeys.Count is not 0 && first.IgnoredJsonKeys.Any(x => x == ap.Key))
				continue;

			if (vals is 1)
				if (discord.Configuration.EnableLibraryDeveloperMode)
				{
					discord.Logger.LogInformation("{sentry}", sentryMessage);
					discord.Logger.LogDebug("{json}", json);
				}

			var fieldType = InferFieldType(ap.Value);
			sentryFields[ap.Key] = fieldType;
			if (discord.Configuration.EnableLibraryDeveloperMode)
				discord.Logger.LogInformation("Found field {field}: {type} on {object}", ap.Key, fieldType, first.GetType().Name);
		}

		if (!discord.Configuration.EnableSentry || sentryFields.Count == 0)
			return obj;

		var sentryJson = JsonConvert.SerializeObject(sentryFields, Formatting.Indented);
		sentryMessage += "\n\nNew fields: " + sentryJson;

		var scrubbedPayload = Utilities.StripTokensAndOptIdsInJson(json, discord.Configuration.EnableDiscordIdScrubber);

		byte[]? filePayload = null;
		string? filePayloadName = null;
		if (scrubbedPayload is not null && scrubbedPayload.Length > 8192)
		{
			filePayload = System.Text.Encoding.UTF8.GetBytes(scrubbedPayload);
			filePayloadName = $"scrubbed-payload-{first.GetType().Name}.json";
			scrubbedPayload = scrubbedPayload[..8192] + "... (truncated, full payload in file)";
		}

		discord.DiagnosticsSink.CaptureReport(new()
		{
			Source = "DisCatSharp",
			Severity = Telemetry.DiagnosticSeverity.Warning,
			Logger = nameof(DiscordJson),
			Message = sentryMessage,
			Extra = new Dictionary<string, object>
			{
				{ "Found Fields", sentryJson },
				{ "Scrubbed Payload", scrubbedPayload ?? "null" }
			},
			Tags = new Dictionary<string, string> { ["dcs.entity_type"] = first.GetType().Name },
			FilePayload = filePayload,
			FilePayloadName = filePayloadName,
			UserInfo = Telemetry.TelemetryBootstrap.BuildUserInfo(discord.Configuration, discord.CurrentUser)
		});

		if (discord.Configuration.EnableLibraryDeveloperMode)
			discord.Logger.LogInformation("Missing fields reported to diagnostics sink");

		return obj;
	}

	/// <summary>
	///     Infers a safe, scrubbed type schema from a JSON value.
	///     Returns only structural types — no actual data is leaked.
	///     Recurses into objects and arrays to describe the full shape.
	/// </summary>
	/// <param name="value">The raw value from <see cref="ObservableApiObject.AdditionalProperties" />.</param>
	/// <returns>
	///     A type descriptor: a <see cref="string" /> for primitives (e.g., "integer"),
	///     a <see cref="Dictionary{TKey, TValue}" /> for objects describing nested structure,
	///     or a formatted string for arrays (e.g., "array&lt;object&gt;").
	/// </returns>
	private static object InferFieldType(object? value)
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
			JTokenType.Raw => "raw",
			JTokenType.Bytes => "bytes",
			JTokenType.Guid => "guid",
			JTokenType.Uri => "uri",
			JTokenType.TimeSpan => "timespan",
			_ => "unknown"
		};

	/// <summary>
	///     Recursively builds a type schema for a JSON object.
	///     E.g. <c>{"powerup": {"boost_price": "integer"}}</c>.
	/// </summary>
	private static Dictionary<string, object> InferObjectSchema(JObject obj)
	{
		Dictionary<string, object> schema = [];
		foreach (var prop in obj.Properties())
			schema[prop.Name] = InferJTokenType(prop.Value);
		return schema;
	}

	/// <summary>
	///     Infers a descriptive type for JSON arrays by peeking at the first element.
	///     For object arrays, includes the full element schema.
	/// </summary>
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
