using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Sentry;

namespace DisCatSharp.Net.Serialization;

/// <summary>
/// Represents JSON serialization and deserialization for Discord entities.
/// </summary>
public static class DiscordJson
{
	/// <summary>
	/// Gets the default json serializer with an optional contract resolver.
	/// </summary>
	private static readonly JsonSerializer s_serializer = JsonSerializer.CreateDefault(new()
	{
		ContractResolver = new OptionalJsonContractResolver()
	});

	/// <summary>
	/// Serializes the specified object to a JSON string.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <returns>A JSON string representation of the object.</returns>
	public static string SerializeObject(object value)
		=> SerializeObjectInternal(value, null!, s_serializer);

	/// <summary>
	/// Deserializes a JSON string into an object of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize.</typeparam>
	/// <param name="json">The JSON string to deserialize from.</param>
	/// <param name="discord">The Discord client instance associated with the deserialization.</param>
	/// <returns>An instance of type <typeparamref name="T"/>.</returns>
	public static T DeserializeObject<T>(string json, BaseDiscordClient? discord) where T : ObservableApiObject
		=> DeserializeObjectInternal<T>(json, discord);

	/// <summary>
	/// Deserializes a JSON string into an object of type <typeparamref name="T"/> which implements <see cref="IEnumerable{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize.</typeparam>
	/// <param name="json">The JSON string to deserialize from.</param>
	/// <param name="discord">The Discord client instance associated with the deserialization.</param>
	/// <returns>An instance of type <typeparamref name="T"/>.</returns>
	public static T DeserializeIEnumerableObject<T>(string json, BaseDiscordClient? discord) where T : IEnumerable<ObservableApiObject>
		=> DeserializeIEnumerableObjectInternal<T>(json, discord);

	/// <summary>
	/// Populates an object with the values from a JSON node.
	/// </summary>
	/// <param name="value">The token to populate the object with.</param>
	/// <param name="target">The object to populate.</param>
	public static void PopulateObject(JToken value, object target)
	{
		using var reader = value.CreateReader();
		s_serializer.Populate(reader, target);
	}

	/// <summary>
	/// Converts a JSON token into an object of type <typeparamref name="T"/>, passing any properties through extra <see cref="Newtonsoft.Json.JsonConverter"/>s if needed.
	/// </summary>
	/// <typeparam name="T">The type to convert to.</typeparam>
	/// <param name="token">The token to convert.</param>
	/// <returns>The converted object.</returns>
	public static T ToDiscordObject<T>(this JToken token)
		=> token.ToObject<T>(s_serializer)!;

	/// <summary>
	/// Serializes an object to a JSON string.
	/// </summary>
	/// <param name="value">The object to be serialized.</param>
	/// <param name="type">The type of the object.</param>
	/// <param name="jsonSerializer">The JSON serializer to use.</param>
	/// <returns>A JSON string representation of the object.</returns>
	private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
	{
		var stringWriter = new StringWriter(new(256), CultureInfo.InvariantCulture);
		using (var jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonTextWriter, value, type);
		}

