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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DisCatSharp.ApplicationCommands.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines this application command module's lifespan. Module lifespans are transient by default.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ApplicationCommandModuleLifespanAttribute : Attribute
{
	/// <summary>
	/// Gets the lifespan.
	/// </summary>
	public ApplicationCommandModuleLifespan Lifespan { get; }

	/// <summary>
	/// Defines this application command module's lifespan.
	/// </summary>
	/// <param name="lifespan">The lifespan of the module. Module lifespans are transient by default.</param>
	public ApplicationCommandModuleLifespanAttribute(ApplicationCommandModuleLifespan lifespan)
	{
		this.Lifespan = lifespan;
	}
}
