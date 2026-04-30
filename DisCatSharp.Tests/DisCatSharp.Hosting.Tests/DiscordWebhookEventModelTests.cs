using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Hosting.AspNetCore.Ingress;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public sealed class DiscordWebhookEventModelTests
{
	[Fact]
	public void WebhookEventEnvelopeParser_NormalizesDocumentedEnvelopeShape()
	{
		const string body = """
		                    {
		                      "version": 1,
		                      "application_id": "1234560123453231555",
		                      "type": 1,
		                      "event": {
		                        "type": "APPLICATION_AUTHORIZED",
		                        "timestamp": "2024-10-18T14:42:53.064834",
		                        "data": {
		                          "integration_type": 1,
		                          "scopes": ["applications.commands"],
		                          "user": {
		                            "id": "111178765189277770",
		                            "username": "lala",
		                            "discriminator": "0",
		                            "avatar": null
		                          }
		                        }
		                      }
		                    }
		                    """;

		var result = DiscordWebhookEventEnvelopeParser.Parse(DiscordIngressPayload.FromString(body));

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Envelope);
		Assert.True(result.Envelope.IsEvent);
		Assert.Equal(DiscordWebhookEventTypes.Event, result.Envelope.Type);
		Assert.Equal(1, result.Envelope.Version);
		Assert.Equal("1234560123453231555", result.Envelope.ApplicationId);
		Assert.Equal(DiscordWebhookEventNames.ApplicationAuthorized, result.Envelope.EventType);
		Assert.Equal(DateTimeOffset.Parse("2024-10-18T14:42:53.064834"), result.Envelope.Timestamp);
		Assert.True(result.Envelope.HasEvent);
		Assert.True(result.Envelope.HasData);
		Assert.Equal(typeof(DiscordWebhookApplicationAuthorizedEventData), result.Envelope.SuggestedDataModelType);
	}

	[Fact]
	public void WebhookEventEnvelope_CanDeserializeApplicationAuthorizationData()
	{
		const string body = """
		                    {
		                      "version": 1,
		                      "application_id": "1234560123453231555",
		                      "type": 1,
		                      "event": {
		                        "type": "APPLICATION_AUTHORIZED",
		                        "timestamp": "2024-10-18T14:42:53.064834",
		                        "data": {
		                          "integration_type": 0,
		                          "scopes": ["applications.commands", "bot"],
		                          "user": {
		                            "id": "111178765189277770",
		                            "username": "lala",
		                            "discriminator": "0",
		                            "avatar": null
		                          },
		                          "guild": {
		                            "id": "123456789012345678",
		                            "name": "Cat Cafe",
		                            "icon": "abc123"
		                          }
		                        }
		                      }
		                    }
		                    """;

		var envelope = DiscordWebhookEventEnvelopeParser.Parse(DiscordIngressPayload.FromString(body)).Envelope!;

		Assert.True(envelope.TryDeserializeData<DiscordWebhookApplicationAuthorizedEventData>(out var data));
		Assert.NotNull(data);
		Assert.Equal(ApplicationCommandIntegrationTypes.GuildInstall, data.IntegrationType);
		Assert.Equal(2, data.Scopes.Count);
		Assert.Equal((ulong)111178765189277770, data.User!.Id);
		Assert.Equal((ulong)123456789012345678, data.Guild!.Id);
		Assert.Equal("Cat Cafe", data.Guild.Name);
	}

	[Fact]
	public void WebhookEventEnvelope_CanDeserializeReusableAndIngressOnlyPayloadModels()
	{
		const string entitlementBody = """
		                               {
		                                 "version": 1,
		                                 "application_id": "1234560123453231555",
		                                 "type": 1,
		                                 "event": {
		                                   "type": "ENTITLEMENT_DELETE",
		                                   "timestamp": "2024-10-18T18:41:21.109604",
		                                   "data": {
		                                     "application_id": "1234560123453231555",
		                                     "deleted": true,
		                                     "id": "1234505980407808808",
		                                     "sku_id": "123489045643835123",
		                                     "type": 4,
		                                     "user_id": "111178765189277770"
		                                   }
		                                 }
		                               }
		                               """;
		const string lobbyBody = """
		                         {
		                           "version": 1,
		                           "application_id": "1234567765431056709",
		                           "type": 1,
		                           "event": {
		                             "type": "LOBBY_MESSAGE_UPDATE",
		                             "timestamp": "2025-08-05T20:39:19.587872",
		                             "data": {
		                               "id": "1402390388030832792",
		                               "type": 0,
		                               "content": "noice",
		                               "lobby_id": "1402385687281537066",
		                               "channel_id": "1402389638311841883",
		                               "author": {
		                                 "id": "111178765189277770",
		                                 "username": "lala",
		                                 "discriminator": "0",
		                                 "avatar": null
		                               },
		                               "metadata": {
		                                 "origin": "linked-channel"
		                               },
		                               "edited_timestamp": "2025-08-05T20:39:19.557970+00:00",
		                               "flags": 0,
		                               "timestamp": "2025-08-05T20:38:43.660000+00:00"
		                             }
		                           }
		                         }
		                         """;
		const string gameDmBody = """
		                          {
		                            "version": 1,
		                            "application_id": "1234567765431056709",
		                            "type": 1,
		                            "event": {
		                              "type": "GAME_DIRECT_MESSAGE_UPDATE",
		                              "timestamp": "2025-08-14T16:44:31.847073",
		                              "data": {
		                                "id": "1405591838810706081",
		                                "content": "almost ready to queue?",
		                                "channel_id": "1404960877324533784",
		                                "author": {
		                                  "id": "111178765189277770",
		                                  "username": "lala",
		                                  "discriminator": "0",
		                                  "avatar": null
		                                },
		                                "recipient_id": "1404960877324533784"
		                              }
		                            }
		                          }
		                          """;

		var entitlementEnvelope = DiscordWebhookEventEnvelopeParser.Parse(DiscordIngressPayload.FromString(entitlementBody)).Envelope!;
		var lobbyEnvelope = DiscordWebhookEventEnvelopeParser.Parse(DiscordIngressPayload.FromString(lobbyBody)).Envelope!;
		var gameDmEnvelope = DiscordWebhookEventEnvelopeParser.Parse(DiscordIngressPayload.FromString(gameDmBody)).Envelope!;

		Assert.True(entitlementEnvelope.TryDeserializeData<DiscordEntitlement>(out var entitlement));
		Assert.True(lobbyEnvelope.TryDeserializeData<DiscordWebhookLobbyMessageEventData>(out var lobbyMessage));
		Assert.True(gameDmEnvelope.TryDeserializeData<DiscordWebhookGameDirectMessageEventData>(out var gameDirectMessage));

		Assert.True(entitlement!.Deleted);
		Assert.Equal((ulong)1234505980407808808, entitlement.Id);
		Assert.Equal((ulong)1402385687281537066, lobbyMessage!.LobbyId);
		Assert.Equal("linked-channel", lobbyMessage.Metadata["origin"]);
		Assert.Equal(DateTimeOffset.Parse("2025-08-05T20:38:43.660000+00:00"), lobbyMessage.Timestamp);
		Assert.Equal((ulong)1404960877324533784, gameDirectMessage!.RecipientId);
	}

	[Fact]
	public void WebhookEventModelRegistry_LeavesUnknownAndUnavailableContractsOnRawFallback()
	{
		Assert.Null(DiscordWebhookEventModelRegistry.GetPayloadType(DiscordWebhookEventNames.QuestUserEnrollment));
		Assert.Null(DiscordWebhookEventModelRegistry.GetPayloadType("SOMETHING_NEW"));
		Assert.Equal(typeof(DiscordWebhookLobbyMessageDeleteEventData), DiscordWebhookEventModelRegistry.GetPayloadType(DiscordWebhookEventNames.LobbyMessageDelete));
	}
}
