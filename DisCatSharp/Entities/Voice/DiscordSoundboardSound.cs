using System;
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a soundboard sound in a Discord guild.
/// </summary>
public sealed class DiscordSoundboardSound : SnowflakeObject
{
	internal DiscordSoundboardSound()
		: base(["user_id"])
	{ }

	/// <inheritdoc cref="SnowflakeObject.Id" />
	[JsonProperty("sound_id")]
	public new ulong Id { get; internal set; }

	/// <summary>
	///     Gets the name of the soundboard sound.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	///     Gets the volume of the soundboard sound (0 to 1).
	/// </summary>
	[JsonProperty("volume")]
	public double Volume { get; internal set; }

	/// <summary>
	///     Gets the emoji id of the soundboard sound's custom emoji, if applicable.
	/// </summary>
	[JsonProperty("emoji_id")]
	public ulong? EmojiId { get; internal set; }

	/// <summary>
	///     Gets the emoji name of the soundboard sound's standard emoji, if applicable.
	/// </summary>
	[JsonProperty("emoji_name")]
	public string? EmojiName { get; internal set; }

	/// <summary>
	///     Gets whether this sound can be used.
	/// </summary>
	[JsonProperty("available")]
	public bool Available { get; internal set; }

	/// <summary>
	///     Gets the guild id this sound belongs to, if applicable.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	/// <summary>
	///     Gets the guild this sound belongs to, if applicable.
	/// </summary>
	public DiscordGuild? Guild
		=> this.GuildId.HasValue && this.Discord.Guilds.TryGetValue(this.GuildId.Value, out var guild) ? guild : null;

	/// <summary>
	///     Gets the user who created this soundboard sound, if applicable.
	/// </summary>
	[JsonIgnore]
	public DiscordUser? User { get; internal set; }

	/// <summary>
	///     Gets the transport user, if applicable.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	internal TransportUser? TransportUser { get; set; }

	/// <summary>
	///     Gets the sound URL (as mp3) of this sound.
	/// </summary>
	[JsonIgnore]
	public string Url
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.SOUNDBOARD_SOUNDS}/{this.Id.ToString(CultureInfo.InvariantCulture)}.mp3";

	/// <summary>
	///     Gets the mention of the sound to send it in chat.
	/// </summary>
	public string Mention
		=> $"<sound:{(this.GuildId.HasValue ? this.GuildId : 0)}:{this.Id}>";

	/// <summary>
	///     Modifies the soundboard sound.
	/// </summary>
	/// <param name="action">The action to configure the soundboard sound edit model.</param>
	/// <exception cref="NotFoundException">Throws when the soundboard sound cannot be found</exception>
	/// <exception cref="UnauthorizedException">
	///     Throws when the client does not have the <see cref="Permissions.ManageGuildExpressions" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <returns>The updated <see cref="DiscordSoundboardSound" />.</returns>
	public async Task<DiscordSoundboardSound> ModifyAsync(Action<SoundboardSoundEditModel> action)
	{
		if (!this.GuildId.HasValue)
			throw new InvalidOperationException("You can only edit guild soundboard sounds.");

		var mdl = new SoundboardSoundEditModel();
		action(mdl);

		return await this.Discord.ApiClient.ModifyGuildSoundboardSoundAsync(
			this.GuildId.Value,
			this.Id,
			mdl.Name,
			mdl.Volume,
			mdl.EmojiId,
			mdl.EmojiName
		).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes the soundboard sound.
	/// </summary>
	/// <param name="reason">The reason for deleting the sound, to be logged in the audit log. Optional.</param>
	/// <exception cref="NotFoundException">Throws when the soundboard sound cannot be found</exception>
	/// <exception cref="UnauthorizedException">
	///     Throws when the client does not have the <see cref="Permissions.ManageGuildExpressions" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteSoundboardSoundAsync(string? reason = null)
		=> this.GuildId.HasValue ? this.Discord.ApiClient.DeleteGuildSoundboardSoundAsync(this.GuildId.Value, this.Id, reason) : throw new InvalidOperationException("You can only delete guild soundboard sounds.");
}
