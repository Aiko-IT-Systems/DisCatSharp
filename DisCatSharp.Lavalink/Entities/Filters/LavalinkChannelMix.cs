using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Mixes both channels (left and right), with a configurable factor on how much each channel affects the other. With the defaults, both channels are kept independent of each other. Setting all factors to 0.5 means both channels get the same audio.
/// </summary>
public sealed class LavalinkChannelMix
{
	/// <summary>
	/// Gets or sets the left to left channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)
	/// </summary>
	[JsonProperty("leftToLeft")]
	public Optional<float> LeftToLeft { get; set; }

	/// <summary>
	/// Gets or sets the left to right channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)
	/// </summary>
	[JsonProperty("leftToRight")]
	public Optional<float> LeftToRight { get; set; }

	/// <summary>
	/// Gets or sets the right to left channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)
	/// </summary>
	[JsonProperty("rightToLeft")]
	public Optional<float> RightToLeft { get; set; }

	/// <summary>
	/// Gets or sets the right to right channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)
	/// </summary>
	[JsonProperty("rightToRight")]
	public Optional<float> RightToRight { get; set; }

	/// <inheritdoc cref="LavalinkChannelMix"/>
	/// <param name="leftToLeft">The left to left channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)</param>
	/// <param name="leftToRight">The left to right channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)</param>
	/// <param name="rightToLeft">The right to left channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)</param>
	/// <param name="rightToRight">The right to right channel mix factor (<c>0.0</c> to <c>1.0</c> where <c>0.5</c> is no effect)</param>
	public LavalinkChannelMix(Optional<float> leftToLeft, Optional<float> leftToRight, Optional<float> rightToLeft, Optional<float> rightToRight)
	{
		this.LeftToLeft = leftToLeft;
		this.LeftToRight = leftToRight;
		this.RightToLeft = rightToLeft;
		this.RightToRight = rightToRight;
	}
}
