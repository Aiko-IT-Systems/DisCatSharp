using System.IO;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a guild edit model.
/// </summary>
public class GuildEditModel : BaseEditModel
{
	/// <summary>
	/// The new guild name.
	/// </summary>
	public Optional<string> Name { internal get; set; }

	/// <summary>
	/// The new guild icon.
	/// </summary>
	public Optional<Stream> Icon { internal get; set; }

	/// <summary>
	/// The new guild verification level.
	/// </summary>
	public Optional<VerificationLevel> VerificationLevel { internal get; set; }

	/// <summary>
	/// The new guild default message notification level.
	/// </summary>
	public Optional<DefaultMessageNotifications> DefaultMessageNotifications { internal get; set; }

	/// <summary>
	/// The new guild MFA level.
	/// </summary>
	public Optional<MfaLevel> MfaLevel { internal get; set; }

	/// <summary>
	/// The new guild explicit content filter level.
	/// </summary>
	public Optional<ExplicitContentFilter> ExplicitContentFilter { internal get; set; }

	/// <summary>
	/// The new AFK voice channel.
	/// </summary>
	public Optional<DiscordChannel> AfkChannel { internal get; set; }

	/// <summary>
	/// The new AFK timeout time in seconds.
	/// </summary>
	public Optional<int> AfkTimeout { internal get; set; }

	/// <summary>
	/// The new guild owner.
	/// </summary>
	public Optional<DiscordMember> Owner { internal get; set; }

	/// <summary>
	/// The new guild splash.
	/// </summary>
	public Optional<Stream> Splash { internal get; set; }

	/// <summary>
	/// The new guild system channel.
	/// </summary>
	public Optional<DiscordChannel> SystemChannel { internal get; set; }

	/// <summary>
	/// The guild system channel flags.
	/// </summary>
	public Optional<SystemChannelFlags> SystemChannelFlags { internal get; set; }

	/// <summary>
	/// The guild description.
	/// </summary>
	public Optional<string> Description { internal get; set; }

	/// <summary>
	/// The new guild rules channel.
	/// </summary>
	public Optional<DiscordChannel> RulesChannel { internal get; set; }

	/// <summary>
	/// The new guild public updates channel.
	/// </summary>
	public Optional<DiscordChannel> PublicUpdatesChannel { internal get; set; }

	/// <summary>
	/// The new guild preferred locale.
	/// </summary>
	public Optional<string> PreferredLocale { internal get; set; }

	/// <summary>
	/// The new banner of the guild
	/// </summary>
	public Optional<Stream> Banner { get; set; }

	/// <summary>
	/// The new discovery splash image of the guild
	/// </summary>
	public Optional<Stream> DiscoverySplash { get; set; }

	/// <summary>
	/// The new home header of the guild.
	/// </summary>
	[DiscordInExperiment]
	public Optional<Stream> HomeHeader { get; set; }

	/// <summary>
	/// Whether the premium progress bar should be enabled
	/// </summary>
	public Optional<bool> PremiumProgressBarEnabled { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildEditModel"/> class.
	/// </summary>
	internal GuildEditModel()
	{ }
}
