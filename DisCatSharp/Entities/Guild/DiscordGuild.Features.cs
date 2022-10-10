// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
using System.Linq;
using System.Text;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

public class GuildFeatures
{
	public List<GuildFeaturesEnum> Features { get; }

	public GuildFeatures(DiscordGuild guild)
	{
		this.Features = new List<GuildFeaturesEnum>();

		if (guild.RawFeatures.Contains("ANIMATED_ICON")) this.Features.Add(GuildFeaturesEnum.CanSetAnimatedIcon);
		if (guild.RawFeatures.Contains("ANIMATED_BANNER")) this.Features.Add(GuildFeaturesEnum.CanSetAnimatedBanner);
		if (guild.RawFeatures.Contains("BANNER")) this.Features.Add(GuildFeaturesEnum.CanSetBanner);
		if (guild.RawFeatures.Contains("CHANNEL_BANNER")) this.Features.Add(GuildFeaturesEnum.CanSetChannelBanner);
		if (guild.RawFeatures.Contains("COMMUNITY")) this.Features.Add(GuildFeaturesEnum.HasCommunityEnabled);
		if (!guild.RawFeatures.Contains("DISCOVERABLE_DISABLED") && guild.RawFeatures.Contains("DISCOVERABLE")) this.Features.Add(GuildFeaturesEnum.IsDiscoverable);
		if (guild.RawFeatures.Contains("FEATUREABLE")) this.Features.Add(GuildFeaturesEnum.IsFeatureable);
		if (guild.RawFeatures.Contains("INVITE_SPLASH")) this.Features.Add(GuildFeaturesEnum.CanSetInviteSplash);
		if (guild.RawFeatures.Contains("MEMBER_VERIFICATION_GATE_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasMembershipScreeningEnabled);
		if (guild.RawFeatures.Contains("NEWS")) this.Features.Add(GuildFeaturesEnum.CanCreateNewsChannels);
		if (guild.RawFeatures.Contains("PARTNERED")) this.Features.Add(GuildFeaturesEnum.IsPartnered);
		if (guild.RawFeatures.Contains("MORE_EMOJI")) this.Features.Add(GuildFeaturesEnum.CanUploadMoreEmojis);
		if (guild.RawFeatures.Contains("PREVIEW_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasPreviewEnabled);
		if (guild.RawFeatures.Contains("VANITY_URL")) this.Features.Add(GuildFeaturesEnum.CanSetVanityUrl);
		if (guild.RawFeatures.Contains("VERIFIED")) this.Features.Add(GuildFeaturesEnum.IsVerified);
		if (guild.RawFeatures.Contains("VIP_REGIONS")) this.Features.Add(GuildFeaturesEnum.CanAccessVipRegions);
		if (guild.RawFeatures.Contains("WELCOME_SCREEN_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasWelcomeScreenEnabled);
		if (guild.RawFeatures.Contains("TICKETED_EVENTS_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasTicketedEventsEnabled);
		if (guild.RawFeatures.Contains("MONETIZATION_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasMonetizationEnabled);
		if (guild.RawFeatures.Contains("MORE_STICKERS")) this.Features.Add(GuildFeaturesEnum.CanUploadMoreStickers);
		if (guild.RawFeatures.Contains("PRIVATE_THREADS")) this.Features.Add(GuildFeaturesEnum.CanCreatePrivateThreads);
		if (guild.RawFeatures.Contains("HUB")) this.Features.Add(GuildFeaturesEnum.IsHub);
		if (guild.RawFeatures.Contains("THREADS_ENABLED_TESTING")) this.Features.Add(GuildFeaturesEnum.HasThreadTestingEnabled);
		if (guild.RawFeatures.Contains("THREADS_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasThreadsEnabled);
		if (guild.RawFeatures.Contains("ROLE_ICONS")) this.Features.Add(GuildFeaturesEnum.CanSetRoleIcons);
		if (guild.RawFeatures.Contains("NEW_THREAD_PERMISSIONS")) this.Features.Add(GuildFeaturesEnum.HasNewThreadPermissions);
		if (guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasRoleSubscriptionsEnabled);
		if (guild.RawFeatures.Contains("PREMIUM_TIER_3_OVERRIDE")) this.Features.Add(GuildFeaturesEnum.PremiumTierThreeOverride);
		if (guild.RawFeatures.Contains("THREAD_DEFAULT_AUTO_ARCHIVE_DURATION")) this.Features.Add(GuildFeaturesEnum.CanSetThreadDefaultAutoArchiveDuration);
		if (guild.RawFeatures.Contains("TEXT_IN_VOICE_ENABLED")) this.Features.Add(GuildFeaturesEnum.TextInVoiceEnabled);
		if (guild.RawFeatures.Contains("HAS_DIRECTORY_ENTRY")) this.Features.Add(GuildFeaturesEnum.HasDirectoryEntry);
		if (guild.RawFeatures.Contains("LINKED_TO_HUB")) this.Features.Add(GuildFeaturesEnum.IsLinkedToHub);
		if (guild.RawFeatures.Contains("MEMBER_PROFILES")) this.Features.Add(GuildFeaturesEnum.HasMemberProfiles);
		if (guild.RawFeatures.Contains("INTERNAL_EMPLOYEE_ONLY")) this.Features.Add(GuildFeaturesEnum.IsStaffOnly);
		if (guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_AVAILABLE_FOR_PURCHASE")) this.Features.Add(GuildFeaturesEnum.RoleSubscriptionsIsAvailableForPurchase);
		if (guild.RawFeatures.Contains("AUTO_MODERATION")) this.Features.Add(GuildFeaturesEnum.CanSetupAutoModeration);
		if (guild.RawFeatures.Contains("GUILD_HOME_TEST")) this.Features.Add(GuildFeaturesEnum.GuildHomeTest);
		if (guild.RawFeatures.Contains("INVITES_DISABLED")) this.Features.Add(GuildFeaturesEnum.InvitesDisabled);
	}

	public bool HasFeature(GuildFeaturesEnum flag) => this.Features.Contains(flag);

	public string ToString(string seperator, bool humanReadable)
	{
		if (!humanReadable) return string.Join(seperator, this.Features);

		else
		{
			var humanReadableFeatures = this.Features.Select(x => AddSpacesToWord(x.ToString()));

			return string.Join(seperator, humanReadableFeatures);
		}
	}

	private static string AddSpacesToWord(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return "";
		var newText = new StringBuilder(text.Length * 2);
		newText.Append(text[0]);
		for (int i = 1; i < text.Length; i++)
		{
			if (char.IsUpper(text[i]) && text[i - 1] != ' ')
				newText.Append(' ');
			newText.Append(text[i]);
		}
		return newText.ToString();
	}
}

/// <summary>
/// Represents the guild features.
/// </summary>
public enum GuildFeaturesEnum
{
	/// <summary>
	/// Guild has access to set an animated guild icon.
	/// </summary>
	CanSetAnimatedIcon,

	/// <summary>
	/// Guild has access to set a guild banner image.
	/// </summary>
	CanSetBanner,

	/// <summary>
	/// Guild has access to use commerce features (i.e. create store channels)
	/// </summary>
	[Obsolete("Store applications are EOL.")]
	CanCreateStoreChannels,

	/// <summary>
	/// Guild can enable Welcome Screen, Membership Screening, Stage Channels, News Channels and receives community updates.
	/// Furthermore the guild can apply as a partner and for the discovery (if the prerequisites are given).
	/// <see cref="ChannelType.Stage"/> and <see cref="ChannelType.News"/> is usable.
	/// </summary>
	HasCommunityEnabled,

	/// <summary>
	/// Guild is able to be discovered in the discovery.
	/// </summary>
	IsDiscoverable,

	/// <summary>
	/// Guild is able to be featured in the discovery.
	/// </summary>
	IsFeatureable,

	/// <summary>
	/// Guild has access to set an invite splash background.
	/// </summary>
	CanSetInviteSplash,

	/// <summary>
	/// Guild has enabled Membership Screening.
	/// </summary>
	HasMembershipScreeningEnabled,

	/// <summary>
	/// Guild has access to create news channels.
	/// <see cref="ChannelType.News"/> is usable.
	/// </summary>
	CanCreateNewsChannels,

	/// <summary>
	/// Guild is partnered.
	/// </summary>
	IsPartnered,

	/// <summary>
	/// Guild has increased custom emoji slots.
	/// </summary>
	CanUploadMoreEmojis,

	/// <summary>
	/// Guild can be previewed before joining via Membership Screening or the discovery.
	/// </summary>
	HasPreviewEnabled,

	/// <summary>
	/// Guild has access to set a vanity URL.
	/// </summary>
	CanSetVanityUrl,

	/// <summary>
	/// Guild is verified.
	/// </summary>
	IsVerified,

	/// <summary>
	/// Guild has access to set 384kbps bitrate in voice (previously VIP voice servers).
	/// </summary>
	CanAccessVipRegions,

	/// <summary>
	/// Guild has enabled the welcome screen.
	/// </summary>
	HasWelcomeScreenEnabled,

	/// <summary>
	/// Guild has enabled ticketed events.
	/// </summary>
	HasTicketedEventsEnabled,

	/// <summary>
	/// Guild has enabled monetization.
	/// </summary>
	HasMonetizationEnabled,

	/// <summary>
	/// Guild has increased custom sticker slots.
	/// </summary>
	CanUploadMoreStickers,

	/// <summary>
	/// Guild has access to the three day archive time for threads.
	/// Needs Premium Tier 1 (<see cref="PremiumTier.TierOne"/>).
	/// </summary>
	[Obsolete("Auto archive duration isn't locked to boosts anymore.")]
	CanSetThreadArchiveDurationThreeDays,

	/// <summary>
	/// Guild has access to the seven day archive time for threads.
	/// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
	/// </summary>
	[Obsolete("Auto archive duration isn't locked to boosts anymore.")]
	CanSetThreadArchiveDurationSevenDays,

	/// <summary>
	/// Guild has access to create private threads.
	/// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
	/// </summary>
	CanCreatePrivateThreads,

	/// <summary>
	/// Guild is a hub.
	/// <see cref="ChannelType.GuildDirectory"/> is usable.
	/// </summary>
	IsHub,

	/// <summary>
	/// Guild is in a hub.
	/// https://github.com/discord/discord-api-docs/pull/3757/commits/4932d92c9d0c783861bc715bf7ebbabb15114e34
	/// </summary>
	HasDirectoryEntry,

	/// <summary>
	/// Guild is linked to a hub.
	/// </summary>
	IsLinkedToHub,

	/// <summary>
	/// Guild has full access to threads.
	/// Old Feature.
	/// </summary>
	HasThreadTestingEnabled,

	/// <summary>
	/// Guild has access to threads.
	/// </summary>
	HasThreadsEnabled,

	/// <summary>
	/// Guild can set role icons.
	/// </summary>
	CanSetRoleIcons,

	/// <summary>
	/// Guild has the new thread permissions.
	/// Old Feature.
	/// </summary>
	HasNewThreadPermissions,

	/// <summary>
	/// Guild can set thread default auto archive duration.
	/// Old Feature.
	/// </summary>
	CanSetThreadDefaultAutoArchiveDuration,

	/// <summary>
	/// Guild has enabled role subscriptions.
	/// </summary>
	HasRoleSubscriptionsEnabled,

	/// <summary>
	/// Guild role subscriptions as purchaseable.
	/// </summary>
	RoleSubscriptionsIsAvailableForPurchase,

	/// <summary>
	/// Guild has premium tier 3 override.
	/// </summary>
	PremiumTierThreeOverride,

	/// <summary>
	/// Guild has access to text in voice.
	/// Restricted to <see cref="IsStaffOnly"/>.
	/// </summary>
	TextInVoiceEnabled,

	/// <summary>
	/// Guild can set an animated banner.
	/// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
	/// </summary>
	CanSetAnimatedBanner,

	/// <summary>
	/// Guild can set an animated banner.
	/// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
	/// </summary>
	CanSetChannelBanner,

	/// <summary>
	/// Allows members to customize their avatar, banner and bio for that server.
	/// </summary>
	HasMemberProfiles,

	/// <summary>
	/// Guild is restricted to users with the <see cref="UserFlags.Staff"/> badge.
	/// </summary>
	IsStaffOnly,

	/// <summary>
	/// Guild can use and setup the experimental auto moderation feature.
	/// </summary>
	CanSetupAutoModeration,

	/// <summary>
	/// Guild has access to home.
	/// </summary>
	GuildHomeTest,

	/// <summary>
	/// Guild has disabled invites.
	/// </summary>
	InvitesDisabled,

	///// <summary>
	///// String of guild features.
	///// </summary>
	//public string FeatureString { get; }

	///// <summary>
	///// Checks the guild features and constructs a new <see cref="GuildFeatures"/> object.
	///// </summary>
	///// <param name="guild">Guild to check</param>
	//public GuildFeatures(DiscordGuild guild)
	//{
	//guild.RawFeatures.Contains("ANIMATED_ICON");
	//guild.RawFeatures.Contains("ANIMATED_BANNER");
	//guild.RawFeatures.Contains("BANNER");
	//guild.RawFeatures.Contains("CHANNEL_BANNER");
	//guild.RawFeatures.Contains("COMMERCE");
	//guild.RawFeatures.Contains("COMMUNITY");
	//!guild.RawFeatures.Contains("DISCOVERABLE_DISABLED") && guild.RawFeatures.Contains("DISCOVERABLE");
	//guild.RawFeatures.Contains("FEATUREABLE");
	//guild.RawFeatures.Contains("INVITE_SPLASH");
	//guild.RawFeatures.Contains("MEMBER_VERIFICATION_GATE_ENABLED");
	//guild.RawFeatures.Contains("NEWS");
	//guild.RawFeatures.Contains("PARTNERED");
	//guild.RawFeatures.Contains("MORE_EMOJI");
	//guild.RawFeatures.Contains("PREVIEW_ENABLED");
	//guild.RawFeatures.Contains("VANITY_URL");
	//guild.RawFeatures.Contains("VERIFIED");
	//guild.RawFeatures.Contains("VIP_REGIONS");
	//guild.RawFeatures.Contains("WELCOME_SCREEN_ENABLED");
	//guild.RawFeatures.Contains("TICKETED_EVENTS_ENABLED");
	//guild.RawFeatures.Contains("MONETIZATION_ENABLED");
	//guild.RawFeatures.Contains("MORE_STICKERS");
	//guild.RawFeatures.Contains("THREE_DAY_THREAD_ARCHIVE");
	//guild.RawFeatures.Contains("SEVEN_DAY_THREAD_ARCHIVE");
	//guild.RawFeatures.Contains("PRIVATE_THREADS");
	//guild.RawFeatures.Contains("HUB");
	//guild.RawFeatures.Contains("THREADS_ENABLED_TESTING");
	//guild.RawFeatures.Contains("THREADS_ENABLED");
	//guild.RawFeatures.Contains("ROLE_ICONS");
	//guild.RawFeatures.Contains("NEW_THREAD_PERMISSIONS");
	//guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_ENABLED");
	//guild.RawFeatures.Contains("PREMIUM_TIER_3_OVERRIDE");
	//guild.RawFeatures.Contains("THREAD_DEFAULT_AUTO_ARCHIVE_DURATION");
	//guild.RawFeatures.Contains("TEXT_IN_VOICE_ENABLED");
	//guild.RawFeatures.Contains("HAS_DIRECTORY_ENTRY");
	//guild.RawFeatures.Contains("LINKED_TO_HUB");
	//guild.RawFeatures.Contains("MEMBER_PROFILES");
	//guild.RawFeatures.Contains("INTERNAL_EMPLOYEE_ONLY");
	//guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_AVAILABLE_FOR_PURCHASE");
	//guild.RawFeatures.Contains("AUTO_MODERATION");
	//guild.RawFeatures.Contains("GUILD_HOME_TEST");
	//guild.RawFeatures.Contains("INVITES_DISABLED");

	//	var features = guild.RawFeatures.Any() ? "" : "None";
	//	foreach (var feature in guild.RawFeatures)
	//	{
	//		features += feature + " ";
	//	}
	//	this.FeatureString = features;
	//	}
}
