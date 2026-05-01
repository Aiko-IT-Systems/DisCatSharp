using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Serialization;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents an inline HTTP response to a Discord interaction.
/// </summary>
/// <remarks>
///     Discord requires the initial response to be returned within roughly three seconds. Use one of the deferred
///     response helpers when additional work is required, then finish the interaction through the outbound follow-up
///     webhook APIs before the interaction token expires after fifteen minutes.
/// </remarks>
public sealed class DiscordInteractionIngressResponse
{
	private DiscordInteractionIngressResponse(InteractionResponseType type, DiscordIngressResponse response)
	{
		this.Type = type;
		this.Response = response ?? throw new ArgumentNullException(nameof(response));
	}

	/// <summary>
	///     Gets the Discord interaction callback type returned by this response.
	/// </summary>
	public InteractionResponseType Type { get; }

	internal DiscordIngressResponse Response { get; }

	/// <summary>
	///     Creates the required <c>PONG</c> response for an interaction ping.
	/// </summary>
	/// <returns>The inline pong response.</returns>
	public static DiscordInteractionIngressResponse Pong()
		=> new(InteractionResponseType.Pong, DiscordIngressResponse.Json(StatusCodes.Status200OK, "{\"type\":1}"));

	/// <summary>
	///     Creates an inline interaction callback response.
	/// </summary>
	/// <param name="type">The callback type to send.</param>
	/// <param name="builder">The response builder for callback data, when required.</param>
	/// <returns>The inline interaction callback response.</returns>
	public static DiscordInteractionIngressResponse FromCallback(InteractionResponseType type, DiscordInteractionResponseBuilder? builder = null)
	{
		if (type == InteractionResponseType.Pong)
			return Pong();

		if (type is InteractionResponseType.Modal or InteractionResponseType.Iframe)
			throw new NotSupportedException("Inline modal and iframe interaction callbacks are not supported by the HTTP ingress helpers yet.");

		if (builder is null
			&& type is InteractionResponseType.ChannelMessageWithSource
				or InteractionResponseType.UpdateMessage
				or InteractionResponseType.AutoCompleteResult
				or InteractionResponseType.SocialLayerSkuPurchaseEligibility)
			throw new ArgumentNullException(nameof(builder), $"Interaction response type {type} requires a response builder.");

		var payload = new DiscordInteractionIngressCallbackPayload
		{
			Type = type,
			Data = CreateData(builder)
		};

		return new(type, DiscordIngressResponse.Json(StatusCodes.Status200OK, DiscordJson.SerializeObject(payload)));
	}

	/// <summary>
	///     Creates an inline message response for an incoming Discord interaction.
	/// </summary>
	/// <param name="builder">The message response builder.</param>
	/// <returns>The inline message response.</returns>
	public static DiscordInteractionIngressResponse ChannelMessageWithSource(DiscordInteractionResponseBuilder builder)
		=> FromCallback(InteractionResponseType.ChannelMessageWithSource, builder);

	/// <summary>
	///     Creates a deferred message response for an incoming Discord interaction.
	/// </summary>
	/// <param name="ephemeral">Whether the deferred response should be ephemeral.</param>
	/// <returns>The deferred message response.</returns>
	public static DiscordInteractionIngressResponse DeferredChannelMessageWithSource(bool ephemeral = false)
		=> FromCallback(
			InteractionResponseType.DeferredChannelMessageWithSource,
			ephemeral ? new DiscordInteractionResponseBuilder().AsEphemeral() : null);

