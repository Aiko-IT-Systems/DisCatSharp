using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Store;

public class StoreModelDeserializationTests
{
	[Fact]
	public void EntitlementPayload_DeserializesGuildPowerupFields()
	{
		const string json = """
		                    {
		                      "id": "1417215769351163964",
		                      "application_id": "1340102344645283891",
		                      "user_id": "856780995629154305",
		                      "type": 13,
		                      "tenant_metadata": {
		                        "guild_monetization": {
		                          "powerup": {
		                            "boost_price": 5
		                          }
		                        }
		                      },
		                      "starts_at": null,
		                      "source_type": 6,
		                      "sku_id": "1395150923734581339",
		                      "sku": {
		                        "updated_at": "2025-10-10T16:55:25.560337+02:00",
		                        "type": 2,
		                        "tenant_metadata": {
		                          "guild_monetization": {
		                            "powerup": {
		                              "static_image_url": "https://cdn.discordapp.com/assets/content/4e745042a2d8bfd0006d1040fc5a266bfb31ec9bbda28bd1651c2a48fb627642.png",
		                              "purchase_limit": 1,
		                              "guild_features": {
		                                "features": [
		                                  "GUILD_TAGS_BADGE_PACK_FLEX"
		                                ],
		                                "additional_sticker_slots": 0,
		                                "additional_sound_slots": 0,
		                                "additional_emoji_slots": 0
		                              },
		                              "category_type": "perk",
		                              "boost_price": 5,
		                              "animated_image_url": "https://cdn.discordapp.com/assets/content/20ed9f49e889cf2f2730613ab1a0724d12e8f4b84292c821b792768f0aba0ddc.png"
		                            }
		                          }
		                        },
		                        "slug": "flex-badge-pack",
		                        "selected_options": [
		                          {
		                            "option_value": "default",
		                            "option_name": "default"
		                          }
		                        ],
		                        "release_date": null,
		                        "product_line": 13,
		                        "product_id": "1422296448396951579",
		                        "premium": false,
		                        "powerup_metadata": {
		                          "static_image_url": "https://cdn.discordapp.com/assets/content/4e745042a2d8bfd0006d1040fc5a266bfb31ec9bbda28bd1651c2a48fb627642.png",
		                          "purchase_limit": 1,
		                          "guild_features": {
		                            "features": [
		                              "GUILD_TAGS_BADGE_PACK_FLEX"
		                            ],
		                            "additional_sticker_slots": 0,
		                            "additional_sound_slots": 0,
		                            "additional_emoji_slots": 0
		                          },
		                          "category_type": "perk",
		                          "boost_price": 5,
		                          "animated_image_url": "https://cdn.discordapp.com/assets/content/20ed9f49e889cf2f2730613ab1a0724d12e8f4b84292c821b792768f0aba0ddc.png"
		                        },
		                        "name": {
		                          "localizations": {
		                            "en-US": "Flex Badge Pack",
		                            "de": "Prahlerei-Abzeichenpaket"
		                          },
		                          "default": "Flex Badge Pack"
		                        },
		                        "manifest_labels": null,
		                        "id": "1395150923734581339",
		                        "flags": 0,
		                        "features": [],
		                        "dependent_sku_id": "1351706802684952639",
		                        "created_at": "2025-08-05T22:53:39.13383+02:00",
		                        "application_id": "1340102344645283891",
		                        "access_type": 1
		                      },
		                      "promotion_id": null,
		                      "guild_id": "804032421678153819",
		                      "gift_code_flags": 0,
		                      "ends_at": null,
		                      "deleted": true
		                    }
		                    """;

		var entitlement = Deserialize<DiscordEntitlement>(json);

		Assert.Equal(EntitlementType.GuildPowerup, entitlement.Type);
		Assert.Equal(EntitlementSourceType.GuildPowerup, entitlement.SourceType);
		Assert.NotNull(entitlement.TenantMetadata?.GuildMonetization?.Powerup);
		Assert.Equal(5, entitlement.TenantMetadata!.GuildMonetization!.Powerup!.BoostPrice);
		Assert.NotNull(entitlement.Sku);
		Assert.Equal(ProductLine.GuildPowerup, entitlement.Sku!.ProductLine);
		Assert.Equal((ulong)1422296448396951579, entitlement.Sku.ProductId);
		Assert.Equal("Flex Badge Pack", entitlement.Sku.Name);
		Assert.Equal("Prahlerei-Abzeichenpaket", entitlement.Sku.NameData!.Localizations["de"]);
		Assert.Equal("default", entitlement.Sku.SelectedOptions[0].OptionName);
		Assert.Equal("default", entitlement.Sku.SelectedOptions[0].OptionValue);
		Assert.Equal("perk", entitlement.Sku.PowerupMetadata!.CategoryType);
		Assert.Equal("GUILD_TAGS_BADGE_PACK_FLEX", entitlement.Sku.PowerupMetadata.GuildFeatures!.Features[0]);
		Assert.Equal(new DateTimeOffset(2025, 10, 10, 16, 55, 25, 560, TimeSpan.FromHours(2)).AddTicks(3370), entitlement.Sku.UpdatedAt);
		Assert.Equal(new DateTimeOffset(2025, 8, 5, 22, 53, 39, 133, TimeSpan.FromHours(2)).AddTicks(8300), entitlement.Sku.CreatedAt);
	}

