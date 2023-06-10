// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2023 AITSYS
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
using System.Reflection;
using System.Runtime.Loader;

using DisCatSharp.HybridCommands.Exceptions;

namespace DisCatSharp.HybridCommands.Utilities;
internal class CommandLoadContext : AssemblyLoadContext
{
	private readonly string _filePath;
	private readonly AssemblyDependencyResolver _resolver;

	public CommandLoadContext(string path)
	{
		this._filePath = path;
		this._resolver = new AssemblyDependencyResolver(path);
	}

	protected override Assembly Load(AssemblyName assemblyName)
	{
		var assemblyPath = this._resolver.ResolveAssemblyToPath(assemblyName);
		return assemblyPath != null ? this.LoadFromAssemblyPath(assemblyPath) : throw new AssemblyLoadException(this._filePath, "Failed to load assembly.");
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		var libraryPath = this._resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
		return libraryPath != null ? this.LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
	}
}
