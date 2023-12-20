using System;
using System.Linq;

namespace DisCatSharp.Enums;

/// <summary>
/// Core Domains
/// </summary>
public enum CoreDomain
{
	/// <summary>
	/// dis.gd
	/// </summary>
	[DomainHelp("Marketing URL shortener", "dis.gd")]
	DiscordMarketing = 1,

	/// <summary>
	/// discord.co
	/// </summary>
	[DomainHelp("Admin panel, internal tools", "discord.co")]
	DiscordAdmin = 2,

	/// <summary>
	/// discord.com
	/// </summary>
	[DomainHelp("New app, marketing website, API host", "discord.com")]
	Discord = 3,

	/// <summary>
	/// discord.design
	/// </summary>
	[DomainHelp("Dribbble profile shortlink", "discord.design")]
	DiscordDesign = 4,

	/// <summary>
	/// discord.dev
	/// </summary>
	[DomainHelp("Developer site shortlinks", "discord.dev")]
	DiscordDev = 5,

	/// <summary>
	/// discord.gg
	/// </summary>
	[DomainHelp("Invite shortlinks", "discord.gg")]
	DiscordShortlink = 6,

	/// <summary>
	/// discord.gift
	/// </summary>
	[DomainHelp("Gift shortlinks", "discord.gift")]
	DiscordGift = 7,

	/// <summary>
	/// discord.media
	/// </summary>
	[DomainHelp("Voice servers", "discord.media")]
	DiscordMedia = 8,

	/// <summary>
	/// discord.new
	/// </summary>
	[DomainHelp("Template shortlinks", "discord.new")]
	DiscordTemplate = 9,

	/// <summary>
	/// discord.store
	/// </summary>
	[DomainHelp("Merch store", "discord.store")]
	DiscordMerch = 10,

	/// <summary>
	/// discord.tools
	/// </summary>
	[DomainHelp("Internal tools", "discord.tools")]
	DiscordTools = 11,

	/// <summary>
	/// discordapp.com
	/// </summary>
	[DomainHelp("Old app, marketing website, and API; CDN", "discordapp.com")]
	DiscordAppOld = 12,

	/// <summary>
	/// discordapp.net
	/// </summary>
	[DomainHelp("Media Proxy", "discordapp.net")]
	DiscordAppMediaProxy = 13,

	/// <summary>
	/// discordmerch.com
	/// </summary>
	[DomainHelp("Merch store", "discordmerch.com")]
	DiscordMerchOld = 14,

	/// <summary>
	/// discordpartygames.com
	/// </summary>
	[DomainHelp("Voice channel activity API host", "discordpartygames.com")]
	DiscordActivityAlt = 15,

	/// <summary>
	/// discord-activities.com
	/// </summary>
	[DomainHelp("Voice channel activity API host", "discord-activities.com")]
	DiscordActivityAlt2 = 16,

	/// <summary>
	/// discordsays.com
	/// </summary>
	[DomainHelp("Voice channel activity host", "discordsays.com")]
	DiscordActivity = 17,

	/// <summary>
	/// discordstatus.com
	/// </summary>
	[DomainHelp("Status page", "discordstatus.com")]
	DiscordStatus = 18,

	/// <summary>
	/// cdn.discordapp.com
	/// </summary>
	[DomainHelp("CDN", "cdn.discordapp.com")]
	DiscordCdn = 19
}

/// <summary>
/// Other Domains
/// </summary>
public enum OtherDomain
{
	/// <summary>
	/// airhorn.solutions
	/// </summary>
	[DomainHelp("API implementation example", "airhorn.solutions")]
	Airhorn = 1,

	/// <summary>
	/// airhornbot.com
	/// </summary>
	[DomainHelp("API implementation example", "airhornbot.com")]
	AirhornAlt = 2,

	/// <summary>
	/// bigbeans.solutions
	/// </summary>
	[DomainHelp("April Fools 2017", "bigbeans.solutions")]
	AprilFools = 3,

	/// <summary>
	/// watchanimeattheoffice.com
	/// </summary>
	[DomainHelp("HypeSquad form placeholder/meme", "watchanimeattheoffice.com")]
	HypeSquadMeme = 4
}

/// <summary>
/// Unused Domains
/// </summary>
public enum UnusedDomain
{
	/// <summary>
	/// discordapp.io
	/// </summary>
	[DomainHelp("IO domain for discord", "discordapp.io")]
	DiscordAppIo = 1,

	/// <summary>
	/// discordcdn.com
	/// </summary>
	[DomainHelp("Alternative CDN domain", "discordcdn.com")]
	DiscordCdnCom = 2
}

/// <summary>
/// Represents a discord domain.
/// </summary>
public static class DiscordDomain
{
	/// <summary>
	/// Gets a domain.
	/// Valid types: <see cref="CoreDomain"/>, <see cref="OtherDomain"/> and <see cref="UnusedDomain"/>.
	/// </summary>
	/// <param name="domainEnum">The domain type.</param>
	/// <returns>A DomainHelpAttribute.</returns>
	public static DomainHelpAttribute GetDomain(Enum domainEnum)
	{
		if (domainEnum is not CoreDomain && domainEnum is not OtherDomain && domainEnum is not UnusedDomain)
			throw new NotSupportedException($"Invalid type. Found: {domainEnum.GetType()} Expected: CoreDomain or OtherDomain or UnusedDomain");

		if (domainEnum is CoreDomain domain && (domain == CoreDomain.DiscordAdmin || domain == CoreDomain.DiscordTools))
			throw new UnauthorizedAccessException("You don't have access to this domains");

		var memberInfo = domainEnum.GetType().GetMember(domainEnum.ToString()).FirstOrDefault();
		if (memberInfo != null)
		{
			var attribute = (DomainHelpAttribute)memberInfo.GetCustomAttributes(typeof(DomainHelpAttribute), false).FirstOrDefault();
			return attribute;
		}

		return null;
	}
}

/// <summary>
/// Defines a description and url for this domain.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class DomainHelpAttribute : Attribute
{
	/// <summary>
	/// Gets the Description for this domain.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Gets the Uri for this domain.
	/// </summary>
	public Uri Uri { get; }

	/// <summary>
	/// Gets the Domain for this domain.
	/// </summary>
	public string Domain { get; }

	/// <summary>
	/// Gets the Url for this domain.
	/// </summary>
	public string Url { get; }

	/// <summary>
	/// Defines a description and URIs for this domain.
	/// </summary>
	/// <param name="desc">Description for this domain.</param>
	/// <param name="domain">Url for this domain.</param>
	public DomainHelpAttribute(string desc, string domain)
	{
		this.Description = desc;
		this.Domain = domain;
		var url = $"https://{domain}";
		this.Url = url;
		this.Uri = new(url);
	}
}