	[Fact]
	public void SkuPayload_DeserializesPowerupMetadataAndLocalizedNames()
	{
		const string json = """
		                    {
		                      "updated_at": "2025-10-10T16:55:25.560337+02:00",
		                      "type": 2,
		                      "tenant_metadata": {
		                        "guild_monetization": {
		                          "powerup": {
		                            "boost_price": 5,
		                            "purchase_limit": 1
		                          }
		                        }
		                      },
		                      "slug": "flex-badge-pack",
		                      "selected_options": [
		                        {
		                          "option_value": "default",
		                          "option_name": "default"
		                        }
		                      ],
		                      "release_date": null,
		                      "product_line": 13,
		                      "product_id": "1422296448396951579",
		                      "premium": false,
		                      "powerup_metadata": {
		                        "static_image_url": "https://cdn.discordapp.com/assets/content/static.png",
		                        "purchase_limit": 1,
		                        "guild_features": {
		                          "features": [
		                            "GUILD_TAGS_BADGE_PACK_FLEX"
		                          ],
		                          "additional_sticker_slots": 0,
		                          "additional_sound_slots": 0,
		                          "additional_emoji_slots": 0
		                        },
		                        "category_type": "perk",
		                        "boost_price": 5,
		                        "animated_image_url": "https://cdn.discordapp.com/assets/content/animated.png"
		                      },
		                      "name": {
		                        "localizations": {
		                          "en-US": "Flex Badge Pack",
		                          "de": "Prahlerei-Abzeichenpaket"
		                        },
		                        "default": "Flex Badge Pack"
		                      },
		                      "id": "1395150923734581339",
		                      "flags": 2048,
		                      "features": [],
		                      "dependent_sku_id": "1351706802684952639",
		                      "created_at": "2025-08-05T22:53:39.13383+02:00",
		                      "application_id": "1340102344645283891",
		                      "access_type": 1
		                    }
		                    """;

		var sku = Deserialize<DiscordSku>(json);

		Assert.Equal(ProductLine.GuildPowerup, sku.ProductLine);
		Assert.Equal((ulong)1422296448396951579, sku.ProductId);
		Assert.Equal("Flex Badge Pack", sku.Name);
		Assert.Equal("Prahlerei-Abzeichenpaket", sku.NameData!.Localizations["de"]);
		Assert.Equal("default", sku.SelectedOptions[0].OptionName);
		Assert.Equal("default", sku.SelectedOptions[0].OptionValue);
		Assert.Equal(SkuFlags.AvailableForApplicationGifting, sku.Flags);
		Assert.NotNull(sku.TenantMetadata?.GuildMonetization?.Powerup);
		Assert.Equal(5, sku.TenantMetadata!.GuildMonetization!.Powerup!.BoostPrice);
		Assert.NotNull(sku.PowerupMetadata);
		Assert.Equal("perk", sku.PowerupMetadata!.CategoryType);
		Assert.Equal("https://cdn.discordapp.com/assets/content/static.png", sku.PowerupMetadata.StaticImageUrl);
		Assert.Equal("https://cdn.discordapp.com/assets/content/animated.png", sku.PowerupMetadata.AnimatedImageUrl);
		Assert.Equal("GUILD_TAGS_BADGE_PACK_FLEX", sku.PowerupMetadata.GuildFeatures!.Features[0]);
		Assert.Equal(new DateTimeOffset(2025, 10, 10, 16, 55, 25, 560, TimeSpan.FromHours(2)).AddTicks(3370), sku.UpdatedAt);
		Assert.Equal(new DateTimeOffset(2025, 8, 5, 22, 53, 39, 133, TimeSpan.FromHours(2)).AddTicks(8300), sku.CreatedAt);
	}

	[Fact]
	public void SkuPayload_DeserializesLegacyStringName()
	{
		const string json = """
		                    {
		                      "id": "1395150923734581339",
		                      "application_id": "1340102344645283891",
		                      "type": 2,
		                      "name": "Legacy Nitro Monthly",
		                      "slug": "legacy-nitro-monthly",
		                      "flags": 4,
		                      "access_type": 1,
		                      "product_line": 1,
		                      "premium": true
		                    }
		                    """;

		var sku = Deserialize<DiscordSku>(json);

		Assert.Equal("Legacy Nitro Monthly", sku.Name);
		Assert.NotNull(sku.NameData);
		Assert.Equal("Legacy Nitro Monthly", sku.NameData!.Default);
		Assert.Empty(sku.NameData.Localizations);
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
