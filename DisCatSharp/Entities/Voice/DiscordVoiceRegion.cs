using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents information about a Discord voice server region.
/// </summary>
public class DiscordVoiceRegion : ObservableApiObject
{
	/// <summary>
	/// Gets the unique ID for the region.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; internal set; }

	/// <summary>
	/// Gets the name of the region.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets an example server hostname for this region.
	/// </summary>
	[JsonProperty("sample_hostname", NullValueHandling = NullValueHandling.Ignore)]
	public string SampleHostname { get; internal set; }

	/// <summary>
	/// Gets an example server port for this region.
	/// </summary>
	[JsonProperty("sample_port", NullValueHandling = NullValueHandling.Ignore)]
	public int SamplePort { get; internal set; }

	/// <summary>
	/// Gets whether this region is the most optimal for the current user.
	/// </summary>
	[JsonProperty("optimal", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsOptimal { get; internal set; }

	/// <summary>
	/// Gets whether this voice region is deprecated.
	/// </summary>
	[JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsDeprecated { get; internal set; }

	/// <summary>
	/// Gets whether this is a custom voice region.
	/// </summary>
	[JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsCustom { get; internal set; }

	/// <summary>
	/// Gets whether two <see cref="DiscordVoiceRegion"/>s are equal.
	/// </summary>
	/// <param name="region">The region to compare with.</param>
	public bool Equals(DiscordVoiceRegion region)
		=> this == region;

	/// <summary>
	/// Whether two regions are equal.
	/// </summary>
	/// <param name="obj">A voice region.</param>
	public override bool Equals(object obj) => this.Equals(obj as DiscordVoiceRegion);

	/// <summary>
	/// Gets the hash code.
	/// </summary>
	public override int GetHashCode() => this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordVoiceRegion"/> objects are equal.
	/// </summary>
	/// <param name="left">First voice region to compare.</param>
	/// <param name="right">Second voice region to compare.</param>
	/// <returns>Whether the two voice regions are equal.</returns>
	public static bool operator ==(DiscordVoiceRegion left, DiscordVoiceRegion right)
	{
		var o1 = left as object;
		var o2 = right as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || left.Id == right.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordVoiceRegion"/> objects are not equal.
	/// </summary>
	/// <param name="left">First voice region to compare.</param>
	/// <param name="right">Second voice region to compare.</param>
	/// <returns>Whether the two voice regions are not equal.</returns>
	public static bool operator !=(DiscordVoiceRegion left, DiscordVoiceRegion right)
		=> !(left == right);

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordVoiceRegion"/> class.
	/// </summary>
	internal DiscordVoiceRegion()
	{ }
}
