using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordApplicationCommand : SnowflakeObject
{
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandType? Type { get; set; }

	[JsonProperty("application_id")]
	public ulong ApplicationId { get; set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> NameLocalizations { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> DescriptionLocalizations { get; set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordApplicationCommandOption> Options { get; set; }

	[JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Ignore)]
	public string DefaultMemberPermissions { get; set; }

	[JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Ignore)]
	public bool? DmPermission { get; set; }

	[JsonProperty("default_permission")]
	public bool DefaultPermission { get; set; } = true;

	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Nsfw { get; set; }

	[JsonProperty("version")]
	public ulong Version { get; set; }
}
