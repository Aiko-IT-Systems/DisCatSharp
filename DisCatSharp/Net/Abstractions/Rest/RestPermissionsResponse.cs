using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents permissions computed by Discord for a member or role.
/// </summary>
internal sealed class RestPermissionsResponse : ObservableApiObject
{
	/// <summary>
	///     Gets the computed permissions.
	/// </summary>
	[JsonProperty("permissions")]
	public Permissions Permissions { get; internal set; }
}

