using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities.Guild.AuditLog;

/// <summary>
/// Represents a change set for a server settings update.
/// </summary>
public class GuildUpdateChangeSet : AuditLogChangeSet
{
	/// <inheritdoc cref="AuditLogChangeSet.ValidFor" />
	public new AuditLogActionType ValidFor = AuditLogActionType.GuildUpdate;

	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	public bool IconChanged => this.IconBefore is not null || this.IconAfter is not null;
	public string? IconBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "icon")?.OldValue;
	public string? IconAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "icon")?.NewValue;

	public bool SplashChanged => this.SplashBefore is not null || this.SplashAfter is not null;
	public string? SplashBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "splash")?.OldValue;
	public string? SplashAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "splash")?.NewValue;

	public bool DiscoverySplashChanged => this.DiscoverySplashBefore is not null || this.DiscoverySplashAfter is not null;
	public string? DiscoverySplashBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "discovery_splash")?.OldValue;
	public string? DiscoverySplashAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "discovery_splash")?.NewValue;

	public bool OwnerIdChanged => this.OwnerIdBefore is not null || this.OwnerIdAfter is not null;
	public ulong? OwnerIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "owner_id")?.OldValue;
	public ulong? OwnerIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "owner_id")?.NewValue;

	public bool AfkChannelIdChanged => this.AfkChannelIdBefore is not null || this.AfkChannelIdAfter is not null;
	public ulong? AfkChannelIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "afk_channel_id")?.OldValue;
	public ulong? AfkChannelIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "afk_channel_id")?.NewValue;

	public bool AfkTimeoutChanged => this.AfkTimeoutBefore is not null || this.AfkTimeoutAfter is not null;
	public int? AfkTimeoutBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "afk_timeout")?.OldValue;
	public int? AfkTimeoutAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "afk_timeout")?.NewValue;

	public bool WidgetEnabledChanged => this.WidgetEnabledBefore is not null || this.WidgetEnabledAfter is not null;
	public bool? WidgetEnabledBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "widget_enabled")?.OldValue;
	public bool? WidgetEnabledAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "widget_enabled")?.NewValue;

	public bool WidgetChannelIdChanged => this.WidgetChannelIdBefore is not null || this.WidgetChannelIdAfter is not null;
	public ulong? WidgetChannelIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "widget_channel_id")?.OldValue;
	public ulong? WidgetChannelIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "widget_channel_id")?.NewValue;

	public bool VerificationLevelChanged => this.VerificationLevelBefore is not null || this.VerificationLevelAfter is not null;
	public VerificationLevel? VerificationLevelBefore => (VerificationLevel?)this.Changes.FirstOrDefault(x => x.Key == "verification_level")?.OldValue;
	public VerificationLevel? VerificationLevelAfter => (VerificationLevel?)this.Changes.FirstOrDefault(x => x.Key == "verification_level")?.NewValue;

	public bool DefaultMessageNotificationsChanged => this.DefaultMessageNotificationsBefore is not null || this.DefaultMessageNotificationsAfter is not null;
	public DefaultMessageNotifications? DefaultMessageNotificationsBefore => (DefaultMessageNotifications?)this.Changes.FirstOrDefault(x => x.Key == "default_message_notifications")?.OldValue;
	public DefaultMessageNotifications? DefaultMessageNotificationsAfter => (DefaultMessageNotifications?)this.Changes.FirstOrDefault(x => x.Key == "default_message_notifications")?.NewValue;

	public bool ExplicitContentFilterChanged => this.ExplicitContentFilterBefore is not null || this.ExplicitContentFilterAfter is not null;
	public ExplicitContentFilter? ExplicitContentFilterBefore => (ExplicitContentFilter?)this.Changes.FirstOrDefault(x => x.Key == "explicit_content_filter")?.OldValue;
	public ExplicitContentFilter? ExplicitContentFilterAfter => (ExplicitContentFilter?)this.Changes.FirstOrDefault(x => x.Key == "explicit_content_filter")?.NewValue;

	public bool FeaturesChanged => this.FeaturesBefore is not null || this.FeaturesAfter is not null;
	public IReadOnlyList<string>? FeaturesBefore => (IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "features")?.OldValue;
	public IReadOnlyList<string>? FeaturesAfter => (IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "features")?.NewValue;

	public bool MfaLevelChanged => this.MfaLevelBefore is not null || this.MfaLevelAfter is not null;
	public MfaLevel? MfaLevelBefore => (MfaLevel?)this.Changes.FirstOrDefault(x => x.Key == "mfa_level")?.OldValue;
	public MfaLevel? MfaLevelAfter => (MfaLevel?)this.Changes.FirstOrDefault(x => x.Key == "mfa_level")?.NewValue;

	public bool SystemChannelIdChanged => this.SystemChannelIdBefore is not null || this.SystemChannelIdAfter is not null;
	public ulong? SystemChannelIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "system_channel_id")?.OldValue;
	public ulong? SystemChannelIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "system_channel_id")?.NewValue;

	public bool SystemChannelFlagsChanged => this.SystemChannelFlagsBefore is not null || this.SystemChannelFlagsAfter is not null;
	public SystemChannelFlags? SystemChannelFlagsBefore => (SystemChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "system_channel_flags")?.OldValue;
	public SystemChannelFlags? SystemChannelFlagsAfter => (SystemChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "system_channel_flags")?.NewValue;

	public bool RulesChannelIdChanged => this.RulesChannelIdBefore is not null || this.RulesChannelIdAfter is not null;
	public ulong? RulesChannelIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "rules_channel_id")?.OldValue;
	public ulong? RulesChannelIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "rules_channel_id")?.NewValue;

	public bool VanityUrlCodeChanged => this.VanityUrlCodeBefore is not null || this.VanityUrlCodeAfter is not null;
	public string? VanityUrlCodeBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "vanity_url_code")?.OldValue;
	public string? VanityUrlCodeAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "vanity_url_code")?.NewValue;

	public bool DescriptionChanged => this.DescriptionBefore is not null || this.DescriptionAfter is not null;
	public string? DescriptionBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "description")?.OldValue;
	public string? DescriptionAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "description")?.NewValue;

	public bool BannerChanged => this.BannerBefore is not null || this.BannerAfter is not null;
	public string? BannerBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "banner")?.OldValue;
	public string? BannerAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "banner")?.NewValue;

	public bool PreferredLocaleChanged => this.PreferredLocaleBefore is not null || this.PreferredLocaleAfter is not null;
	public string? PreferredLocaleBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "preferred_locale")?.OldValue;
	public string? PreferredLocaleAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "preferred_locale")?.NewValue;

	public bool PublicUpdatesChannelIdChanged => this.PublicUpdatesChannelIdBefore is not null || this.PublicUpdatesChannelIdAfter is not null;
	public ulong? PublicUpdatesChannelIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "public_updates_channel_id")?.OldValue;
	public ulong? PublicUpdatesChannelIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "public_updates_channel_id")?.NewValue;

	public bool NsfwLevelChanged => this.NsfwLevelBefore is not null || this.NsfwLevelAfter is not null;
	public NsfwLevel? NsfwLevelBefore => (NsfwLevel?)this.Changes.FirstOrDefault(x => x.Key == "nsfw_level")?.OldValue;
	public NsfwLevel? NsfwLevelAfter => (NsfwLevel?)this.Changes.FirstOrDefault(x => x.Key == "nsfw_level")?.NewValue;

	public bool PremiumProgressBarEnabledChanged => this.PremiumProgressBarEnabledBefore is not null || this.PremiumProgressBarEnabledAfter is not null;
	public bool? PremiumProgressBarEnabledBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "premium_progress_bar_enabled")?.OldValue;
	public bool? PremiumProgressBarEnabledAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "premium_progress_bar_enabled")?.NewValue;

	public bool SafetyAlertsChannelIdChanged => this.SafetyAlertsChannelIdBefore is not null || this.SafetyAlertsChannelIdAfter is not null;
	public ulong? SafetyAlertsChannelIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "safety_alerts_channel_id")?.OldValue;
	public ulong? SafetyAlertsChannelIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "safety_alerts_channel_id")?.NewValue;

	public override string ChangeDescription
	{
		get
		{
			var description = $"{this.UserId} executed {this.GetType().Name.Replace("ChangeSet", string.Empty)}\n";

			if (this.NameChanged)
				description += this.NameAfter != null ? $"- Set Name to {this.NameAfter}\n" : "- Unset Name\n";

			if (this.IconChanged)
				description += this.IconAfter != null ? $"- Set Icon to {this.IconAfter}\n" : "- Unset Icon\n";

			if (this.SplashChanged)
				description += this.SplashAfter != null ? $"- Set Splash to {this.SplashAfter}\n" : "- Unset Splash\n";

			if (this.DiscoverySplashChanged)
				description += this.DiscoverySplashAfter != null ? $"- Set Discovery Splash to {this.DiscoverySplashAfter}\n" : "- Unset Discovery Splash\n";

			if (this.OwnerIdChanged)
				description += this.OwnerIdAfter != null ? $"- Set Owner ID to {this.OwnerIdAfter}\n" : "- Unset Owner ID\n";

			if (this.AfkChannelIdChanged)
				description += this.AfkChannelIdAfter != null ? $"- Set AFK Channel ID to {this.AfkChannelIdAfter}\n" : "- Unset AFK Channel ID\n";

			if (this.AfkTimeoutChanged)
				description += this.AfkTimeoutAfter != null ? $"- Set AFK Timeout to {this.AfkTimeoutAfter} seconds\n" : "- Unset AFK Timeout\n";

			if (this.WidgetEnabledChanged)
				description += this.WidgetEnabledAfter != null ? $"- Set Widget Enabled to {this.WidgetEnabledAfter}\n" : "- Unset Widget Enabled\n";

			if (this.WidgetChannelIdChanged)
				description += this.WidgetChannelIdAfter != null ? $"- Set Widget Channel ID to {this.WidgetChannelIdAfter}\n" : "- Unset Widget Channel ID\n";

			if (this.VerificationLevelChanged)
				description += this.VerificationLevelAfter != null ? $"- Set Verification Level to {this.VerificationLevelAfter}\n" : "- Unset Verification Level\n";

			if (this.DefaultMessageNotificationsChanged)
				description += this.DefaultMessageNotificationsAfter != null ? $"- Set Default Message Notifications to {this.DefaultMessageNotificationsAfter}\n" : "- Unset Default Message Notifications\n";

			if (this.ExplicitContentFilterChanged)
				description += this.ExplicitContentFilterAfter != null ? $"- Set Explicit Content Filter to {this.ExplicitContentFilterAfter}\n" : "- Unset Explicit Content Filter\n";

			if (this.FeaturesChanged)
				description += this.FeaturesAfter != null ? $"- Set Features to {string.Join(", ", this.FeaturesAfter)}\n" : "- Unset Features\n";

			if (this.MfaLevelChanged)
				description += this.MfaLevelAfter != null ? $"- Set MFA Level to {this.MfaLevelAfter}\n" : "- Unset MFA Level\n";

			if (this.SystemChannelIdChanged)
				description += this.SystemChannelIdAfter != null ? $"- Set System Channel ID to {this.SystemChannelIdAfter}\n" : "- Unset System Channel ID\n";

			if (this.SystemChannelFlagsChanged)
				description += this.SystemChannelFlagsAfter != null ? $"- Set System Channel Flags to {this.SystemChannelFlagsAfter}\n" : "- Unset System Channel Flags\n";

			if (this.RulesChannelIdChanged)
				description += this.RulesChannelIdAfter != null ? $"- Set Rules Channel ID to {this.RulesChannelIdAfter}\n" : "- Unset Rules Channel ID\n";

			if (this.VanityUrlCodeChanged)
				description += this.VanityUrlCodeAfter != null ? $"- Set Vanity URL Code to {this.VanityUrlCodeAfter}\n" : "- Unset Vanity URL Code\n";

			if (this.DescriptionChanged)
				description += this.DescriptionAfter != null ? $"- Set Description to {this.DescriptionAfter}\n" : "- Unset Description\n";

			if (this.BannerChanged)
				description += this.BannerAfter != null ? $"- Set Banner to {this.BannerAfter}\n" : "- Unset Banner\n";

			if (this.PreferredLocaleChanged)
				description += this.PreferredLocaleAfter != null ? $"- Set Preferred Locale to {this.PreferredLocaleAfter}\n" : "- Unset Preferred Locale\n";

			if (this.PublicUpdatesChannelIdChanged)
				description += this.PublicUpdatesChannelIdAfter != null ? $"- Set Public Updates Channel ID to {this.PublicUpdatesChannelIdAfter}\n" : "- Unset Public Updates Channel ID\n";

			if (this.NsfwLevelChanged)
				description += this.NsfwLevelAfter != null ? $"- Set NSFW Level to {this.NsfwLevelAfter}\n" : "- Unset NSFW Level\n";

			if (this.PremiumProgressBarEnabledChanged)
				description += this.PremiumProgressBarEnabledAfter != null ? $"- Set Premium Progress Bar Enabled to {this.PremiumProgressBarEnabledAfter}\n" : "- Unset Premium Progress Bar Enabled\n";

			if (this.SafetyAlertsChannelIdChanged)
				description += this.SafetyAlertsChannelIdAfter != null ? $"- Set Safety Alerts Channel ID to {this.SafetyAlertsChannelIdAfter}\n" : "- Unset Safety Alerts Channel ID\n";

			return description;
		}
	}
}