		return stringWriter.ToString();
	}

	/// <summary>
	/// Deserializes a JSON string into an object of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize.</typeparam>
	/// <param name="json">The JSON string to deserialize from.</param>
	/// <param name="discord">The associated Discord client instance.</param>
	/// <returns>An instance of type <typeparamref name="T"/>.</returns>
	private static T DeserializeObjectInternal<T>(string json, BaseDiscordClient? discord) where T : ObservableApiObject
	{
		var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
		{
			ContractResolver = new OptionalJsonContractResolver()
		})!;

		if (discord == null)
			return obj;

		obj.Discord = discord;

		if (!discord.Configuration.ReportMissingFields || !obj.AdditionalProperties.Any())
			return obj;

		var sentryMessage = "Found missing properties in api response for " + obj.GetType().Name;
		List<string> sentryFields = new();
		var values = 0;
		foreach (var ap in obj.AdditionalProperties)
		{
			values++;
			if (obj.IgnoredJsonKeys.Count != 0 && obj.IgnoredJsonKeys.Any(x => x == ap.Key))
				continue;

			if (values == 1)
				if (discord.Configuration.EnableLibraryDeveloperMode)
				{
					discord.Logger.LogInformation("{sentry}", sentryMessage);
					discord.Logger.LogDebug("{json}", json);
				}

			sentryFields.Add(ap.Key);
			if (discord.Configuration.EnableLibraryDeveloperMode)
				discord.Logger.LogInformation("Found field {field} on {object}", ap.Key, obj.GetType().Name);
		}

		if (!discord.Configuration.EnableSentry || sentryFields.Count == 0)
			return obj;

		var sentryJson = JsonConvert.SerializeObject(sentryFields);
		sentryMessage += "\n\nNew fields: " + sentryJson;
		SentryEvent sentryEvent = new()
		{
			Level = SentryLevel.Warning,
			Logger = nameof(DiscordJson),
			Message = sentryMessage
		};
		sentryEvent.SetFingerprint("{{ default }}", "{{ module }}", sentryJson.GetHashCode().ToString());
		sentryEvent.SetExtra("Found Fields", sentryJson);
		if (discord.Configuration.AttachUserInfo && discord.CurrentUser != null)
			sentryEvent.User = new()
			{
				Id = discord.CurrentUser.Id.ToString(),
				Username = discord.CurrentUser.UsernameWithDiscriminator,
				Other = new Dictionary<string, string>()
				{
					{ "developer", discord.Configuration.DeveloperUserId?.ToString() ?? "not_given" },
					{ "email", discord.Configuration.FeedbackEmail ?? "not_given" }
				}
			};
		var sid = discord.Sentry.CaptureEvent(sentryEvent);
		_ = Task.Run(discord.Sentry.FlushAsync);
		if (discord.Configuration.EnableLibraryDeveloperMode)
			discord.Logger.LogInformation("Reported to sentry with id {sid}", sid.ToString());

		return obj;
	}

	/// <summary>
	/// Deserializes a JSON string into an object of type <typeparamref name="T"/> which implements <see cref="IEnumerable{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize.</typeparam>
	/// <param name="json">The JSON string to deserialize from.</param>
	/// <param name="discord">The associated Discord client instance.</param>
	/// <returns>An instance of type <typeparamref name="T"/>.</returns>
	private static T DeserializeIEnumerableObjectInternal<T>(string json, BaseDiscordClient? discord) where T : IEnumerable<ObservableApiObject>
	{
		var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
		{
			ContractResolver = new OptionalJsonContractResolver()
		})!;

		if (discord == null)
			return obj;

		foreach (var ob in obj)
			ob.Discord = discord;

		if (!discord.Configuration.ReportMissingFields || !obj.Any(x => x.AdditionalProperties.Any()))
			return obj;

		var first = obj.First();
		var sentryMessage = "Found missing properties in api response for " + first.GetType().Name;
		List<string> sentryFields = new();
		var values = 0;
		foreach (var ap in first.AdditionalProperties)
		{
			values++;
			if (first.IgnoredJsonKeys.Count != 0 && first.IgnoredJsonKeys.Any(x => x == ap.Key))
				continue;

			if (values == 1)
				if (discord.Configuration.EnableLibraryDeveloperMode)
				{
					discord.Logger.LogInformation("{sentry}", sentryMessage);
					discord.Logger.LogDebug("{json}", json);
				}

			sentryFields.Add(ap.Key);
			if (discord.Configuration.EnableLibraryDeveloperMode)
				discord.Logger.LogInformation("Found field {field} on {object}", ap.Key, first.GetType().Name);
		}

		if (!discord.Configuration.EnableSentry || sentryFields.Count == 0)
			return obj;

		var sentryJson = JsonConvert.SerializeObject(sentryFields);
		sentryMessage += "\n\nNew fields: " + sentryJson;
		SentryEvent sentryEvent = new()
		{
			Level = SentryLevel.Warning,
			Logger = nameof(DiscordJson),
			Message = sentryMessage
		};
		sentryEvent.SetFingerprint("{{ default }}", "{{ module }}", sentryJson.GetHashCode().ToString());
		sentryEvent.SetExtra("Found Fields", sentryJson);
		if (discord.Configuration.AttachUserInfo && discord.CurrentUser != null)
			sentryEvent.User = new()
			{
				Id = discord.CurrentUser.Id.ToString(),
				Username = discord.CurrentUser.UsernameWithDiscriminator,
				Other = new Dictionary<string, string>()
				{
					{ "developer", discord.Configuration.DeveloperUserId?.ToString() ?? "not_given" },
					{ "email", discord.Configuration.FeedbackEmail ?? "not_given" }
				}
			};
		var sid = discord.Sentry.CaptureEvent(sentryEvent);
		_ = Task.Run(discord.Sentry.FlushAsync);
		if (discord.Configuration.EnableLibraryDeveloperMode)
			discord.Logger.LogInformation("Reported to sentry with id {sid}", sid.ToString());

		return obj;
	}
}
