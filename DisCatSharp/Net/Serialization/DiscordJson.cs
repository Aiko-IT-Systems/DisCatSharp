// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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
	private static readonly JsonSerializer s_serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
	{
		ContractResolver = new OptionalJsonContractResolver()
	});

	/// <summary>Serializes the specified object to a JSON string.</summary>
	/// <param name="value">The object to serialize.</param>
	/// <returns>A JSON string representation of the object.</returns>
	public static string SerializeObject(object value)
		=> SerializeObjectInternal(value, null, s_serializer);

	public static T DeserializeObject<T>(string json, BaseDiscordClient discord) where T : ObservableApiObject
		=> DeserializeObjectInternal<T>(json, discord);

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
		=> token.ToObject<T>(s_serializer);

	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="type">The type.</param>
	/// <param name="jsonSerializer">The json serializer.</param>
	private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
	{
		var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
		using (var jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonTextWriter, value, type);
		}
		return stringWriter.ToString();
	}

	private static T DeserializeObjectInternal<T>(string json, BaseDiscordClient discord) where T : ObservableApiObject
	{
		var obj = JsonConvert.DeserializeObject<T>(json);
		obj.Discord = discord;

		if (discord.Configuration.ReportMissingFields && obj.AdditionalProperties.Any())
		{
			var sentryMessage = "Found missing properties in api response for " + obj.GetType().Name;
			List<string> sentryFields = new();
			discord.Logger.LogInformation(sentryMessage);
			foreach (var ap in obj.AdditionalProperties)
			{
				sentryFields.Add(ap.Key);
				discord.Logger.LogInformation("Found field {field} on {object}", ap.Key, obj.GetType().Name);
			}

			if (discord.Configuration.EnableSentry)
			{
				var sentryJson = JsonConvert.SerializeObject(sentryFields);
				sentryMessage += "\n\nNew fields: " + sentryJson;
				SentryEvent sevnt = new()
				{
					Level = SentryLevel.Warning,
					Logger = nameof(DiscordJson),
					Message = sentryMessage
				};
				sevnt.SetExtra("Found Fields", sentryJson);
				var sid = SentrySdk.CaptureEvent(sevnt);
				discord.Logger.LogInformation("Reported to sentry with id {sid}", sid.ToString());
			}
		}

		return obj;
	}
}
