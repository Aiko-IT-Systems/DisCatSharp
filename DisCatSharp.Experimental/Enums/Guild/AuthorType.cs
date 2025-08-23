using System.Runtime.Serialization;

namespace DisCatSharp.Experimental.Enums;

/// <summary>
/// Represents the type of author for message search.
/// </summary>
public enum AuthorType
{
    /// <summary>
    /// Represents a user.
    /// </summary>
    [EnumMember(Value = "user")]
    User,

    /// <summary>
    /// Represents a bot.
    /// </summary>
    [EnumMember(Value = "bot")]
    Bot,

    /// <summary>
    /// Represents a webhook.
    /// </summary>
    [EnumMember(Value = "webhook")]
    Webhook,

    /// <summary>
    /// Represents no user.
    /// </summary>
    [EnumMember(Value = "-user")]
    NoUser,

    /// <summary>
    /// Represents no bot.
    /// </summary>
    [EnumMember(Value = "-bot")]
    NoBot,

    /// <summary>
    /// Represents no webhook.
    /// </summary>
    [EnumMember(Value = "-webhook")]
    NoWebhook
}
