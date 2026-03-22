using System;
using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents parameters for interaction commands.
/// </summary>
public sealed class DiscordInteractionDataOption : ObservableApiObject
{
	/// <summary>
	///     Gets the name of this interaction parameter.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	///     Gets the type of this interaction parameter.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandOptionType Type { get; internal set; }

	/// <summary>
	///     Whether this option is currently focused by the user.
	///     Only applicable for autocomplete option choices.
	/// </summary>
	[JsonProperty("focused")]
	public bool Focused { get; internal set; }

	/// <summary>
	///     Gets the value of this interaction parameter.
	/// </summary>
	[JsonProperty("value")]
	internal string RawValue { get; set; }

	/// <summary>
	///     Gets the raw, unparsed interaction value exactly as it arrived from Discord.
	/// </summary>
	/// <remarks>
	///     This is especially useful for autocomplete interactions, where the currently focused option can contain
	///     partially typed text that does not yet satisfy the declared command option type.
	/// </remarks>
	[JsonIgnore]
	public string RawValueString => this.RawValue;

	/// <summary>
	///     Gets the value of this interaction parameter.
	///     <para>
	///         This can be cast to a <see langword="long" />, <see langword="bool"></see>, <see langword="string"></see>,
	///         <see langword="double"></see> or <see langword="ulong" /> depending on the <see cref="System.Type" />
	///     </para>
	/// </summary>
	[JsonIgnore]
	public object Value
	{
		get
		{
			if (string.IsNullOrEmpty(this.RawValue))
				return null!;

			if (this.Type == ApplicationCommandOptionType.String)
				return this.RawValue;

			try
			{
				return this.Type == ApplicationCommandOptionType.Integer && int.TryParse(this.RawValue, out var raw)
					? raw
					: this.Type == ApplicationCommandOptionType.Integer
						? long.Parse(this.RawValue)
						: this.Type switch
						{
							ApplicationCommandOptionType.Boolean => bool.Parse(this.RawValue),
							ApplicationCommandOptionType.Channel => ulong.Parse(this.RawValue),
							ApplicationCommandOptionType.User => ulong.Parse(this.RawValue),
							ApplicationCommandOptionType.Role => ulong.Parse(this.RawValue),
							ApplicationCommandOptionType.Mentionable => ulong.Parse(this.RawValue),
							ApplicationCommandOptionType.Number => double.Parse(this.RawValue),
							ApplicationCommandOptionType.Attachment => ulong.Parse(this.RawValue),
							_ => this.RawValue
						};
			}
			catch (FormatException) when (this.Focused)
			{
				// Autocomplete payloads can carry in-progress text that does not yet satisfy the declared option type.
				// Returning the raw string here lets providers inspect what the user typed without blowing up the
				// interaction pipeline on intermediate values like "a" for an integer option.
				return this.RawValue;
			}
		}
	}

	/// <summary>
	///     Attempts to parse the current interaction option value into the requested type without throwing.
	/// </summary>
	/// <typeparam name="T">The target value type to read.</typeparam>
	/// <param name="value">The parsed value when successful; otherwise the default value for <typeparamref name="T" />.</param>
	/// <returns><see langword="true" /> when the value could be converted to <typeparamref name="T" />; otherwise <see langword="false" />.</returns>
	/// <remarks>
	///     This is especially useful for autocomplete providers, where focused options can contain partially typed text
	///     that is not yet valid for the declared command option type.
	/// </remarks>
	public bool TryGetValue<T>(out T? value)
	{
		value = default;

		if (string.IsNullOrEmpty(this.RawValue))
			return false;

		var requestedType = typeof(T);
		var underlyingType = Nullable.GetUnderlyingType(requestedType) ?? requestedType;
		var rawValue = this.RawValue;

		try
		{
			object parsedValue;
			if (underlyingType == typeof(string))
			{
				parsedValue = rawValue;
			}
			else if (underlyingType == typeof(int))
			{
				if (!int.TryParse(rawValue, out var parsedInt))
					return false;

				parsedValue = parsedInt;
			}
			else if (underlyingType == typeof(long))
			{
				if (!long.TryParse(rawValue, out var parsedLong))
					return false;

				parsedValue = parsedLong;
			}
			else if (underlyingType == typeof(bool))
			{
				if (!bool.TryParse(rawValue, out var parsedBool))
					return false;

				parsedValue = parsedBool;
			}
			else if (underlyingType == typeof(double))
			{
				if (!double.TryParse(rawValue, out var parsedDouble))
					return false;

				parsedValue = parsedDouble;
			}
			else if (underlyingType == typeof(ulong))
			{
				if (!ulong.TryParse(rawValue, out var parsedUnsignedLong))
					return false;

				parsedValue = parsedUnsignedLong;
			}
			else
			{
				return false;
			}

			value = (T)parsedValue;
			return true;
		}
		catch (InvalidCastException)
		{
			return false;
		}
	}

	/// <summary>
	///     Gets the additional parameters if this parameter is a subcommand.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordInteractionDataOption> Options { get; internal set; }
}
