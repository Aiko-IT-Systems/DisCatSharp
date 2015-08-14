using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a test entitlement create payload.
/// </summary>
internal sealed class TestEntitlementCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the sku to grant entitlement to, as Discord for this value.
	/// </summary>
	[JsonProperty("sku_id")]
	public ulong SkuId { get; set; }

	/// <summary>
	/// Gets or sets the guild id or user id to grant entitlement to.
	/// </summary>
	[JsonProperty("owner_id")]
	public ulong OwnerId { get; set; }

	/// <summary>
	/// Gets or sets the type of subscription (guild, user).
	/// </summary>
	[JsonProperty("owner_type")]
	public EntitlementOwnerType OwnerType { get; set; }
}
