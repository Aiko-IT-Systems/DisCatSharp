using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// The pagination buttons.
/// </summary>
public class PaginationButtons
{
	/// <summary>
	/// Gets or sets the skip left button.
	/// </summary>
	public DiscordButtonComponent SkipLeft { internal get; set; }

	/// <summary>
	/// Gets or sets the left button.
	/// </summary>
	public DiscordButtonComponent Left { internal get; set; }

	/// <summary>
	/// Gets or sets the stop button.
	/// </summary>
	public DiscordButtonComponent Stop { internal get; set; }

	/// <summary>
	/// Gets or sets the right button.
	/// </summary>
	public DiscordButtonComponent Right { internal get; set; }

	/// <summary>
	/// Gets or sets the skip right button.
	/// </summary>
	public DiscordButtonComponent SkipRight { internal get; set; }

	/// <summary>
	/// Gets the button array.
	/// </summary>
	internal DiscordButtonComponent[] ButtonArray => [this.SkipLeft, this.Left, this.Stop, this.Right, this.SkipRight];

	public const string SKIP_LEFT_CUSTOM_ID = "pgb-skip-left";
	public const string LEFT_CUSTOM_ID = "pgb-left";
	public const string STOP_CUSTOM_ID = "pgb-stop";
	public const string RIGHT_CUSTOM_ID = "pgb-right";
	public const string SKIP_RIGHT_CUSTOM_ID = "pgb-skip-right";

	/// <summary>
	/// Initializes a new instance of the <see cref="PaginationButtons"/> class.
	/// </summary>
	public PaginationButtons()
	{
		this.SkipLeft = new(ButtonStyle.Secondary, "leftskip", null, false, new(DiscordEmoji.FromUnicode("⏮")));
		this.Left = new(ButtonStyle.Secondary, "left", null, false, new(DiscordEmoji.FromUnicode("◀")));
		this.Stop = new(ButtonStyle.Secondary, "stop", null, false, new(DiscordEmoji.FromUnicode("⏹")));
		this.Right = new(ButtonStyle.Secondary, "right", null, false, new(DiscordEmoji.FromUnicode("▶")));
		this.SkipRight = new(ButtonStyle.Secondary, "rightskip", null, false, new(DiscordEmoji.FromUnicode("⏭")));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PaginationButtons"/> class.
	/// </summary>
	/// <param name="other">The other <see cref="PaginationButtons"/>.</param>
	public PaginationButtons(PaginationButtons other)
	{
		this.Stop = new(other.Stop);
		this.Left = new(other.Left);
		this.Right = new(other.Right);
		this.SkipLeft = new(other.SkipLeft);
		this.SkipRight = new(other.SkipRight);
	}
}
