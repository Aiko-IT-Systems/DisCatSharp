using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a response to a field in a guild's membership screening form.
/// </summary>
public sealed class DiscordGuildMembershipScreeningFieldResponse : DiscordGuildMembershipScreeningField
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordGuildMembershipScreeningFieldResponse" /> class.
	/// </summary>
	internal DiscordGuildMembershipScreeningFieldResponse()
	{ }
	/// <summary>
	///     Gets the response. Type varies depending on <see cref="DiscordGuildMembershipScreeningField.FieldType"/>
	/// 	<list type="bullet">
	/// 		<item>
	/// 			<see cref="MembershipScreeningFieldType.TextInput" />: <see langword="string"/> (applicant's text response).
	/// 		</item>
	/// 		<item>
	/// 			<see cref="MembershipScreeningFieldType.Paragraph" />: <see langword="string"/> (applicant's text response).
	/// 		</item>
	/// 		<item>
	/// 			<see cref="MembershipScreeningFieldType.MultipleChoice" />: <see langword="int"/> (index of the choice selected by the applicant).
	/// 		</item>
	/// 		<item>
	/// 			<see cref="MembershipScreeningFieldType.Terms" />: <see langword="bool"/> (whether the applicant accepted the terms).
	/// 		</item>
	/// 	</list>
	/// </summary>
	[JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
	public object? Response { get; internal set; }
}
