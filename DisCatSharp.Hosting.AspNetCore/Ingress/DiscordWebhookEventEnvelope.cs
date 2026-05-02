using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using DisCatSharp.Entities;
using DisCatSharp.Net.Serialization;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a parsed inbound Discord webhook event envelope.
/// </summary>
public sealed class DiscordWebhookEventEnvelope
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordWebhookEventEnvelope" /> class.
	/// </summary>
	/// <param name="type">The envelope type discriminator.</param>
	/// <param name="payload">The raw payload bytes.</param>
	/// <param name="root">The parsed JSON root element.</param>
	/// <param name="version">The webhook envelope version when present.</param>
	/// <param name="applicationId">The application identifier when present.</param>
	/// <param name="eventType">The event type when present.</param>
	/// <param name="timestampRaw">The event timestamp when present.</param>
	/// <param name="eventBody">The raw nested event object when present.</param>
	/// <param name="data">The raw nested event data object when present.</param>
	public DiscordWebhookEventEnvelope(
		int type,
		DiscordIngressPayload payload,
		JsonElement root,
		int? version = null,
		string? applicationId = null,
		string? eventType = null,
		string? timestampRaw = null,
		JsonElement? eventBody = null,
		JsonElement? data = null
	)
	{
		ArgumentNullException.ThrowIfNull(payload);

		if (root.ValueKind != JsonValueKind.Object)
			throw new ArgumentException("The webhook event envelope root must be a JSON object.", nameof(root));

		this.Type = type;
		this.Payload = payload;
		this.Root = root;
		this.Version = version;
		this.ApplicationId = applicationId;
		this.EventType = eventType;
		this.TimestampRaw = timestampRaw;
		this.Event = eventBody ?? default;
		this.Data = data ?? default;
	}

	/// <summary>
	///     Gets the webhook envelope type discriminator.
	/// </summary>
	public int Type { get; }

	/// <summary>
	///     Gets a value indicating whether the envelope represents a Discord webhook ping.
	/// </summary>
	[Newtonsoft.Json.JsonIgnore]
	public bool IsPing
		=> this.Type is DiscordWebhookEventTypes.Ping;

	/// <summary>
	///     Gets a value indicating whether the envelope represents a Discord webhook event.
	/// </summary>
	[Newtonsoft.Json.JsonIgnore]
	public bool IsEvent
		=> this.Type is DiscordWebhookEventTypes.Event;

	/// <summary>
	///     Gets the raw request payload.
	/// </summary>
	public DiscordIngressPayload Payload { get; }

	/// <summary>
	///     Gets the parsed JSON root object.
	/// </summary>
	public JsonElement Root { get; }

	/// <summary>
	///     Gets the webhook envelope version when present on the payload.
	/// </summary>
	public int? Version { get; }

	/// <summary>
	///     Gets the application identifier when present on the payload.
	/// </summary>
	public string? ApplicationId { get; }

	/// <summary>
	///     Gets the event type when present on the payload.
	/// </summary>
	public string? EventType { get; }

	/// <summary>
	///     Gets the raw nested event object when present on the payload.
	/// </summary>
	public JsonElement Event { get; }

	/// <summary>
	///     Gets a value indicating whether the payload included a nested event object.
	/// </summary>
	[Newtonsoft.Json.JsonIgnore]
	public bool HasEvent
		=> this.Event.ValueKind is not JsonValueKind.Undefined && this.Event.ValueKind is not JsonValueKind.Null;

	/// <summary>
	///     Gets the raw event timestamp when present on the payload.
	/// </summary>
	public string? TimestampRaw { get; }

	/// <summary>
	///     Gets the parsed event timestamp when present on the payload.
	/// </summary>
	[Newtonsoft.Json.JsonIgnore]
	public DateTimeOffset? Timestamp
		=> !string.IsNullOrWhiteSpace(this.TimestampRaw) && DateTimeOffset.TryParse(this.TimestampRaw, out var timestamp) ? timestamp : null;

	/// <summary>
	///     Gets the raw nested event data object when present on the payload.
	/// </summary>
	public JsonElement Data { get; }

	/// <summary>
	///     Gets a value indicating whether the payload included nested event data.
	/// </summary>
	[Newtonsoft.Json.JsonIgnore]
	public bool HasData
		=> this.Data.ValueKind is not JsonValueKind.Undefined && this.Data.ValueKind is not JsonValueKind.Null;

	/// <summary>
	///     Gets the recommended CLR model for the current <see cref="EventType" />, when known.
	/// </summary>
	[Newtonsoft.Json.JsonIgnore]
	public Type? SuggestedDataModelType
		=> DiscordWebhookEventModelRegistry.GetPayloadType(this.EventType);

	/// <summary>
	///     Attempts to resolve a top-level property from the envelope.
	/// </summary>
	/// <param name="name">The property name to resolve.</param>
	/// <param name="value">The resolved JSON value.</param>
	/// <returns><see langword="true" /> when the property exists.</returns>
	public bool TryGetProperty(string name, out JsonElement value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return this.Root.TryGetProperty(name, out value);
	}

	/// <summary>
	///     Attempts to resolve a property from the nested event object.
	/// </summary>
	/// <param name="name">The property name to resolve.</param>
	/// <param name="value">The resolved JSON value.</param>
	/// <returns><see langword="true" /> when the property exists on the nested event object.</returns>
	public bool TryGetEventProperty(string name, out JsonElement value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		value = default;
		return this.HasEvent && this.Event.TryGetProperty(name, out value);
	}

	/// <summary>
	///     Attempts to resolve a property from the nested event data object.
	/// </summary>
	/// <param name="name">The property name to resolve.</param>
	/// <param name="value">The resolved JSON value.</param>
	/// <returns><see langword="true" /> when the property exists on the nested event data object.</returns>
	public bool TryGetDataProperty(string name, out JsonElement value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		value = default;
		return this.HasData && this.Data.TryGetProperty(name, out value);
	}

	/// <summary>
	///     Deserializes the nested event data into a typed model.
	/// </summary>
	/// <typeparam name="T">The payload model type.</typeparam>
	/// <returns>The deserialized payload model.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the envelope does not contain nested event data.</exception>
	public T DeserializeData<T>() where T : ObservableApiObject
	{
		return !this.HasData
			? throw new InvalidOperationException("The webhook event envelope does not contain nested event data.")
			: DiscordJson.DeserializeObject<T>(this.Data.GetRawText(), null);
	}

	/// <summary>
	///     Attempts to deserialize the nested event data into a typed model.
	/// </summary>
	/// <typeparam name="T">The payload model type.</typeparam>
	/// <param name="data">The deserialized payload model when successful.</param>
	/// <returns><see langword="true" /> when the payload was deserialized successfully.</returns>
	public bool TryDeserializeData<T>([NotNullWhen(true)] out T? data) where T : ObservableApiObject
	{
		try
		{
			data = this.HasData ? this.DeserializeData<T>() : null;
			return data is not null;
		}
		catch (Newtonsoft.Json.JsonException)
		{
			data = null;
			return false;
		}
	}
}
