namespace DisCatSharp.Enums;

/// <summary>
/// Represents a directory entries primary category type.
/// </summary>
public enum DirectoryCategory
{
	/// <summary>
	/// Indicates that this entry falls under the category Clubs.
	/// </summary>
	Clubs = 1,

	/// <summary>
	/// Indicates that this entry falls under the category Classes.
	/// </summary>
	Classes = 2,

	/// <summary>
	/// Indicates that this entry falls under the category Social and Study.
	/// </summary>
	SocialAndStudy = 3,

	/// <summary>
	/// Indicates that this entry falls under the category Majors and Subjects.
	/// </summary>
	MajorsAndSubjects = 4,

	/// <summary>
	/// Indicates that this entry falls under the category Miscellaneous.
	/// </summary>
	Miscellaneous = 5,

	/// <summary>
	/// Indicates unknown category type.
	/// </summary>
	Unknown = int.MaxValue
}
