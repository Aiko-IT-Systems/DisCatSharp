using System.Threading.Tasks;

using DisCatSharp.Exceptions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a application emoji.
/// </summary>
public sealed class DiscordApplicationEmoji : DiscordEmoji
{
	/// <summary>
	/// Gets the user that created the emoji (Either a team member or the bot through api).
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser? User { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApplicationEmoji"/> class.
	/// </summary>
	internal DiscordApplicationEmoji()
	{ }

	/// <summary>
	/// Modifies this emoji.
	/// </summary>
	/// <param name="name">New name for this emoji.</param>
	/// <returns>The modified emoji.</returns>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordApplicationEmoji> ModifyAsync(string name)
		=> this.Discord.ApiClient.ModifyApplicationEmojiAsync(this.Discord.CurrentApplication.Id, this.Id, name);

	/// <summary>
	/// Deletes this emoji.
	/// </summary>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync()
		=> this.Discord.ApiClient.DeleteApplicationEmojiAsync(this.Discord.CurrentApplication.Id, this.Id);
}
