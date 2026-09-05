using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Application;

public class ApplicationModelDeserializationTests
{
	[Fact]
	public void TransportApplicationPayload_DeserializesNewFields()
	{
		const string json = """
		                    {
		                      "id": "891436243903728565",
		                      "name": "Wordle",
		                      "icon": "546242649e3b09a97af7e8f29983837b",
		                      "description": "Official Wordle",
		                      "summary": "",
		                      "owner": {
		                        "id": "1110738998453837384",
		                        "username": "owner",
		                        "discriminator": "0001",
		                        "avatar": null
		                      },
		                      "is_discoverable": true,
		                      "max_participants": -1,
		                      "approximate_user_authorization_count": 42,
		                      "bot_approximate_guild_count": 100,
		                      "approved_consoles": [1, 2, 3],
		                      "pricing_localization_strategy": "localized_price_sets"
		                    }
		                    """;

		var application = Deserialize<TransportApplication>(json);

		Assert.True(application.IsDiscoverable);
		Assert.Equal(-1, application.MaxParticipants);
		Assert.Equal(42, application.ApproximateUserAuthorizationCount);
		Assert.Equal(100, application.BotApproximateGuildCount);
		Assert.Equal(
			[
				ApprovableConsoleType.Xbox,
				ApprovableConsoleType.PlayStation5,
				ApprovableConsoleType.PlayStation4
			],
			application.ApprovedConsoles);
		Assert.Equal("localized_price_sets", application.PricingLocalizationStrategy);
	}

	[Fact]
	public void DiscordApplicationPayload_DeserializesNewFields()
	{
		const string json = """
		                    {
		                      "id": "891436243903728565",
		                      "name": "Wordle",
		                      "icon": "546242649e3b09a97af7e8f29983837b",
		                      "description": "Official Wordle",
		                      "is_discoverable": true,
		                      "max_participants": null,
		                      "approximate_user_authorization_count": 42,
		                      "bot_approximate_guild_count": 100,
		                      "approved_consoles": [1, 2],
		                      "pricing_localization_strategy": "localized_price_sets"
		                    }
		                    """;

		var application = Deserialize<TransportApplication>(json);

		Assert.True(application.IsDiscoverable);
		Assert.Null(application.MaxParticipants);
		Assert.Equal(42, application.ApproximateUserAuthorizationCount);
		Assert.Equal(100, application.BotApproximateGuildCount);
		Assert.Equal(
			[
				ApprovableConsoleType.Xbox,
				ApprovableConsoleType.PlayStation5
			],
			application.ApprovedConsoles);
		Assert.Equal("localized_price_sets", application.PricingLocalizationStrategy);
	}

	[Fact]
	public void DiscordApplicationPayload_DeserializesReviewAndEligibleOAuth2Scopes()
	{
		const string json = """
		                    {
		                      "id": "891436243903728565",
		                      "name": "Wordle",
		                      "icon": null,
		                      "description": "Official Wordle",
		                      "owner": {
		                        "id": "1110738998453837384",
		                        "username": "owner",
		                        "discriminator": "0001",
		                        "avatar": null
		                      },
		                      "privileged_intents_review": {
		                        "limited_intents_threshold_exceeded_at": "2026-08-13T18:01:28.637384+00:00",
		                        "limited_intents_revocation_deadline": "2026-11-11T18:01:28.637384+00:00",
		                        "has_submitted": false
		                      },
		                      "eligible_oauth2_scopes": ["bot", "applications.commands"]
		                    }
		                    """;

		var application = new DiscordApplication(Deserialize<TransportApplication>(json));

		Assert.NotNull(application.PrivilegedIntentsReview);
		Assert.False(application.PrivilegedIntentsReview.HasSubmitted);
		Assert.Equal(new DateTimeOffset(2026, 8, 13, 18, 1, 28, 637, TimeSpan.Zero).AddTicks(3840), application.PrivilegedIntentsReview.LimitedIntentsThresholdExceededAt);
		Assert.Equal(new DateTimeOffset(2026, 11, 11, 18, 1, 28, 637, TimeSpan.Zero).AddTicks(3840), application.PrivilegedIntentsReview.LimitedIntentsRevocationDeadline);
		Assert.Equal(["bot", "applications.commands"], application.EligibleOAuth2Scopes);
	}

	[Fact]
	public void DiscordUserPayload_MapsVadColorIntegersToDiscordColors()
	{
		const string json = """
		                    {
		                      "id": "1110738998453837384",
		                      "username": "user",
		                      "discriminator": "0001",
		                      "avatar": null,
		                      "vad_colors": [16711680, 65280, 255]
		                    }
		                    """;

		var user = new DiscordUser(Deserialize<TransportUser>(json));

		Assert.Equal([16711680, 65280, 255], user.VadColorsInternal);
		Assert.Equal(["#FF0000", "#00FF00", "#0000FF"], user.VadColors?.Select(color => color.ToString()));
	}

	private static T Deserialize<T>(string json)
	{
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new DisCatSharpContractResolver()
		};

		return JsonConvert.DeserializeObject<T>(json, settings) ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}.");
	}
}
