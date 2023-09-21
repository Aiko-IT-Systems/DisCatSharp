using DisCatSharp.Entities;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a tag edit model.
/// </summary>
public class ForumPostTagEditModel : BaseEditModel
{
	/// <summary>
	/// Sets the tags's new name.
	/// </summary>
	public Optional<string> Name { internal get; set; }

	/// <summary>
	/// Sets the tags's new emoji.
	/// </summary>
	public Optional<DiscordEmoji> Emoji { internal get; set; }

	/// <summary>
	/// Sets whether the tag should be mod only.
	/// </summary>
	public Optional<bool> Moderated { internal get; set; }
}
