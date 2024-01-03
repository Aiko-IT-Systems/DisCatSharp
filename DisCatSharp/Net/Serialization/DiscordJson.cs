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
/// Represents discord json.
/// </summary>
public static class DiscordJson
{
	/// <summary>
	/// Gets the serializer.
	/// </summary>
	private static readonly JsonSerializer s_serializer = JsonSerializer.CreateDefault(new()
	{
		ContractResolver = new OptionalJsonContractResolver()
	});

	/// <summary>Serializes the specified object to a JSON string.</summary>
	/// <param name="value">The object to serialize.</param>
	/// <returns>A JSON string representation of the object.</returns>
	public static string SerializeObject(object value)
		=> SerializeObjectInternal(value, null!, s_serializer);

	/// <summary>
	/// Deserializes the specified JSON string to an object.
	/// </summary>
	/// <typeparam name="T">The type</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	public static T DeserializeObject<T>(string? json, BaseDiscordClient? discord) where T : ObservableApiObject
		=> DeserializeObjectInternal<T>(json, discord);

	/// <summary>
	/// Deserializes the specified JSON string to an object of the type <see cref="IEnumerable{T}"/>.
	/// </summary>
	/// <typeparam name="T">The enumerable type.</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	public static T DeserializeIEnumerableObject<T>(string? json, BaseDiscordClient? discord) where T : IEnumerable<ObservableApiObject>
		=> DeserializeIEnumerableObjectInternal<T>(json, discord);

	/// <summary>Populates an object with the values from a JSON node.</summary>
	/// <param name="value">The token to populate the object with.</param>
	/// <param name="target">The object to populate.</param>
	public static void PopulateObject(JToken value, object target)
	{
		using var reader = value.CreateReader();
		s_serializer.Populate(reader, target);
	}

	/// <summary>
	/// Converts this token into an object, passing any properties through extra <see cref="Newtonsoft.Json.JsonConverter"/>s if needed.
	/// </summary>
	/// <param name="token">The token to convert</param>
	/// <typeparam name="T">Type to convert to</typeparam>
	/// <returns>The converted token</returns>
	public static T ToDiscordObject<T>(this JToken token)
		=> token.ToObject<T>(s_serializer)!;

	/// <summary>
	/// Serializes the object.
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
	/// Deserializes the specified JSON string to an object.
	/// </summary>
	/// <typeparam name="T">The type</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	private static T DeserializeObjectInternal<T>(string? json, BaseDiscordClient? discord) where T : ObservableApiObject
	{
		ArgumentNullException.ThrowIfNull(json, nameof(json));

		var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
		{
			ContractResolver = new OptionalJsonContractResolver()
		})!;
		if (discord is null)
			return obj;

		obj.Discord = discord;

		if (!discord.Configuration.ReportMissingFields || !obj.AdditionalProperties.Any())
			return obj;

		var sentryMessage = "Found missing properties in api response for " + obj.GetType().Name;
		List<string> sentryFields = [];
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

			sentryFields.Add(ap.Key);
			if (discord.Configuration.EnableLibraryDeveloperMode)
				discord.Logger.LogInformation("Found field {field} on {object}", ap.Key, obj.GetType().Name);
		}

		if (!discord.Configuration.EnableSentry || sentryFields.Count is 0)
			return obj;

		var sentryJson = JsonConvert.SerializeObject(sentryFields);
		sentryMessage += "\n\nNew fields: " + sentryJson;
		SentryEvent sentryEvent = new()
		{
			Level = SentryLevel.Warning,
			Logger = nameof(DiscordJson),
			Message = sentryMessage
		};
		sentryEvent.SetFingerprint(BaseDiscordClient.GenerateSentryFingerPrint(sentryEvent));
		sentryEvent.SetExtra("Found Fields", sentryJson);
		if (discord.Configuration.AttachUserInfo && discord.CurrentUser is not null)
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
	/// Deserializes the specified JSON string to an object of the type <see cref="IEnumerable{T}"/>.
	/// </summary>
	/// <typeparam name="T">The enumerable type.</typeparam>
	/// <param name="json">The received json.</param>
	/// <param name="discord">The discord client.</param>
	private static T DeserializeIEnumerableObjectInternal<T>(string? json, BaseDiscordClient? discord) where T : IEnumerable<ObservableApiObject>
	{
		ArgumentNullException.ThrowIfNull(json, nameof(json));

		var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
		{
			ContractResolver = new OptionalJsonContractResolver()
		})!;
		if (discord is null)
			return obj;

		foreach (var ob in obj)
			ob.Discord = discord;

		if (!discord.Configuration.ReportMissingFields || !obj.Any(x => x.AdditionalProperties.Any()))
			return obj;

		var first = obj.First();
		var sentryMessage = "Found missing properties in api response for " + first.GetType().Name;
		List<string> sentryFields = [];
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
		sentryEvent.SetFingerprint(BaseDiscordClient.GenerateSentryFingerPrint(sentryEvent));
		sentryEvent.SetExtra("Found Fields", sentryJson);
		if (discord.Configuration.AttachUserInfo && discord.CurrentUser is not null)
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
