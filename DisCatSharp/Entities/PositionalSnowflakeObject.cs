using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an object in the Discord API.
/// </summary>
public abstract class PositionalSnowflakeObject : SnowflakeObject
{
	/// <summary>
	/// Gets the position
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; internal set; }

	/// <summary>
	/// High position value equals a lower position.
	/// </summary>
	[JsonIgnore]
	internal virtual bool HighIsLow { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PositionalSnowflakeObject"/> class.
	/// </summary>
	/// <param name="ignored">List of property names to ignore during JSON serialization.</param>
	internal PositionalSnowflakeObject(List<string>? ignored = null)
		: base(ignored)
	{
	}

	/// <summary>
	/// Determines whether the left <see cref="PositionalSnowflakeObject"/> is higher positioned than the right <see cref="PositionalSnowflakeObject"/>.
	/// </summary>
	/// <param name="left">The first <see cref="PositionalSnowflakeObject"/>.</param>
	/// <param name="right">The second <see cref="PositionalSnowflakeObject"/>.</param>
	/// <returns><see langword="true"/> if the left one is higher positioned; otherwise, <see langword="false"/>.</returns>
	public static bool operator >(PositionalSnowflakeObject? left, PositionalSnowflakeObject? right)
		=> left is not null && right is not null &&
		   (left.HighIsLow ? left.Position < right.Position : left.Position > right.Position);

	/// <summary>
	/// Determines whether the left <see cref="PositionalSnowflakeObject"/> is lower positioned than the right <see cref="PositionalSnowflakeObject"/>.
	/// </summary>
	/// <param name="left">The first <see cref="PositionalSnowflakeObject"/>.</param>
	/// <param name="right">The second <see cref="PositionalSnowflakeObject"/>.</param>
	/// <returns><see langword="true"/> if the left one is lower positioned; otherwise, <see langword="false"/>.</returns>
	public static bool operator <(PositionalSnowflakeObject? left, PositionalSnowflakeObject? right)
		=> left is not null && right is not null &&
		   (left.HighIsLow ? left.Position > right.Position : left.Position < right.Position);

	/// <summary>
	/// Returns a <see langword="string"/> which represents the <see cref="PositionalSnowflakeObject"/>.
	/// </summary>
	/// <returns>A <see langword="string"/> which represents the current <see cref="PositionalSnowflakeObject"/>.</returns>
	public override string ToString()
		=> $"{this.GetType().Name} (ID: {this.Id}, Position: {this.Position})";
}
