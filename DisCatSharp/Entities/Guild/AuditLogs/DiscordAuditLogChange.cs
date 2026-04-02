using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a single Discord audit log change object.
/// </summary>
public sealed class DiscordAuditLogChange
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordAuditLogChange"/> class.
	/// </summary>
	/// <param name="key">The Discord change key.</param>
	/// <param name="oldValue">The raw old value supplied by Discord.</param>
	/// <param name="newValue">The raw new value supplied by Discord.</param>
	internal DiscordAuditLogChange(string key, JToken? oldValue, JToken? newValue)
	{
		this.Key = key;
		this.OldValue = oldValue;
		this.NewValue = newValue;
	}

	/// <summary>
	///     Gets the Discord change key.
	/// </summary>
	public string Key { get; }

	/// <summary>
	///     Gets the old raw value.
	/// </summary>
	public JToken? OldValue { get; }

	/// <summary>
	///     Gets whether Discord supplied an old value.
	/// </summary>
	public bool HasOldValue
		=> this.OldValue is not null;

	/// <summary>
	///     Gets the new raw value.
	/// </summary>
	public JToken? NewValue { get; }

	/// <summary>
	///     Gets whether Discord supplied a new value.
	/// </summary>
	public bool HasNewValue
		=> this.NewValue is not null;

	/// <summary>
	///     Converts the old raw value to the specified type.
	/// </summary>
	/// <typeparam name="T">The target CLR type.</typeparam>
	/// <returns>The converted value, or the default value of <typeparamref name="T" /> when no old value exists.</returns>
	/// <exception cref="Newtonsoft.Json.JsonException">
	///     Thrown when the raw token cannot be converted to <typeparamref name="T" />.
	/// </exception>
	/// <remarks>
	///     Discord may change the shape of individual change values between actions, so callers should prefer nullable
	///     or defensive target types when reading undocumented keys.
	/// </remarks>
	public T? GetOldValue<T>()
		=> this.OldValue is null ? default : this.OldValue.ToObject<T>();

	/// <summary>
	///     Converts the new raw value to the specified type.
	/// </summary>
	/// <typeparam name="T">The target CLR type.</typeparam>
	/// <returns>The converted value, or the default value of <typeparamref name="T" /> when no new value exists.</returns>
	/// <exception cref="Newtonsoft.Json.JsonException">
	///     Thrown when the raw token cannot be converted to <typeparamref name="T" />.
	/// </exception>
	/// <remarks>
	///     Discord may change the shape of individual change values between actions, so callers should prefer nullable
	///     or defensive target types when reading undocumented keys.
	/// </remarks>
	public T? GetNewValue<T>()
		=> this.NewValue is null ? default : this.NewValue.ToObject<T>();

	/// <summary>
	///     Converts the old raw value to a snowflake when possible.
	/// </summary>
	/// <returns>The parsed identifier, or <see langword="null" /> when the value is absent or malformed.</returns>
	/// <remarks>
	///     Discord commonly serializes snowflakes as strings inside audit log changes.
	/// </remarks>
	public ulong? GetOldSnowflake()
		=> TryGetSnowflake(this.OldValue, out var id) ? id : null;

	/// <summary>
	///     Converts the new raw value to a snowflake when possible.
	/// </summary>
	/// <returns>The parsed identifier, or <see langword="null" /> when the value is absent or malformed.</returns>
	/// <remarks>
	///     Discord commonly serializes snowflakes as strings inside audit log changes.
	/// </remarks>
	public ulong? GetNewSnowflake()
		=> TryGetSnowflake(this.NewValue, out var id) ? id : null;

	/// <summary>
	///     Converts the old raw value to a boolean when possible.
	/// </summary>
	/// <returns>The parsed boolean, or <see langword="null" /> when the value is absent or malformed.</returns>
	public bool? GetOldBoolean()
		=> TryGetBoolean(this.OldValue, out var value) ? value : null;

	/// <summary>
	///     Converts the new raw value to a boolean when possible.
	/// </summary>
	/// <returns>The parsed boolean, or <see langword="null" /> when the value is absent or malformed.</returns>
	public bool? GetNewBoolean()
		=> TryGetBoolean(this.NewValue, out var value) ? value : null;

	/// <summary>
	///     Converts the old raw value to a 32-bit integer when possible.
	/// </summary>
	/// <returns>The parsed integer, or <see langword="null" /> when the value is absent or malformed.</returns>
	public int? GetOldInt32()
		=> TryGetInt32(this.OldValue, out var value) ? value : null;

	/// <summary>
	///     Converts the new raw value to a 32-bit integer when possible.
	/// </summary>
	/// <returns>The parsed integer, or <see langword="null" /> when the value is absent or malformed.</returns>
	public int? GetNewInt32()
		=> TryGetInt32(this.NewValue, out var value) ? value : null;

	/// <summary>
	///     Converts the old raw value to a 64-bit integer when possible.
	/// </summary>
	/// <returns>The parsed integer, or <see langword="null" /> when the value is absent or malformed.</returns>
	public long? GetOldInt64()
		=> TryGetInt64(this.OldValue, out var value) ? value : null;

	/// <summary>
	///     Converts the new raw value to a 64-bit integer when possible.
	/// </summary>
	/// <returns>The parsed integer, or <see langword="null" /> when the value is absent or malformed.</returns>
	public long? GetNewInt64()
		=> TryGetInt64(this.NewValue, out var value) ? value : null;

	/// <summary>
	///     Converts the old raw value to a <see cref="DateTimeOffset" /> when possible.
	/// </summary>
	/// <returns>The parsed timestamp, or <see langword="null" /> when the value is absent or malformed.</returns>
	public DateTimeOffset? GetOldDateTimeOffset()
		=> TryGetDateTimeOffset(this.OldValue, out var value) ? value : null;

	/// <summary>
	///     Converts the new raw value to a <see cref="DateTimeOffset" /> when possible.
	/// </summary>
	/// <returns>The parsed timestamp, or <see langword="null" /> when the value is absent or malformed.</returns>
	public DateTimeOffset? GetNewDateTimeOffset()
		=> TryGetDateTimeOffset(this.NewValue, out var value) ? value : null;

	/// <summary>
	///     Converts the old raw value to the specified enum type when possible.
	/// </summary>
	/// <typeparam name="TEnum">The target enum type.</typeparam>
	/// <returns>The parsed enum value, or <see langword="null" /> when the value is absent or malformed.</returns>
	public TEnum? GetOldEnum<TEnum>() where TEnum : struct, Enum
		=> TryGetEnum(this.OldValue, out TEnum value) ? value : null;

	/// <summary>
	///     Converts the new raw value to the specified enum type when possible.
	/// </summary>
	/// <typeparam name="TEnum">The target enum type.</typeparam>
	/// <returns>The parsed enum value, or <see langword="null" /> when the value is absent or malformed.</returns>
	public TEnum? GetNewEnum<TEnum>() where TEnum : struct, Enum
		=> TryGetEnum(this.NewValue, out TEnum value) ? value : null;

	/// <summary>
	///     Converts the old raw value to a permissions bitset when possible.
	/// </summary>
	/// <returns>The parsed permissions, or <see langword="null" /> when the value is absent or malformed.</returns>
	/// <remarks>
	///     Discord commonly serializes permissions as decimal strings inside audit log change payloads.
	/// </remarks>
	public Permissions? GetOldPermissions()
		=> TryGetPermissions(this.OldValue, out var value) ? value : null;

	/// <summary>
	///     Converts the new raw value to a permissions bitset when possible.
	/// </summary>
	/// <returns>The parsed permissions, or <see langword="null" /> when the value is absent or malformed.</returns>
	/// <remarks>
	///     Discord commonly serializes permissions as decimal strings inside audit log change payloads.
	/// </remarks>
	public Permissions? GetNewPermissions()
		=> TryGetPermissions(this.NewValue, out var value) ? value : null;

	/// <summary>
	///     Converts the raw change into a typed scalar value change.
	/// </summary>
	/// <typeparam name="T">The target CLR type.</typeparam>
	/// <returns>A typed before-and-after value change.</returns>
	public DiscordAuditLogValueChange<T> ToValueChange<T>()
		=> new(this.HasOldValue, this.GetOldValue<T>(), this.HasNewValue, this.GetNewValue<T>());

	/// <summary>
	///     Converts the raw change into a typed collection change.
	/// </summary>
	/// <typeparam name="T">The target item type.</typeparam>
	/// <returns>A typed before-and-after collection change.</returns>
	public DiscordAuditLogCollectionChange<T> ToCollectionChange<T>()
		=> new(this.HasOldValue, GetCollectionValues<T>(this.OldValue), this.HasNewValue, GetCollectionValues<T>(this.NewValue));

	/// <summary>
	///     Converts the raw change into a snowflake value change.
	/// </summary>
	/// <returns>A typed before-and-after snowflake change.</returns>
	public DiscordAuditLogValueChange<ulong?> ToSnowflakeChange()
		=> new(this.HasOldValue, this.GetOldSnowflake(), this.HasNewValue, this.GetNewSnowflake());

	/// <summary>
	///     Converts the raw change into a <see cref="DateTimeOffset" /> value change.
	/// </summary>
	/// <returns>A typed before-and-after timestamp change.</returns>
	public DiscordAuditLogValueChange<DateTimeOffset?> ToDateTimeOffsetChange()
		=> new(this.HasOldValue, this.GetOldDateTimeOffset(), this.HasNewValue, this.GetNewDateTimeOffset());

	/// <summary>
	///     Converts the raw change into a 32-bit integer value change.
	/// </summary>
	/// <returns>A typed before-and-after integer change.</returns>
	public DiscordAuditLogValueChange<int?> ToInt32Change()
		=> new(this.HasOldValue, this.GetOldInt32(), this.HasNewValue, this.GetNewInt32());

	/// <summary>
	///     Converts the raw change into a permissions value change.
	/// </summary>
	/// <returns>A typed before-and-after permissions change.</returns>
	public DiscordAuditLogValueChange<Permissions?> ToPermissionsChange()
		=> new(this.HasOldValue, this.GetOldPermissions(), this.HasNewValue, this.GetNewPermissions());

	/// <summary>
	///     Converts the raw change into a partial role collection change.
	/// </summary>
	/// <returns>A typed before-and-after partial role collection change.</returns>
	/// <remarks>
	///     This is primarily useful for Discord's special <c>$add</c> and <c>$remove</c> member role delta keys.
	/// </remarks>
	public DiscordAuditLogCollectionChange<DiscordAuditLogPartialRole> ToPartialRoleCollectionChange()
		=> new(this.HasOldValue, GetPartialRoles(this.OldValue), this.HasNewValue, GetPartialRoles(this.NewValue));

	/// <summary>
	///     Converts the raw change into a snowflake collection change.
	/// </summary>
	/// <returns>A typed before-and-after snowflake collection change.</returns>
	public DiscordAuditLogCollectionChange<ulong> ToSnowflakeCollectionChange()
		=> new(this.HasOldValue, GetSnowflakeCollection(this.OldValue), this.HasNewValue, GetSnowflakeCollection(this.NewValue));

	/// <summary>
	///     Converts the change back into the raw Discord object shape.
	/// </summary>
	/// <returns>A raw object containing the original key and values.</returns>
	/// <remarks>
	///     The returned object uses deep-cloned tokens so callers can mutate it without affecting the cached entry state.
	/// </remarks>
	internal JObject ToRawObject()
	{
		var raw = new JObject
		{
			["key"] = this.Key
		};

		if (this.OldValue is not null)
			raw["old_value"] = this.OldValue.DeepClone();

		if (this.NewValue is not null)
			raw["new_value"] = this.NewValue.DeepClone();

		return raw;
	}

	/// <summary>
	///     Attempts to parse a snowflake from a raw audit log token.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <param name="id">When this method returns, contains the parsed snowflake if successful.</param>
	/// <returns><see langword="true" /> when the token contains a valid snowflake; otherwise <see langword="false" />.</returns>
	private static bool TryGetSnowflake(JToken? token, out ulong id)
	{
		id = default;
		return token is not null && token.Type switch
		{
			JTokenType.Integer => ulong.TryParse(token.Value<long>().ToString(CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out id),
			JTokenType.String => ulong.TryParse(token.Value<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id),
			_ => false
		};
	}

	/// <summary>
	///     Attempts to parse a boolean from a raw audit log token.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <param name="value">When this method returns, contains the parsed boolean if successful.</param>
	/// <returns><see langword="true" /> when the token contains a valid boolean; otherwise <see langword="false" />.</returns>
	private static bool TryGetBoolean(JToken? token, out bool value)
	{
		value = default;
		return token is not null && token.Type switch
		{
			JTokenType.Boolean => (value = token.Value<bool>()) is var _,
			JTokenType.String => bool.TryParse(token.Value<string>(), out value),
			JTokenType.Integer => (value = token.Value<long>() != 0) is var _,
			_ => false
		};
	}

	/// <summary>
	///     Attempts to parse a 32-bit integer from a raw audit log token.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <param name="value">When this method returns, contains the parsed integer if successful.</param>
	/// <returns><see langword="true" /> when the token contains a valid integer; otherwise <see langword="false" />.</returns>
	private static bool TryGetInt32(JToken? token, out int value)
	{
		value = default;
		return token is not null && token.Type switch
		{
			JTokenType.Integer => int.TryParse(token.Value<long>().ToString(CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out value),
			JTokenType.String => int.TryParse(token.Value<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value),
			_ => false
		};
	}

	/// <summary>
	///     Attempts to parse a 64-bit integer from a raw audit log token.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <param name="value">When this method returns, contains the parsed integer if successful.</param>
	/// <returns><see langword="true" /> when the token contains a valid integer; otherwise <see langword="false" />.</returns>
	private static bool TryGetInt64(JToken? token, out long value)
	{
		value = default;
		return token is not null && token.Type switch
		{
			JTokenType.Integer => (value = token.Value<long>()) is var _,
			JTokenType.String => long.TryParse(token.Value<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value),
			_ => false
		};
	}

	/// <summary>
	///     Attempts to parse a timestamp from a raw audit log token.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <param name="value">When this method returns, contains the parsed timestamp if successful.</param>
	/// <returns><see langword="true" /> when the token contains a valid timestamp; otherwise <see langword="false" />.</returns>
	private static bool TryGetDateTimeOffset(JToken? token, out DateTimeOffset value)
	{
		value = default;
		return token is not null && token.Type switch
		{
			JTokenType.Date => (value = token.Value<DateTimeOffset>()) is var _,
			JTokenType.String => DateTimeOffset.TryParse(token.Value<string>(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value),
			_ => false
		};
	}

	/// <summary>
	///     Attempts to parse an enum value from a raw audit log token.
	/// </summary>
	/// <typeparam name="TEnum">The target enum type.</typeparam>
	/// <param name="token">The token to inspect.</param>
	/// <param name="value">When this method returns, contains the parsed enum value if successful.</param>
	/// <returns><see langword="true" /> when the token contains a valid enum representation; otherwise <see langword="false" />.</returns>
	private static bool TryGetEnum<TEnum>(JToken? token, out TEnum value) where TEnum : struct, Enum
	{
		value = default;
		if (token is null)
			return false;

		if (token.Type is JTokenType.String)
			return Enum.TryParse(token.Value<string>(), ignoreCase: true, out value);

		if (token.Type is not JTokenType.Integer)
			return false;

		var rawValue = token.Value<long>();
		value = (TEnum)Enum.ToObject(typeof(TEnum), rawValue);
		return true;
	}

	/// <summary>
	///     Attempts to parse a permissions bitset from a raw audit log token.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <param name="value">When this method returns, contains the parsed permissions if successful.</param>
	/// <returns><see langword="true" /> when the token contains a valid permissions value; otherwise <see langword="false" />.</returns>
	private static bool TryGetPermissions(JToken? token, out Permissions value)
	{
		value = default;
		if (!TryGetInt64(token, out var rawValue))
			return false;

		value = (Permissions)rawValue;
		return true;
	}

	/// <summary>
	///     Converts a raw token into a typed collection when possible.
	/// </summary>
	/// <typeparam name="T">The target item type.</typeparam>
	/// <param name="token">The token to inspect.</param>
	/// <returns>The converted collection, or <see langword="null" /> when the token is absent or malformed.</returns>
	private static IReadOnlyList<T>? GetCollectionValues<T>(JToken? token)
	{
		if (token is null)
			return null;

		try
		{
			return token.ToObject<List<T>>();
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	///     Converts a raw token into Discord's partial role array structure when possible.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <returns>The converted partial role collection, or <see langword="null" /> when the token is absent or malformed.</returns>
	private static IReadOnlyList<DiscordAuditLogPartialRole>? GetPartialRoles(JToken? token)
	{
		if (token is not JArray array)
			return null;

		var roles = new List<DiscordAuditLogPartialRole>(array.Count);
		foreach (var element in array)
		{
			if (element is not JObject roleObject)
				return null;

			if (!TryGetSnowflake(roleObject["id"], out var id))
				return null;

			roles.Add(new(id, roleObject.Value<string>("name")));
		}

		return roles;
	}

	/// <summary>
	///     Converts a raw token into a snowflake collection when possible.
	/// </summary>
	/// <param name="token">The token to inspect.</param>
	/// <returns>The converted snowflake collection, or <see langword="null" /> when the token is absent or malformed.</returns>
	private static IReadOnlyList<ulong>? GetSnowflakeCollection(JToken? token)
	{
		if (token is not JArray array)
			return null;

		var ids = new List<ulong>(array.Count);
		foreach (var element in array)
		{
			if (!TryGetSnowflake(element, out var id))
				return null;

			ids.Add(id);
		}

		return ids;
	}
}
