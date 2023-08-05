// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
