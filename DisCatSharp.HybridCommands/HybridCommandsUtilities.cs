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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.HybridCommands.Attributes;
using DisCatSharp.HybridCommands.Context;
using DisCatSharp.HybridCommands.Entities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DisCatSharp.HybridCommands;
internal static class HybridCommandsUtilities
{
	/// <summary>
	/// Whether this module is a candidate type.
	/// </summary>
	/// <param name="type">The type.</param>
	internal static bool IsModuleCandidateType(this Type type)
		=> type.GetTypeInfo().IsModuleCandidateType();

	/// <summary>
	/// Whether this module is a candidate type.
	/// </summary>
	/// <param name="typeInfo">The type info.</param>
	internal static bool IsModuleCandidateType(this TypeInfo typeInfo)
	{
		if (typeInfo.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
			return false;

		if (!typeof(HybridCommandsModule).GetTypeInfo().IsAssignableFrom(typeInfo))
		{
			if (HybridCommandsExtension.DebugEnabled)
				HybridCommandsExtension.Logger?.LogDebug("Not assignable from type");
			return false;
		}

		if (typeInfo.IsGenericType && typeInfo.Name.Contains("AnonymousType") && (typeInfo.Name.StartsWith("<>") || typeInfo.Name.StartsWith("VB$")) && (typeInfo.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
		{
			if (HybridCommandsExtension.DebugEnabled)
				HybridCommandsExtension.Logger?.LogDebug("Anonymous");
			return false;
		}

		if (!typeInfo.IsClass || typeInfo.IsAbstract)
			return false;

		var tdelegate = typeof(Delegate).GetTypeInfo();
		if (tdelegate.IsAssignableFrom(typeInfo))
		{
			if (HybridCommandsExtension.DebugEnabled)
				HybridCommandsExtension.Logger?.LogDebug("Delegated");
			return false;
		}

		if (HybridCommandsExtension.DebugEnabled)
			HybridCommandsExtension.Logger?.LogDebug("Checking qualifying methods");

		return typeInfo.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _)) || typeInfo.DeclaredNestedTypes.Any(xti => xti.IsModuleCandidateType());
	}

	/// <summary>
	/// Whether this is a command candidate.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="parameters">The parameters.</param>
	internal static bool IsCommandCandidate(this MethodInfo method, out ParameterInfo[]? parameters)
	{
		parameters = null;

		if (method == null)
		{
			if (HybridCommandsExtension.DebugEnabled)
				HybridCommandsExtension.Logger?.LogDebug("Not existent");
			return false;
		}
		if (HybridCommandsExtension.DebugEnabled)
			HybridCommandsExtension.Logger?.LogDebug("Checking method {name}", method.Name);

		if (method.IsAbstract || method.IsConstructor || method.IsSpecialName)
		{
			if (HybridCommandsExtension.DebugEnabled)
				HybridCommandsExtension.Logger?.LogDebug("Abstract, Constructor or Special name");
			return false;
		}

		if (!method.GetCustomAttributes().Any(x => x.GetType() == typeof(HybridCommandAttribute)))
		{
			if (HybridCommandsExtension.DebugEnabled)
				HybridCommandsExtension.Logger?.LogDebug("Does not have HybridCommandAttribute");
			return false;
		}

		parameters = method.GetParameters();
		if (!parameters.Any() || parameters.First().ParameterType != typeof(HybridCommandContext) || method.ReturnType != typeof(Task))
		{
			if (HybridCommandsExtension.DebugEnabled)
				HybridCommandsExtension.Logger?.LogDebug("Missing first parameter with type HybridCommandContext");
			return false;
		}

		if (HybridCommandsExtension.DebugEnabled)
			HybridCommandsExtension.Logger?.LogDebug("Qualifies");

		return true;
	}

	internal static async Task<Assembly[]> CompileCommands(this Type type)
	{
		HybridCommandsExtension.ExecutionDirectory ??= new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

		var CacheDirectory = $"{HybridCommandsExtension.ExecutionDirectory}/CachedHybridCommands";
		var CacheConfig = $"{CacheDirectory}/cache.json";

		if (!Directory.Exists(CacheDirectory))
			Directory.CreateDirectory(CacheDirectory);

		var typeHash = JsonConvert.SerializeObject(type.GetTypeInfo(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }).ComputeSHA256Hash();

		var cacheConfig = File.Exists(CacheConfig) ? JsonConvert.DeserializeObject<CacheConfig>(File.ReadAllText(CacheConfig)) ?? new() : new();

		if (cacheConfig.LastKnownTypeHashes.Contains(typeHash) && new DirectoryInfo(CacheDirectory).GetFiles().Any(x => $"{x.Name.Replace(".dll", "")}-app" == typeHash))
		{

		}

		return Array.Empty<Assembly>();
	}

	/// <summary>
	/// Compute the SHA256-Hash for the given string
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	internal static string ComputeSHA256Hash(this string str)
		=> BitConverter.ToString(SHA256.HashData(Encoding.ASCII.GetBytes(str))).Replace("-", "").ToLowerInvariant();
}
