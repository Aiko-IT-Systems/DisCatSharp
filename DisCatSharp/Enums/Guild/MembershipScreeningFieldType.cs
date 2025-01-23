using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a membership screening field type.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum MembershipScreeningFieldType
{
	/// <summary>
	///     Specifies the server rules
	/// </summary>
	[EnumMember(Value = "TERMS")]
	Terms,

	/// <summary>
	///     Specifies a text input question.
	/// </summary>
	[EnumMember(Value = "TEXT_INPUT")]
	TextInput,

	/// <summary>
	///    Specifies a paragraph question.
	/// </summary>
	[EnumMember(Value = "PARAGRAPH")]
	Paragraph,

	/// <summary>
	///    Specifies a multiple choice question.
	/// </summary>
	[EnumMember(Value = "MULTIPLE_CHOICE")]
	MultipleChoice
}
