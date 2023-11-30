using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

public sealed class InteractionCallbackHint
{
	/*[JsonProperty("allowed_callback_type", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionCallbackTypes AllowedCallbackType { get; internal set; }*/

	[JsonProperty("ephemerality", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionCallbackEphemerality Ephemerality { get; internal set; }

	[JsonProperty("required_permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions RequiredPermissions { get; internal set; }
}
