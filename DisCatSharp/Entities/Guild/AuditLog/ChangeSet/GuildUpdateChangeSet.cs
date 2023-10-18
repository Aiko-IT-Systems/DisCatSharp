using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a server settings update.
/// </summary>
public sealed class GuildUpdateChangeSet : DiscordAuditLogEntry
{
	public GuildUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.GuildUpdate;
	}

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
	public ulong? OwnerIdBefore => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "owner_id")?.OldValue);
	public ulong? OwnerIdAfter => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "owner_id")?.NewValue);

	public bool AfkChannelIdChanged => this.AfkChannelIdBefore is not null || this.AfkChannelIdAfter is not null;
	public ulong? AfkChannelIdBefore => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "afk_channel_id")?.OldValue);
	public ulong? AfkChannelIdAfter => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "afk_channel_id")?.NewValue);

	public bool AfkTimeoutChanged => this.AfkTimeoutBefore is not null || this.AfkTimeoutAfter is not null;
	public int? AfkTimeoutBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "afk_timeout")?.OldValue;
	public int? AfkTimeoutAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "afk_timeout")?.NewValue;

	public bool WidgetEnabledChanged => this.WidgetEnabledBefore is not null || this.WidgetEnabledAfter is not null;
	public bool? WidgetEnabledBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "widget_enabled")?.OldValue;
	public bool? WidgetEnabledAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "widget_enabled")?.NewValue;

	public bool WidgetChannelIdChanged => this.WidgetChannelIdBefore is not null || this.WidgetChannelIdAfter is not null;
	public ulong? WidgetChannelIdBefore => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "widget_channel_id")?.OldValue);
	public ulong? WidgetChannelIdAfter => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "widget_channel_id")?.NewValue);

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
	public ulong? SystemChannelIdBefore => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "system_channel_id")?.OldValue);
	public ulong? SystemChannelIdAfter => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "system_channel_id")?.NewValue);

	public bool SystemChannelFlagsChanged => this.SystemChannelFlagsBefore is not null || this.SystemChannelFlagsAfter is not null;
	public SystemChannelFlags? SystemChannelFlagsBefore => (SystemChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "system_channel_flags")?.OldValue;
	public SystemChannelFlags? SystemChannelFlagsAfter => (SystemChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "system_channel_flags")?.NewValue;

	public bool RulesChannelIdChanged => this.RulesChannelIdBefore is not null || this.RulesChannelIdAfter is not null;
	public ulong? RulesChannelIdBefore => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "rules_channel_id")?.OldValue);
	public ulong? RulesChannelIdAfter => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "rules_channel_id")?.NewValue);

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
	public ulong? PublicUpdatesChannelIdBefore => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "public_updates_channel_id")?.OldValue);
	public ulong? PublicUpdatesChannelIdAfter => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "public_updates_channel_id")?.NewValue);

	public bool NsfwLevelChanged => this.NsfwLevelBefore is not null || this.NsfwLevelAfter is not null;
	public NsfwLevel? NsfwLevelBefore => (NsfwLevel?)this.Changes.FirstOrDefault(x => x.Key == "nsfw_level")?.OldValue;
	public NsfwLevel? NsfwLevelAfter => (NsfwLevel?)this.Changes.FirstOrDefault(x => x.Key == "nsfw_level")?.NewValue;

	public bool PremiumProgressBarEnabledChanged => this.PremiumProgressBarEnabledBefore is not null || this.PremiumProgressBarEnabledAfter is not null;
	public bool? PremiumProgressBarEnabledBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "premium_progress_bar_enabled")?.OldValue;
	public bool? PremiumProgressBarEnabledAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "premium_progress_bar_enabled")?.NewValue;

	public bool SafetyAlertsChannelIdChanged => this.SafetyAlertsChannelIdBefore is not null || this.SafetyAlertsChannelIdAfter is not null;
	public ulong? SafetyAlertsChannelIdBefore => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "safety_alerts_channel_id")?.OldValue);
	public ulong? SafetyAlertsChannelIdAfter => ConvertToUlong(this.Changes.FirstOrDefault(x => x.Key == "safety_alerts_channel_id")?.NewValue);

	/// <inheritdoc />
	internal override string ChangeDescription
	{
		get
		{
			var description = $"{this.UserId} executed {this.GetType().Name.Replace("ChangeSet", string.Empty)} with reason {this.Reason ?? "No reason given".Italic()}\n";

			if (this.NameChanged)
			{
				description += $"Old Name: {this.NameBefore ?? "Not set"}\n";
				description += $"New Name: {this.NameAfter ?? "Not set"}\n";
			}

			if (this.IconChanged)
			{
				description += $"Old Icon: {this.IconBefore ?? "Not set"}\n";
				description += $"New Icon: {this.IconAfter ?? "Not set"}\n";
			}

			if (this.SplashChanged)
			{
				description += $"Old Splash: {this.SplashBefore ?? "Not set"}\n";
				description += $"New Splash: {this.SplashAfter ?? "Not set"}\n";
			}

			if (this.DiscoverySplashChanged)
			{
				description += $"Old Discovery Splash: {this.DiscoverySplashBefore ?? "Not set"}\n";
				description += $"New Discovery Splash: {this.DiscoverySplashAfter ?? "Not set"}\n";
			}

			if (this.OwnerIdChanged)
			{
				description += $"Old Owner ID: {this.OwnerIdBefore?.ToString() ?? "Not set"}\n";
				description += $"New Owner ID: {this.OwnerIdAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.AfkChannelIdChanged)
			{
				description += $"Old AFK Channel ID: {this.AfkChannelIdBefore?.ToString() ?? "Not set"}\n";
				description += $"New AFK Channel ID: {this.AfkChannelIdAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.AfkTimeoutChanged)
			{
				description += $"Old AFK Timeout: {this.AfkTimeoutBefore?.ToString() ?? "Not set"} seconds\n";
				description += $"New AFK Timeout: {this.AfkTimeoutAfter?.ToString() ?? "Not set"} seconds\n";
			}

			if (this.WidgetEnabledChanged)
			{
				description += $"Old Widget Enabled: {this.WidgetEnabledBefore?.ToString() ?? "Not set"}\n";
				description += $"New Widget Enabled: {this.WidgetEnabledAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.WidgetChannelIdChanged)
			{
				description += $"Old Widget Channel ID: {this.WidgetChannelIdBefore?.ToString() ?? "Not set"}\n";
				description += $"New Widget Channel ID: {this.WidgetChannelIdAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.VerificationLevelChanged)
			{
				description += $"Old Verification Level: {this.VerificationLevelBefore?.ToString() ?? "Not set"}\n";
				description += $"New Verification Level: {this.VerificationLevelAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.DefaultMessageNotificationsChanged)
			{
				description += $"Old Default Message Notifications: {this.DefaultMessageNotificationsBefore?.ToString() ?? "Not set"}\n";
				description += $"New Default Message Notifications: {this.DefaultMessageNotificationsAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.ExplicitContentFilterChanged)
			{
				description += $"Old Explicit Content Filter: {this.ExplicitContentFilterBefore?.ToString() ?? "Not set"}\n";
				description += $"New Explicit Content Filter: {this.ExplicitContentFilterAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.FeaturesChanged)
			{
				description += $"Old Features: {string.Join(", ", this.FeaturesBefore ?? new List<string>())}\n";
				description += $"New Features: {string.Join(", ", this.FeaturesAfter ?? new List<string>())}\n";
			}

			if (this.MfaLevelChanged)
			{
				description += $"Old MFA Level: {this.MfaLevelBefore?.ToString() ?? "Not set"}\n";
				description += $"New MFA Level: {this.MfaLevelAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.SystemChannelIdChanged)
			{
				description += $"Old System Channel ID: {this.SystemChannelIdBefore?.ToString() ?? "Not set"}\n";
				description += $"New System Channel ID: {this.SystemChannelIdAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.SystemChannelFlagsChanged)
			{
				description += $"Old System Channel Flags: {this.SystemChannelFlagsBefore?.ToString() ?? "Not set"}\n";
				description += $"New System Channel Flags: {this.SystemChannelFlagsAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.RulesChannelIdChanged)
			{
				description += $"Old Rules Channel ID: {this.RulesChannelIdBefore?.ToString() ?? "Not set"}\n";
				description += $"New Rules Channel ID: {this.RulesChannelIdAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.VanityUrlCodeChanged)
			{
				description += $"Old Vanity URL Code: {this.VanityUrlCodeBefore ?? "Not set"}\n";
				description += $"New Vanity URL Code: {this.VanityUrlCodeAfter ?? "Not set"}\n";
			}

			if (this.DescriptionChanged)
			{
				description += $"Old Description: {this.DescriptionBefore ?? "Not set"}\n";
				description += $"New Description: {this.DescriptionAfter ?? "Not set"}\n";
			}

			if (this.BannerChanged)
			{
				description += $"Old Banner: {this.BannerBefore ?? "Not set"}\n";
				description += $"New Banner: {this.BannerAfter ?? "Not set"}\n";
			}

			if (this.PreferredLocaleChanged)
			{
				description += $"Old Preferred Locale: {this.PreferredLocaleBefore ?? "Not set"}\n";
				description += $"New Preferred Locale: {this.PreferredLocaleAfter ?? "Not set"}\n";
			}

			if (this.PublicUpdatesChannelIdChanged)
			{
				description += $"Old Public Updates Channel ID: {this.PublicUpdatesChannelIdBefore?.ToString() ?? "Not set"}\n";
				description += $"New Public Updates Channel ID: {this.PublicUpdatesChannelIdAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.NsfwLevelChanged)
			{
				description += $"Old NSFW Level: {this.NsfwLevelBefore?.ToString() ?? "Not set"}\n";
				description += $"New NSFW Level: {this.NsfwLevelAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.PremiumProgressBarEnabledChanged)
			{
				description += $"Old Premium Progress Bar Enabled: {this.PremiumProgressBarEnabledBefore?.ToString() ?? "Not set"}\n";
				description += $"New Premium Progress Bar Enabled: {this.PremiumProgressBarEnabledAfter?.ToString() ?? "Not set"}\n";
			}

			if (this.SafetyAlertsChannelIdChanged)
			{
				description += $"Old Safety Alerts Channel ID: {this.SafetyAlertsChannelIdBefore?.ToString() ?? "Not set"}\n";
				description += $"New Safety Alerts Channel ID: {this.SafetyAlertsChannelIdAfter?.ToString() ?? "Not set"}\n";
			}

			return description;
		}
	}
}
