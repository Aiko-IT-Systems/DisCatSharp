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
	internal DiscordButtonComponent[] ButtonArray => new[]
	{
		this.SkipLeft,
		this.Left,
		this.Stop,
		this.Right,
		this.SkipRight
	};

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
		this.SkipLeft = new DiscordButtonComponent(ButtonStyle.Secondary, "leftskip", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏮")));
		this.Left = new DiscordButtonComponent(ButtonStyle.Secondary, "left", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
		this.Stop = new DiscordButtonComponent(ButtonStyle.Secondary, "stop", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏹")));
		this.Right = new DiscordButtonComponent(ButtonStyle.Secondary, "right", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
		this.SkipRight = new DiscordButtonComponent(ButtonStyle.Secondary, "rightskip", null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏭")));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PaginationButtons"/> class.
	/// </summary>
	/// <param name="other">The other <see cref="PaginationButtons"/>.</param>
	public PaginationButtons(PaginationButtons other)
	{
		this.Stop = new DiscordButtonComponent(other.Stop);
		this.Left = new DiscordButtonComponent(other.Left);
		this.Right = new DiscordButtonComponent(other.Right);
		this.SkipLeft = new DiscordButtonComponent(other.SkipLeft);
		this.SkipRight = new DiscordButtonComponent(other.SkipRight);
	}
}
