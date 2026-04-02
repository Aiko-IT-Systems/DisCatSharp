using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Attributes;
using DisCatSharp.Exceptions;

namespace DisCatSharp;

/// <summary>
///     Configuration for Discord client telemetry and Sentry error reporting.
/// </summary>
public sealed class TelemetryConfiguration
{
	/// <summary>
	///     The exceptions tracked with Sentry.
	/// </summary>
	private List<Type> _exceptions = [typeof(ServerErrorException), typeof(BadRequestException), typeof(DiscordJsonException)];

	/// <summary>
	///     Creates a new telemetry configuration with default values.
	/// </summary>
	public TelemetryConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another telemetry configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public TelemetryConfiguration(TelemetryConfiguration other)
	{
		this.EnableSentry = other.EnableSentry;
		this.AttachRecentLogEntries = other.AttachRecentLogEntries;
		this.AttachUserInfo = other.AttachUserInfo;
		this.FeedbackEmail = other.FeedbackEmail;
		this.DeveloperUserId = other.DeveloperUserId;
		this.EnableDiscordIdScrubber = other.EnableDiscordIdScrubber;
		this._exceptions = other._exceptions;
		this.DisableScrubber = other.DisableScrubber;
		this.SentryDebug = other.SentryDebug;
		this.DisableExceptionFilter = other.DisableExceptionFilter;
		this.CustomSentryDsn = other.CustomSentryDsn;
	}

	/// <summary>
	///     <para>Whether to enable sentry.</para>
	///     <para>This helps us to track missing data and library bugs better.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	///     <para>
	///         <note type="note">
	///             Please refer to the <a href="https://docs.dcs.aitsys.dev/articles/misc/sentry">docs</a> for
	///             more information.
	///         </note>
	///     </para>
	/// </summary>
	public bool EnableSentry { internal get; set; } = false;

	/// <summary>
	///     <para>Whether to attach recent log entries.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool AttachRecentLogEntries { internal get; set; } = false;

	/// <summary>
	///     <para>Whether to attach the bots username and id to sentry reports.</para>
	///     <para>This helps us to pinpoint problems.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool AttachUserInfo { internal get; set; } = false;

	/// <summary>
	///     <para>Your email address we can reach out when your bot encounters library bugs.</para>
	///     <para>Will only be transmitted if <see cref="AttachUserInfo" /> is <see langword="true" />.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	public string? FeedbackEmail { internal get; set; } = null;

	/// <summary>
	///     <para>Your discord user id we can reach out when your bot encounters library bugs.</para>
	///     <para>Will only be transmitted if <see cref="AttachUserInfo" /> is <see langword="true" />.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	public ulong? DeveloperUserId { internal get; set; } = null;

	/// <summary>
	///     <para>Whether to replace every discord-based id with <c>{DISCORD_ID}</c>.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool EnableDiscordIdScrubber { internal get; set; } = false;

	/// <summary>
	///     <para>Sets which exceptions to track with sentry.</para>
	///     <para>Do not touch this unless you're developing the library.</para>
	/// </summary>
	/// <exception cref="InvalidOperationException">
	///     Thrown when the base type of all exceptions is not
	///     <see cref="DisCatSharpException" />.
	/// </exception>
	internal List<Type> TrackExceptions
	{
		get => this._exceptions;
		set
		{
			if (value is null)
				this._exceptions.Clear();
			else
				this._exceptions = value.All(val => val.BaseType == typeof(DisCatSharpException))
					? value
					: throw new InvalidOperationException("Can only track exceptions who inherit from " + nameof(DisCatSharpException) + " and must be constructed with typeof(Type)");
		}
	}

	/// <summary>
	///     <para>Whether to disable all safety scrubbers.</para>
	///     <para>Can only be enabled by whitelisted developers.</para>
	/// </summary>
	internal bool DisableScrubber { get; set; } = false;

	/// <summary>
	///     Whether to turn sentry's debug mode on.
	/// </summary>
	internal bool SentryDebug { get; set; } = false;

	/// <summary>
	///     Whether to disable the exception filter.
	/// </summary>
	internal bool DisableExceptionFilter { get; set; } = false;

	/// <summary>
	///     Custom Sentry Dsn.
	/// </summary>
	internal string? CustomSentryDsn { get; set; } = null;
}
