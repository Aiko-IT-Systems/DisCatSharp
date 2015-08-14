using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Interactivity.Enums;

/// <summary>
/// A modal page.
/// </summary>
public class ModalPage
{
	/// <summary>
	/// Creates a new modal page for the paginated modal builder.
	/// </summary>
	/// <param name="modal">The modal to display.</param>
	/// <param name="openButton">The button to display to open the current page. This is skipped if possible.</param>
	/// <param name="openText">The text to display to open the current page. This is skipped if possible.</param>
	public ModalPage(DiscordInteractionModalBuilder modal, DiscordButtonComponent openButton = null, string openText = null)
	{
		this.Modal = modal;
		this.OpenButton = openButton ?? new DiscordButtonComponent(ButtonStyle.Primary, null, "Open next page", false, new(DiscordEmoji.FromUnicode("📄")));
		this.OpenMessage = new DiscordInteractionResponseBuilder().WithContent(openText ?? "`Click the button below to continue to the next page.`").AsEphemeral();
	}

	/// <summary>
	/// The modal that will be displayed.
	/// </summary>
	public DiscordInteractionModalBuilder Modal { get; set; }

	/// <summary>
	/// The button that will be displayed on the ephemeral message.
	/// </summary>
	public DiscordButtonComponent OpenButton { get; set; }

	/// <summary>
	/// The ephemeral message to display for this page.
	/// </summary>
	public DiscordInteractionResponseBuilder OpenMessage { get; set; }
}
