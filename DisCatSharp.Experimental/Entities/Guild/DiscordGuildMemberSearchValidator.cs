using System;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Provides a validator for <see cref="DiscordGuildMemberSearchParams"/>.
/// </summary>
public static class DiscordGuildMemberSearchValidator
{
	/// <summary>
	/// Gets the max OR query item count.
	/// </summary>
	private const int MAX_OR_QUERY_ITEMS = 10;

	/// <summary>
	/// Gets the max AND query item count.
	/// </summary>
	private const int MAX_AND_QUERY_ITEMS = 10;

	/// <summary>
	/// Gets the max limit count.
	/// </summary>
	private const int MAX_LIMIT = 1000;

	/// <summary>
	/// Gets the min limit count.
	/// </summary>
	private const int MIN_LIMIT = 1;

	/// <summary>
	/// Gets the max query length.
	/// </summary>
	private const int MAX_QUERY_LENGTH = 100;

	/// <summary>
	/// Validates the guild member search parameters.
	/// </summary>
	/// <param name="searchParams">The search parameters to validate.</param>
	/// <returns>A tuple containing a boolean indicating validity and an optional error message.</returns>
	public static (bool IsValid, string? ErrorMessage) Validate(this DiscordGuildMemberSearchParams searchParams)
	{
		if (searchParams.Limit is < MIN_LIMIT or > MAX_LIMIT)
			return (false, $"Limit must be between {MIN_LIMIT} and {MAX_LIMIT}.");

		if (searchParams.OrQuery is not null)
		{
			var orQueryCheck = searchParams.OrQuery.ValidateQuery("OR", true);
			if (!orQueryCheck.IsValid)
				return orQueryCheck;
		}

		if (searchParams.AndQuery is not null)
		{
			var andQueryCheck = searchParams.AndQuery.ValidateQuery("AND", false);
			if (!andQueryCheck.IsValid)
				return andQueryCheck;
		}

		return (true, null);
	}

	/// <summary>
	/// Validates a DiscordMemberFilter query.
	/// </summary>
	private static (bool IsValid, string? ErrorMessage) ValidateQuery(this DiscordMemberFilter filter, string queryType, bool validateOrQuery)
	{
		if (filter.UserId is not null)
		{
			var result = filter.UserId.ValidateQueryConditions(queryType, "UserId", ValidateSnowflakeType, allowRange: true);
			if (!result.IsValid)
				return result;
		}

		if (filter.Usernames is not null && validateOrQuery)
		{
			var result = filter.Usernames.ValidateQueryConditions(queryType, "Usernames", ValidateStringType, true);
			if (!result.IsValid)
				return result;
		}
		else if (filter.Usernames is not null)
			return (false, $"Usernames can only be used in OR queries.");

		if (filter.RoleIds is not null)
		{
			var result = filter.RoleIds.ValidateQueryConditions(queryType, "RoleIds", ValidateSnowflakeType, true, true);
			if (!result.IsValid)
				return result;
		}

		if (filter.GuildJoinedAt is not null)
		{
			var result = filter.GuildJoinedAt.ValidateQueryConditions(queryType, "GuildJoinedAt", ValidateIntegerType, allowRange: true);
			if (!result.IsValid)
				return result;
		}

		if (filter.JoinSourceType is not null)
		{
			var result = filter.JoinSourceType.ValidateQueryConditions(queryType, "JoinSourceType", ValidateIntegerType, true);
			if (!result.IsValid)
				return result;
		}

		if (filter.SourceInviteCode is not null)
		{
			var result = filter.SourceInviteCode.ValidateQueryConditions(queryType, "SourceInviteCode", ValidateStringType, true);
			if (!result.IsValid)
				return result;
		}

		if (filter.SafetySignals is not null)
		{
			var safetySignalsCheck = ValidateSafetySignals(filter.SafetySignals);
			if (!safetySignalsCheck.IsValid)
				return safetySignalsCheck;
		}

		return (true, null);
	}

	/// <summary>
	/// Validates the conditions and types of an individual query within a filter.
	/// </summary>
	private static (bool IsValid, string? ErrorMessage) ValidateQueryConditions(this DiscordQuery query, string queryType, string fieldName, Func<string, bool> validateType, bool allowOr = false, bool allowAnd = false, bool allowRange = false)
	{
		if (query.OrQuery is not null && !allowOr)
			return (false, $"{fieldName} in {queryType} filter cannot be used with OR queries.");

		if (query.AndQuery is not null && !allowAnd)
			return (false, $"{fieldName} in {queryType} filter cannot be used with AND queries.");

		if (query.RangeQuery is not null && !allowRange)
			return (false, $"{fieldName} in {queryType} filter cannot be used with RANGE queries.");

		if (query.OrQuery is { Count: > MAX_OR_QUERY_ITEMS } or { Count: < 1 })
			return (false, $"{fieldName} OR query in {queryType} filter must have between 1 and {MAX_OR_QUERY_ITEMS} items.");

		if (query.AndQuery is { Count: > MAX_AND_QUERY_ITEMS } or { Count: < 1 })
			return (false, $"{fieldName} AND query in {queryType} filter must have between 1 and {MAX_AND_QUERY_ITEMS} items.");

		if (query.OrQuery is not null || query.AndQuery is not null)
			foreach (var item in query.OrQuery ?? query.AndQuery)
			{
				if (!validateType(item))
					return (false, $"{fieldName} in {queryType} filter contains an invalid type.");
				if (item.Length > MAX_QUERY_LENGTH)
					return (false, $"{fieldName} in {queryType} filter cannot exceed {MAX_QUERY_LENGTH} characters.");
			}

		if (query.RangeQuery is not null)
			if (query.RangeQuery.Gte is null && query.RangeQuery.Lte is null)
				return (false, $"{fieldName} range query in {queryType} filter must specify at least one bound (gte or lte).");

		return (true, null);
	}

	/// <summary>
	/// Validates safety signals queries.
	/// </summary>
	private static (bool IsValid, string? ErrorMessage) ValidateSafetySignals(this DiscordSafetySignals safetySignals)
	{
		if (safetySignals.UnusualDmActivityUntil is not null)
		{
			var result = safetySignals.UnusualDmActivityUntil.ValidateQueryConditions("SafetySignals", "UnusualDmActivityUntil", ValidateIntegerType, allowRange: true);
			if (!result.IsValid)
				return result;
		}

		if (safetySignals.CommunicationDisabledUntil is not null)
		{
			var result = safetySignals.CommunicationDisabledUntil.ValidateQueryConditions("SafetySignals", "CommunicationDisabledUntil", ValidateIntegerType, allowRange: true);
			if (!result.IsValid)
				return result;
		}

		return (true, null);
	}

	/// <summary>
	/// Validates that the query type is snowflake.
	/// </summary>
	private static bool ValidateSnowflakeType(this string query)
		=> ulong.TryParse(query, out _);

	/// <summary>
	/// Validates that the query type is integer.
	/// </summary>
	private static bool ValidateIntegerType(this string query)
		=> int.TryParse(query, out _);

	/// <summary>
	/// Validates that the query type is string.
	/// </summary>
	private static bool ValidateStringType(this string query)
		=> !string.IsNullOrWhiteSpace(query);
}
