// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

// ReSharper disable InconsistentNaming

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a guild's content level.
/// </summary>
public enum NsfwLevel
{
	/// <summary>
	/// Indicates the guild has no special NSFW level.
	/// </summary>
	Default = 0,

	/// <summary>
	/// Indicates the guild has extremely suggestive or mature content that would only be suitable for users over 18
	/// </summary>
	Explicit = 1,

	/// <summary>
	/// Indicates the guild has no content that could be deemed NSFW. It is SFW.
	/// </summary>
	Safe = 2,

	/// <summary>
	/// Indicates the guild has mildly NSFW content that may not be suitable for users under 18.
	/// </summary>
	Age_Restricted = 3
}
