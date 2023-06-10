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

using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.HybridCommands.Attributes;
using DisCatSharp.HybridCommands.Context;
using DisCatSharp.HybridCommands.Entities;
using DisCatSharp.HybridCommands.Utilities;

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

	internal static async Task<Assembly[]> CompileCommands(this Type type, ulong? guildId = null)
	{
		if (HybridCommandsExtension.Configuration is null)
			throw new InvalidOperationException("Configuration is null");

		HybridCommandsExtension.ExecutionDirectory ??= new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

		if (HybridCommandsExtension.DebugEnabled)
			HybridCommandsExtension.Logger?.LogDebug("ExecutionDirectory: {Dir}", HybridCommandsExtension.ExecutionDirectory);

		var CacheDirectory = $"{HybridCommandsExtension.ExecutionDirectory}/CachedHybridCommands";
		var CacheConfig = $"{CacheDirectory}/cache.json";

		if (HybridCommandsExtension.DebugEnabled)
			HybridCommandsExtension.Logger?.LogDebug("CacheDirectory: {Dir}", CacheDirectory);

		if (HybridCommandsExtension.DebugEnabled)
			HybridCommandsExtension.Logger?.LogDebug("CacheDirectory: {Dir}", CacheConfig);

		if (!Directory.Exists(CacheDirectory))
			Directory.CreateDirectory(CacheDirectory);

		var typeHash = JsonConvert.SerializeObject(type.GetTypeInfo(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }).ComputeSHA256Hash();

		if (HybridCommandsExtension.DebugEnabled)
			HybridCommandsExtension.Logger?.LogDebug("TypeHash: {Hash}", typeHash);

		var cacheConfig = File.Exists(CacheConfig) ? JsonConvert.DeserializeObject<CacheConfig>(File.ReadAllText(CacheConfig)) ?? new() : new();

		var fileInfos = new DirectoryInfo(CacheDirectory).GetFiles();

		Dictionary<Assembly, string> loadedAssemblies = new();

		if (!HybridCommandsExtension.Configuration.DisableCompilationCache && cacheConfig.LastKnownTypeHashes.Contains(typeHash))
			foreach (var file in fileInfos)
			{
				if ($"{file.Name.Replace(".dll", "")}" == $"{typeHash}-app")
				{
					var loadContext = new CommandLoadContext(file.FullName);
					var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(file.FullName)));

					if (assembly.GetTypes().Any(x => typeof(ApplicationCommandsModule).IsAssignableTo(x)))
						loadedAssemblies.Add(assembly, $"{typeHash}-app");
				}

				if ($"{file.Name.Replace(".dll", "")}" == $"{typeHash}-prefix")
				{
					var loadContext = new CommandLoadContext(file.FullName);
					var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(file.FullName)));

					if (assembly.GetTypes().Any(x => typeof(ApplicationCommandsModule).IsAssignableTo(x)))
						loadedAssemblies.Add(assembly, $"{typeHash}-prefix");
				}
			}

		if (loadedAssemblies.Values.Any(x => x == $"{typeHash}-prefix") && loadedAssemblies.Values.Any(x => x == $"{typeHash}-prefix"))
			return loadedAssemblies.Keys.ToArray();

		string[] GenerateAppCommandMethods()
		{

		}

		var AppClass =
		  $$"""
			// This class has been auto-generated by DisCatSharp.HybridCommands.

			namespace DisCatSharp.RunTimeCompiled;

			public sealed class a{{typeHash}}_AppCommands : {{typeof(ApplicationCommandsModule).FullName}}
			{
			{{string.Join("\n\n", GenerateAppCommandMethods())}}
			}
			""";

		string[] GeneratePrefixCommandMethods()
		{
			List<string> generatedMethods = new();

			foreach (var method in type.GetMethods())
			{
				if (!method.IsCommandCandidate(out var parameters))
					continue;

				var cmdInfo = method.GetCustomAttribute<HybridCommandAttribute>();
				var restrictedTypes = method.GetCustomAttributes().Any(x => x is RestrictedExecutionTypesAttribute) ? method.GetCustomAttributes<RestrictedExecutionTypesAttribute>() : null;

				if (!restrictedTypes?.First().Types.Any(y => y == Enums.HybridExecutionType.PrefixCommand) ?? false)
				{
					if (HybridCommandsExtension.DebugEnabled)
						HybridCommandsExtension.Logger?.LogDebug("Command is limited to execution with: {types}", string.Join("\n", restrictedTypes?.First().Types.Select(x => Enum.GetName(x)).ToArray() ?? Array.Empty<string>()));
					continue;
				}

				if (parameters is null ||
					parameters.Length == 0 ||
					parameters[0].ParameterType != typeof(HybridCommandContext) ||
					(parameters.Length > 1 && parameters.Where(x => x.ParameterType != typeof(HybridCommandContext)).All(x => x.GetCustomAttributes().Any(y => y is OptionAttribute))))
				{
					if (HybridCommandsExtension.DebugEnabled)
						HybridCommandsExtension.Logger?.LogDebug("Method has no HybridCommandContext or is missing OptionAttributes");

					continue;
				}

				if (cmdInfo is null)
				{
					if (HybridCommandsExtension.DebugEnabled)
						HybridCommandsExtension.Logger?.LogDebug("Method has no HybridCommandAttribute");

					continue;
				}

				var MethodName = $"a{Guid.NewGuid().ToString().ToLower().Replace("-", "")}";

				var filteredParams = parameters.Where(x => x.ParameterType != typeof(HybridCommandContext));
				string MakeParameters()
				{
					List<string> generatedParams = new();

					foreach (var param in filteredParams)
					{
						var option = param.GetCustomAttribute<OptionAttribute>() ?? throw new InvalidOperationException("Method Parameter is missing OptionAttribute.");
						var useRemainingString = param.GetCustomAttributes().Any(x => x is RemainingTextAttribute);
						generatedParams.Add($"[{typeof(DescriptionAttribute).FullName}(\"{option.Description.SanitzeForString()}\")]" +
							$"{(useRemainingString ? $"[{typeof(RemainingTextAttribute).FullName}]" : "")} {param.ParameterType.FullName} {param.Name}");
					}

					return string.Join(", ", generatedParams);
				}
				
				generatedMethods.Add($$"""
						public static {{typeof(MethodInfo)}} {{MethodName}}_CommandMethod { get; set; }

						public static {{MethodName}}_Populate()
							=> {{MethodName}}_CommandMethod = {{typeof(Assembly).FullName}}.GetCallingAssembly().GetType($"{{type.FullName}}").GetMethods().First(x => x.Name == "{{method.Name}}");

						[{{typeof(CommandAttribute).FullName}}("{{cmdInfo.Name.SanitzeForString()}}"),
						{{typeof(DescriptionAttribute).FullName}}("{{cmdInfo.Description.SanitzeForString()}}")]
						public {{typeof(Task).FullName}} {{MethodName}}_Execute({{typeof(CommandContext).FullName}} ctx{{MakeParameters()}})
						{
							{{(guildId is not null ? $"if (ctx.Guild?.Id ?? 0 != {guildId}) return;" : "")}}
							
							{{MethodName}}_CommandMethod.Invoke({{typeof(Activator).FullName}}.CreateInstance({{MethodName}}_CommandMethod.DeclaringType), new object[]
							{
								new {{typeof(HybridCommandContext).FullName}}(ctx){{(filteredParams.Any() ? $", {string.Join(", ", filteredParams.Select(x => x.Name))}" : "")}}
							});
							return {{typeof(Task).FullName}}.Completed;
						}
					""");
			}

			return generatedMethods.ToArray();
		}

		var PrefixClass =
		  $$"""
			// This class has been auto-generated by DisCatSharp.HybridCommands.

			namespace DisCatSharp.RunTimeCompiled;

			public sealed class a{{typeHash}}_PrefixCommands : {{typeof(BaseCommandModule).FullName}}
			{
			{{string.Join("\n\n", GeneratePrefixCommandMethods())}}
			}
			""";

		if (HybridCommandsExtension.Configuration.OutputGeneratedClasses)
		{
			File.WriteAllText($"{CacheDirectory}/{typeHash}-app.cs", AppClass);
			File.WriteAllText($"{CacheDirectory}/{typeHash}-prefix.cs", PrefixClass);
		}

		return Array.Empty<Assembly>();
	}

	/// <summary>
	/// Compute the SHA256-Hash for the given string
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	private static string ComputeSHA256Hash(this string str)
		=> BitConverter.ToString(SHA256.HashData(Encoding.ASCII.GetBytes(str))).Replace("-", "").ToLowerInvariant();

	private static string SanitzeForString(this string str)
		=> str.Replace("\"", "\\\"");
}
