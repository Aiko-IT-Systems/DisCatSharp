using System;
using System.IO;

using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;
using DisCatSharp.Entities;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests;

public class SurfaceRegressionTests
{
	[Fact]
	public void ApplicationChannel_IsWritableAndMovable()
	{
		var channel = new DiscordChannel { Type = ChannelType.Application };
		Assert.True(channel.IsWritable());
		Assert.True(channel.IsMovable());
		Assert.True(channel.IsMovableInParent());
	}
	[Fact]
	public void AttachmentSpoiler_CanBeRequestedAndCleared()
	{
		var attachment = new DiscordAttachment { Id = 123, IsSpoiler = true };
		Assert.True((bool)JObject.Parse(DiscordJson.SerializeObject(attachment))["is_spoiler"]!);
		attachment.IsSpoiler = false;
		Assert.False((bool)JObject.Parse(DiscordJson.SerializeObject(attachment))["is_spoiler"]!);
		attachment.IsSpoiler = null;
		Assert.Null(JObject.Parse(DiscordJson.SerializeObject(attachment))["is_spoiler"]);
		using var stream = new MemoryStream();
		var builder = new DiscordMessageBuilder().AddSpoilerFile("photo.png", stream);
		Assert.True(Assert.Single(builder.Files!).IsSpoiler);
	}
	[Fact]
	public void ComponentMediaScanAndResolvedChannelPermissions_AreDeserialized()
	{
		var media = DiscordJson.DeserializeObject<DiscordUnfurledMediaItem>("{\"url\":\"https://example.org/a.png\",\"content_scan_metadata\":{\"flags\":5}}", null);
		Assert.Equal(ContentScanFlags.Explicit | ContentScanFlags.SelfHarm, media.ContentScanMetadata!.Flags);
		var channel = DiscordJson.DeserializeObject<DiscordChannel>("{\"app_permissions\":\"1024\"}", null);
		Assert.Equal((Permissions)1024, channel.AppPermissions);
		Assert.Equal(128, (long)AttachmentFlags.ContainsSelfHarmContent);
		Assert.Equal(128, (long)EmbedFlags.ContainsSelfHarmContent);
	}

	[Fact]
	public void ComponentEmbed_DeserializesContainerLayout()
	{
		var embed = DiscordJson.DeserializeObject<DiscordEmbed>("{\"type\":\"components\",\"components\":[{\"type\":17,\"components\":[{\"type\":10,\"content\":\"hello\"}]}]}", null);
		Assert.Equal("components", embed.Type);
		Assert.IsType<DiscordContainerComponent>(Assert.Single(embed.Components!));
	}
	[Fact]
	public void GuildFeatures_RecognizeNewAndExistingSpamLinkMappings()
	{
		var guild = new DiscordGuild { RawFeatures = ["PRUNE_REQUIRES_ADMIN", "AUTOMOD_TRIGGER_SPAM_LINK_FILTER", "UNKNOWN_FUTURE_FEATURE"] };
		Assert.True(guild.Features.HasFeature(GuildFeaturesEnum.PruneRequiresAdmin));
		Assert.True(guild.Features.HasFeature(GuildFeaturesEnum.AutomodTriggerSpamLinkFilter));
		Assert.True(guild.Features.HasFeature(GuildFeaturesEnum.AutomodTriggerSpamLinkFilterGuild));
		Assert.Contains("UNKNOWN_FUTURE_FEATURE", guild.RawFeatures);
	}
	[Fact]
	public void ApplicationChannelPayload_ContainsApplicationIdOnlyWhenProvided()
	{
		var payload = new RestChannelCreatePayload { Name = "activity", Type = ChannelType.Application, ApplicationId = 123 };
		var json = JObject.Parse(DiscordJson.SerializeObject(payload));
		Assert.Equal(21, (int)json["type"]!);
		Assert.Equal(123UL, (ulong)json["application_id"]!);
		payload.ApplicationId = null;
		Assert.Null(JObject.Parse(DiscordJson.SerializeObject(payload))["application_id"]);
	}

	[Fact]
	public void ChannelApplicationId_DistinguishesOmittedAndNull()
	{
		var missing = DiscordJson.DeserializeObject<DiscordChannel>("{}", null);
		var cleared = DiscordJson.DeserializeObject<DiscordChannel>("{\"application_id\":null}", null);
		var populated = DiscordJson.DeserializeObject<DiscordChannel>("{\"application_id\":\"123\"}", null);
		Assert.False(missing.HasApplicationId);
		Assert.True(cleared.HasApplicationId);
		Assert.Null(cleared.ApplicationId);
		Assert.Equal(123UL, populated.ApplicationId);
	}
	[Fact]
	public void GuildListShard_UsesOptionalNonnegativeQueryParameter()
	{
		var url = new QueryUriBuilder("https://discord.com/api/users/@me/guilds");
		DiscordApiClient.AddGuildListShard(url, null);
		Assert.Empty(url.Build().Query);
		DiscordApiClient.AddGuildListShard(url, 3);
		Assert.Equal("?shard=3", url.Build().Query);
		Assert.Throws<ArgumentOutOfRangeException>(() => DiscordApiClient.AddGuildListShard(url, -1));
	}
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

	[Fact]
	public void GuildAndSubscriptionEnumValues_RemainStable()
	{
		Assert.Equal(171, (int)AuditLogActionType.GuildHomeFeatureItem);
		Assert.Equal(172, (int)AuditLogActionType.GuildHomeRemoveItem);
		Assert.Equal(4, (int)AutomodActionType.QuarantineUser);
		Assert.Equal(2, (int)AutomodEventType.GuildMemberEvent);
		Assert.Equal(6, (int)AutomodTriggerType.UserProfile);
		Assert.Equal(1, (int)SubscriptionStatus.Inactive);
		Assert.Equal(2, (int)SubscriptionStatus.Ending);
	}
}
