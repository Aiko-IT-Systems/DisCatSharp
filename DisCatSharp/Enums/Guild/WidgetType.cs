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

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the formats for a guild widget.
/// </summary>
public enum WidgetType : int
{
	/// <summary>
	/// The widget is represented in shield format.
	/// <para>This is the default widget type.</para>
	/// </summary>
	Shield = 0,

	/// <summary>
	/// The widget is represented as the first banner type.
	/// </summary>
	Banner1 = 1,

	/// <summary>
	/// The widget is represented as the second banner type.
	/// </summary>
	Banner2 = 2,

	/// <summary>
	/// The widget is represented as the third banner type.
	/// </summary>
	Banner3 = 3,

	/// <summary>
	/// The widget is represented in the fourth banner type.
	/// </summary>
	Banner4 = 4
}
