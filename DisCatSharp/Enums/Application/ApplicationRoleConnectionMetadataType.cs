// This file is part of the DisCatSharp project, based off DSharpPlus.
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the type of an <see cref="DisCatSharp.Entities.DiscordApplicationRoleConnectionMetadata"/>.
/// </summary>
public enum ApplicationRoleConnectionMetadataType
{
	/// <summary>
	/// The metadata value (`integer`) is less than or equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerLessThanOrÈqual = 1,

	/// <summary>
	/// The metadata value (`integer`) is greater than or equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerGreaterThanOrÈqual = 2,

	/// <summary>
	/// The metadata value (`integer`) is equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerEqual = 3,

	/// <summary>
	/// The metadata value (`integer`) is not equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerNotEqual = 4,

	/// <summary>
	/// The metadata value (`ISO8601 string`) is less than or equal to the guild's configured value (`integer`; `days before current date`).
	/// </summary>
	DatetimeLessThanOrÈqual = 5,

	/// <summary>
	/// The metadata value (`ISO8601 string`) is greater than or equal to the guild's configured value (`integer`; `days before current date`).
	/// </summary>
	DatetimeGreaterThanOrÈqual = 6,

	/// <summary>
	/// The metadata value (`integer`) is equal to the guild's configured value (`integer`; `1`).
	/// </summary>
	BooleanEqual = 7,

	/// <summary>
	/// The metadata value (`integer`) is not equal to the guild's configured value (`integer`; `1`).
	/// </summary>
	BooleanNotEqual = 8
}
