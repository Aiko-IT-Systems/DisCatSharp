using DisCatSharp.Enums;
using DisCatSharp.Net;

using Xunit;

namespace DisCatSharp.Copilot.Tests;

public class SurfaceRegressionTests
{
	[Fact]
	public void StoreAndApplicationEnumValues_RemainStable()
	{
		Assert.Equal(13, (int)EntitlementType.GuildPowerup);
		Assert.Equal(6, (int)EntitlementSourceType.GuildPowerup);
		Assert.Equal(13, (int)ProductLine.GuildPowerup);
		Assert.Equal(1L << 11, (long)SkuFlags.AvailableForApplicationGifting);
		Assert.Equal(1L << 33, (long)ApplicationFlags.Parent);
		Assert.True((SkuFlags.AvailableForApplicationGifting | SkuFlags.Available).HasSkuFlag(SkuFlags.AvailableForApplicationGifting));
	}

	[Fact]
	public void OAuthResolveScopes_ReturnsExpectedNewScopeStrings()
	{
		Assert.Equal("application_identities.write", OAuth.ResolveScopes(OAuthScopes.APPLICATION_IDENTITIES_WRITE));
		Assert.Equal("sdk.social_layer_presence", OAuth.ResolveScopes(OAuthScopes.SDK_SOCIAL_LAYER_PRESENCE));
		Assert.Equal("openid", OAuth.ResolveScopes(OAuthScopes.OPENID));
		Assert.Equal("sdk.social_layer", OAuth.ResolveScopes(OAuthScopes.SDK_SOCIAL_LAYER));
		Assert.Equal("openid sdk.social_layer_presence", OAuth.ResolveScopes(OAuthScopes.SDK_DEFAULT_PRESENCE));
		Assert.Equal("openid sdk.social_layer", OAuth.ResolveScopes(OAuthScopes.SDK_DEFAULT_COMMUNICATION));
	}

	[Fact]
	public void ConstantsAndMessageTypes_RemainStable()
	{
		Assert.Equal(64, (int)MessageType.PremiumGroupInvite);
		Assert.Equal("/channels/@me/dms", Endpoints.ME_DMS);
	}
}
