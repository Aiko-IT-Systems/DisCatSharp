using System;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Sets a minimum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class MinimumValueAttribute : Attribute
{
	/// <summary>
	/// The value.
	/// </summary>
	public object Value { get; internal set; }

	/// <summary>
	/// Sets a minimum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
	/// </summary>
	public MinimumValueAttribute(int value)
	{
		this.Value = value;
	}

	/// <summary>
	/// Sets a minimum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
	/// </summary>
	public MinimumValueAttribute(long value)
	{
		this.Value = value;
	}

	/// <summary>
	/// Sets a minimum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
	/// </summary>
	public MinimumValueAttribute(double value)
	{
		this.Value = value;
	}
}

/// <summary>
/// Sets a maximum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class MaximumValueAttribute : Attribute
{
	/// <summary>
	/// The value.
	/// </summary>
	public object Value { get; internal set; }

	/// <summary>
	/// Sets a maximum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
	/// </summary>
	public MaximumValueAttribute(int value)
	{
		this.Value = value;
	}

	/// <summary>
	/// Sets a maximum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
	/// </summary>
	public MaximumValueAttribute(long value)
	{
		this.Value = value;
	}

	/// <summary>
	/// Sets a maximum value for this slash command option. Only valid for <see cref="int"/>, <see cref="long"/> or <see cref="double"/> parameters.
	/// </summary>
	public MaximumValueAttribute(double value)
	{
		this.Value = value;
	}
}

/// <summary>
/// Sets a minimum value for this slash command option. Only valid for <see cref="string"/> parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class MinimumLengthAttribute : Attribute
{
	/// <summary>
	/// The value.
	/// </summary>
	public int? Value { get; internal set; }

	/// <summary>
	/// Sets a minimum value for this slash command option. Only valid for <see cref="string"/> parameters.
	/// </summary>
	public MinimumLengthAttribute(int value)
	{
		if (value > 600)
			throw new ArgumentException("Minimum cannot be more than 6000.");

		this.Value = value;
	}
}

/// <summary>
/// Sets a maximum value for this slash command option.  Only valid for <see cref="string"/> parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class MaximumLengthAttribute : Attribute
{
	/// <summary>
	/// The value.
	/// </summary>
	public int? Value { get; internal set; }

	/// <summary>
	/// Sets a maximum value for this slash command option. Only valid for <see cref="string"/> parameters.
	/// </summary>
	public MaximumLengthAttribute(int value)
	{
		if (value == 0 || value > 600)
			throw new ArgumentException("Maximum length cannot be less than 1 and cannot be more than 6000.");

		this.Value = value;
	}
}
