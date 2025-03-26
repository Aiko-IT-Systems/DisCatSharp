namespace DisCatSharp.Enums;

/// <summary>
///    Represents the visibility of a guild profile.
/// </summary>
public enum GuildProfileVisibility : int
{
    /// <summary>
    ///    Not specified visibility.
    /// </summary>
    NotSpecified = 0,

    /// <summary>
    ///    Public visibility.
    /// </summary>
    Public = 1,

    /// <summary>
    ///    Restricted visibility.
    /// </summary>
    Restricted = 2,

    /// <summary>
    ///    Public with recruitment enabled.
    /// </summary>
    PublicWithRecruitment = 3
}
