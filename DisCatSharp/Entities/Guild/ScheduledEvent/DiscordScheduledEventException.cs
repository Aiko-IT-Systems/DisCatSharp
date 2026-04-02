using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Exceptions;
using DisCatSharp.Net.Models;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a guild scheduled event exception.
/// </summary>
public sealed class DiscordScheduledEventException : ObservableApiObject, IEquatable<DiscordScheduledEventException>
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordScheduledEventException" /> class.
	/// </summary>
	internal DiscordScheduledEventException()
	{ }

	/// <summary>
	///     Gets the guild id, when present in the payload.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	/// <summary>
	///     Gets the scheduled event id this exception belongs to.
	/// </summary>
	[JsonProperty("event_id")]
	public ulong EventId { get; internal set; }

	/// <summary>
	///     Gets the scheduled event exception id.
	/// </summary>
	[JsonProperty("event_exception_id")]
	public ulong Id { get; internal set; }

	/// <summary>
	///     Gets the rescheduled start time, if one exists.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? ScheduledStartTime
		=> !string.IsNullOrWhiteSpace(this.ScheduledStartTimeRaw) && DateTimeOffset.TryParse(this.ScheduledStartTimeRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets the rescheduled start time as the raw API string.
	/// </summary>
	[JsonProperty("scheduled_start_time", NullValueHandling = NullValueHandling.Ignore)]
	internal string? ScheduledStartTimeRaw { get; set; }

	/// <summary>
	///     Gets the rescheduled end time, if one exists.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? ScheduledEndTime
		=> !string.IsNullOrWhiteSpace(this.ScheduledEndTimeRaw) && DateTimeOffset.TryParse(this.ScheduledEndTimeRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets the rescheduled end time as the raw API string.
	/// </summary>
	[JsonProperty("scheduled_end_time", NullValueHandling = NullValueHandling.Ignore)]
	internal string? ScheduledEndTimeRaw { get; set; }

	/// <summary>
	///     Gets whether the recurrence is canceled.
	/// </summary>
	[JsonProperty("is_canceled")]
	public bool IsCanceled { get; internal set; }

	/// <summary>
	///     Gets the scheduled event this exception belongs to, if it can be resolved from cache.
	/// </summary>
	[JsonIgnore]
	public DiscordScheduledEvent? ScheduledEvent
	{
		get
		{
			return this.Discord is null
				? null
				: this.GuildId.HasValue && this.Discord.Guilds.TryGetValue(this.GuildId.Value, out var guild) && guild.ScheduledEvents.TryGetValue(this.EventId, out var scheduledEvent)
				? scheduledEvent
				: this.Discord.Guilds.Values.SelectMany(x => x.ScheduledEvents.Values).FirstOrDefault(x => x.Id == this.EventId);
		}
	}

	/// <summary>
	///     Modifies this scheduled event exception.
	/// </summary>
	/// <param name="action">The action to apply to the edit model.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The updated scheduled event exception.</returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the correct permissions.</exception>
	/// <exception cref="NotFoundException">Thrown when the event or exception does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordScheduledEventException> ModifyAsync(Action<ScheduledEventExceptionEditModel> action, CancellationToken cancellationToken = default)
	{
		var mdl = new ScheduledEventExceptionEditModel();
		action(mdl);

		var scheduledEvent = this.ScheduledEvent ?? throw new InvalidOperationException("Unable to resolve the parent scheduled event for this exception.");
		return this.Discord.ApiClient.ModifyGuildScheduledEventExceptionAsync(scheduledEvent.GuildId, this.EventId, this.Id, mdl.ScheduledStartTime, mdl.ScheduledEndTime, mdl.IsCanceled, mdl.AuditLogReason, cancellationToken: cancellationToken);
	}

	/// <summary>
	///     Deletes this scheduled event exception.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the correct permissions.</exception>
	/// <exception cref="NotFoundException">Thrown when the event or exception does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync(string? reason = null, CancellationToken cancellationToken = default)
	{
		var scheduledEvent = this.ScheduledEvent ?? throw new InvalidOperationException("Unable to resolve the parent scheduled event for this exception.");
		return this.Discord.ApiClient.DeleteGuildScheduledEventExceptionAsync(scheduledEvent.GuildId, this.EventId, this.Id, reason, cancellationToken: cancellationToken);
	}

	/// <summary>
	///     Gets users subscribed to this scheduled event exception.
	/// </summary>
	/// <param name="limit">The maximum number of users to return.</param>
	/// <param name="before">Get users before this user id.</param>
	/// <param name="after">Get users after this user id.</param>
	/// <param name="withMember">Whether to include guild member data.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>A map of subscribed users by user id.</returns>
	/// <exception cref="NotFoundException">Thrown when the event does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>

	public Task<IReadOnlyDictionary<ulong, DiscordScheduledEventUser>> GetUsersAsync(int? limit = null, ulong? before = null, ulong? after = null, bool? withMember = null, CancellationToken cancellationToken = default)
	{
		var scheduledEvent = this.ScheduledEvent ?? throw new InvalidOperationException("Unable to resolve the parent scheduled event for this exception.");
		return this.Discord.ApiClient.GetGuildScheduledEventExceptionUsersAsync(scheduledEvent.GuildId, this.EventId, this.Id, limit, before, after, withMember, cancellationToken: cancellationToken);
	}

	/// <summary>
	///     Checks whether this <see cref="DiscordScheduledEventException" /> is equal to another <see cref="DiscordScheduledEventException" />.
	/// </summary>
	/// <param name="other">The exception to compare to.</param>
	/// <returns>Whether the exceptions are equal.</returns>
	public bool Equals(DiscordScheduledEventException? other)
		=> other is not null && (ReferenceEquals(this, other) || HashCode.Combine(this.EventId, this.Id) == HashCode.Combine(other.EventId, other.Id));

	/// <summary>
	///     Checks whether this <see cref="DiscordScheduledEventException" /> is equal to another object.
	/// </summary>
	/// <param name="obj">The object to compare to.</param>
	/// <returns>Whether the object is equal to this exception.</returns>
	public override bool Equals(object? obj)
		=> this.Equals(obj as DiscordScheduledEventException);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordScheduledEventException" />.
	/// </summary>
	/// <returns>The hash code for this exception.</returns>
	public override int GetHashCode()
		=> HashCode.Combine(this.EventId, this.Id);

	/// <summary>
	///     Gets whether two <see cref="DiscordScheduledEventException" /> objects are equal.
	/// </summary>
	/// <param name="left">The first exception to compare.</param>
	/// <param name="right">The second exception to compare.</param>
	/// <returns>Whether the exceptions are equal.</returns>
	public static bool operator ==(DiscordScheduledEventException? left, DiscordScheduledEventException? right)
		=> Equals(left, right);

	/// <summary>
	///     Gets whether two <see cref="DiscordScheduledEventException" /> objects are not equal.
	/// </summary>
	/// <param name="left">The first exception to compare.</param>
	/// <param name="right">The second exception to compare.</param>
	/// <returns>Whether the exceptions are not equal.</returns>
	public static bool operator !=(DiscordScheduledEventException? left, DiscordScheduledEventException? right)
		=> !(left == right);
}
