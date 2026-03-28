using DisCatSharp;

using Xunit;

namespace DisCatSharp.CopilotTests.Utilities;

public class UtilitiesScrubbingTests
{
	[Fact]
	public void StripTokensAndOptIdsInJson_ReplacesEmbeddedDiscordIdsInObjectsAndArrays()
	{
		const string json = """
		                    {
		                      "roles": [
		                        "1480674874463752384",
		                        "804032421757976697"
		                      ],
		                      "guild_id": 804032421678153819,
		                      "user": {
		                        "id": "773493116404629504"
		                      },
		                      "label": "not-an-id"
		                    }
		                    """;

		var scrubbed = DisCatSharp.Utilities.StripTokensAndOptIdsInJson(json, true);

		Assert.NotNull(scrubbed);
		Assert.DoesNotContain("1480674874463752384", scrubbed);
		Assert.DoesNotContain("804032421757976697", scrubbed);
		Assert.DoesNotContain("804032421678153819", scrubbed);
		Assert.DoesNotContain("773493116404629504", scrubbed);
		Assert.Contains("{DISCORD_ID}", scrubbed);
		Assert.Contains("not-an-id", scrubbed);
	}

	[Fact]
	public void StripTokensAndOptIdsInJson_StripsTokensBeforeProcessingJsonIds()
	{
		const string json = """
		                    {
		                      "token": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdef",
		                      "guild_id": 804032421678153819
		                    }
		                    """;

		var scrubbed = DisCatSharp.Utilities.StripTokensAndOptIdsInJson(json, true);

		Assert.NotNull(scrubbed);
		Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdef", scrubbed);
		Assert.DoesNotContain("804032421678153819", scrubbed);
		Assert.Contains("{WEBHOOK_OR_INTERACTION_TOKEN}", scrubbed);
		Assert.Contains("{DISCORD_ID}", scrubbed);
	}

	[Fact]
	public void StripTokensAndOptIdsInJson_StripsTokensEvenWhenIdScrubbingIsDisabled()
	{
		const string json = """
		                    {
		                      "token": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdef",
		                      "guild_id": 804032421678153819
		                    }
		                    """;

		var scrubbed = DisCatSharp.Utilities.StripTokensAndOptIdsInJson(json, false);

		Assert.NotNull(scrubbed);
		Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdef", scrubbed);
		Assert.Contains("{WEBHOOK_OR_INTERACTION_TOKEN}", scrubbed);
		Assert.Contains("804032421678153819", scrubbed);
	}

	[Fact]
	public void StripTokensAndOptIdsInJson_ReturnsOriginalStringWhenNoDiscordIdsExist()
	{
		const string json = """
		                    {
		                      "name": "bubble_gum",
		                      "label": "COLLECTIBLES_NAMEPLATES_ANGELS_A11Y",
		                      "asset": "nameplates/nameplates/angels/"
		                    }
		                    """;

		var scrubbed = DisCatSharp.Utilities.StripTokensAndOptIdsInJson(json, true);

		Assert.Equal(json, scrubbed);
	}
}
