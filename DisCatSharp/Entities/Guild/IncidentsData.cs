using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an incidents data object
/// </summary>
public class IncidentsData : ObservableApiObject
{
	/// <summary>
	/// Gets until when invites are disabled.
	/// </summary>
	[JsonProperty("invites_disabled_until", NullValueHandling = NullValueHandling.Include)]
	public DateTimeOffset? InvitesDisabledUntil { get; internal set; }

	/// <summary>
	/// Gets until when direct messages are disabled.
	/// </summary>
	[JsonProperty("dms_disabled_until", NullValueHandling = NullValueHandling.Include)]
	public DateTimeOffset? DmsDisabledUntil { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="IncidentsData"/> object.
	/// </summary>
	internal IncidentsData()
	{ }
}