	private static DiscordInteractionIngressCallbackData? CreateData(DiscordInteractionResponseBuilder? builder)
	{
		return builder is null
			? null
			: builder.Files is { Count: > 0 }
			? throw new NotSupportedException("Inline HTTP interaction responses do not support file uploads yet. Defer the interaction and use the outbound follow-up webhook APIs instead.")
			: builder.Attachments is { Count: > 0 }
			? throw new NotSupportedException("Inline HTTP interaction responses do not support attachment metadata yet.")
			: builder.Poll is not null
			? throw new NotSupportedException("Inline HTTP interaction responses do not support poll payloads yet.")
			: new DiscordInteractionIngressCallbackData
		{
			IsTts = builder.IsTts ? true : null,
			Content = builder.IsComponentsV2 ? null : builder.Content,
			Embeds = builder.IsComponentsV2 ? null : builder.Embeds,
			AllowedMentions = CreateAllowedMentions(builder.Mentions),
			Flags = CreateFlags(builder),
			Components = builder.Components,
			Choices = builder.Choices,
			Eligible = builder.IsEligible ? true : null
		};
	}

	private static MessageFlags? CreateFlags(DiscordInteractionResponseBuilder builder)
	{
		var flags = MessageFlags.None;
		if (builder.IsEphemeral)
			flags |= MessageFlags.Ephemeral;
		if (builder.EmbedsSuppressed && !builder.IsComponentsV2)
			flags |= MessageFlags.SuppressedEmbeds;
		if (builder.NotificationsSuppressed)
			flags |= MessageFlags.SuppressNotifications;
		if (builder.IsComponentsV2)
			flags |= MessageFlags.IsComponentsV2;

		return flags == MessageFlags.None ? null : flags;
	}

	private static DiscordInteractionIngressAllowedMentions? CreateAllowedMentions(IReadOnlyList<IMention>? mentions)
	{
		if (mentions is null)
			return null;

		if (mentions.Count == 0)
		{
			return new DiscordInteractionIngressAllowedMentions
			{
				Parse = [],
				RepliedUser = false
			};
		}

		HashSet<string> parse = [];
		HashSet<ulong> users = [];
		HashSet<ulong> roles = [];
		var repliedUser = false;

		foreach (var mention in mentions)
		{
			switch (mention)
			{
				case UserMention { Id: { } userId }:
					users.Add(userId);
					break;
				case UserMention:
					parse.Add("users");
					break;
				case RoleMention { Id: { } roleId }:
					roles.Add(roleId);
					break;
				case RoleMention:
					parse.Add("roles");
					break;
				case EveryoneMention:
					parse.Add("everyone");
					break;
				case RepliedUserMention:
					repliedUser = true;
					break;
				default:
					throw new NotSupportedException($"Mention type {mention.GetType().FullName} is not supported by the inline interaction ingress response serializer.");
			}
		}

		return new DiscordInteractionIngressAllowedMentions
		{
			Parse = parse.Count > 0 ? [.. parse] : null,
			Users = parse.Contains("users") || users.Count == 0 ? null : [.. users],
			Roles = parse.Contains("roles") || roles.Count == 0 ? null : [.. roles],
			RepliedUser = repliedUser ? true : null
		};
	}

	private sealed class DiscordInteractionIngressCallbackPayload
	{
		[JsonProperty("type")]
		public required InteractionResponseType Type { get; init; }

		[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
		public DiscordInteractionIngressCallbackData? Data { get; init; }
	}

	private sealed class DiscordInteractionIngressCallbackData
	{
		[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsTts { get; init; }

		[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
		public string? Content { get; init; }

		[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyList<DiscordEmbed>? Embeds { get; init; }

		[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
		public DiscordInteractionIngressAllowedMentions? AllowedMentions { get; init; }

		[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
		public MessageFlags? Flags { get; init; }

		[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyList<DiscordComponent>? Components { get; init; }

		[JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyList<DiscordApplicationCommandAutocompleteChoice>? Choices { get; init; }

		[JsonProperty("eligible", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Eligible { get; init; }
	}

	private sealed class DiscordInteractionIngressAllowedMentions
	{
		[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyList<ulong>? Roles { get; init; }

		[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyList<ulong>? Users { get; init; }

		[JsonProperty("parse", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyList<string>? Parse { get; init; }

		[JsonProperty("replied_user", NullValueHandling = NullValueHandling.Ignore)]
		public bool? RepliedUser { get; init; }
	}
}
