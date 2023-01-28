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

using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// Declares name of a property in serialized data. This is used for mapping serialized data to object properties and fields.
/// </summary>
[Obsolete("Use [DataMember] with set Name instead.")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SerializedNameAttribute : SerializationAttribute
{
	/// <summary>
	/// Gets the serialized name of the field or property.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Declares name of a property in serialized data. This is used for mapping serialized data to object properties and fields.
	/// </summary>
	/// <param name="name">Name of the field or property in serialized data.</param>
	public SerializedNameAttribute(string name)
	{
		this.Name = name;
	}
}
