using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.ApplicationCommands;

public static class ApplicationCommandsUtilities
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
	/// <param name="targetTypeInfo">The type info.</param>
	internal static bool IsModuleCandidateType(this TypeInfo targetTypeInfo)
	{
		if (ApplicationCommandsExtension.DebugEnabled)
			ApplicationCommandsExtension.Logger.LogDebug("Checking type {name}", targetTypeInfo.FullName);
		// check if compiler-generated
		if (targetTypeInfo.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
			return false;

		// check if derives from the required base class
		var type = typeof(ApplicationCommandsModule);
		var typeInfo = type.GetTypeInfo();
		if (!typeInfo.IsAssignableFrom(targetTypeInfo))
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Not assignable from type");
			return false;
		}

		// check if anonymous
		if (targetTypeInfo.IsGenericType && targetTypeInfo.Name.Contains("AnonymousType") && (targetTypeInfo.Name.StartsWith("<>") || targetTypeInfo.Name.StartsWith("VB$")) && (targetTypeInfo.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Anonymous");
			return false;
		}

		// check if abstract, static, or not a class
		if (!targetTypeInfo.IsClass || targetTypeInfo.IsAbstract)
			return false;

		// check if delegate type
		var typeDelegate = typeof(Delegate).GetTypeInfo();
		if (typeDelegate.IsAssignableFrom(targetTypeInfo))
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Delegated");
			return false;
		}

		if (ApplicationCommandsExtension.DebugEnabled)
			ApplicationCommandsExtension.Logger.LogDebug("Checking qualifying methods");
		// qualifies if any method or type qualifies
		return targetTypeInfo.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _)) || targetTypeInfo.DeclaredNestedTypes.Any(xti => xti.IsModuleCandidateType());
	}

	/// <summary>
	/// Whether this is a command candidate.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="parameters">The parameters.</param>
	internal static bool IsCommandCandidate(this MethodInfo? method, out ParameterInfo[]? parameters)
	{
		parameters = null;
		// check if exists
		if (method == null)
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Not existent");
			return false;
		}
		if (ApplicationCommandsExtension.DebugEnabled)
			ApplicationCommandsExtension.Logger.LogDebug("Checking method {name}", method.Name);

		// check if static, non-public, abstract, a constructor, or a special name
		if (method.IsAbstract || method.IsConstructor || method.IsSpecialName) // method.IsStatic
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("abstract, constructor or special name");
			return false;
		}

		// check if appropriate return and arguments
		parameters = method.GetParameters();
		if (!parameters.Any() || (parameters.First().ParameterType != typeof(ContextMenuContext) && parameters.First().ParameterType != typeof(InteractionContext)) || method.ReturnType != typeof(Task))
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Missing first parameter with type ContextMenuContext or InteractionContext");
			return false;
		}

		if (ApplicationCommandsExtension.DebugEnabled)
			ApplicationCommandsExtension.Logger.LogDebug("Qualifies");
		// qualifies
		return true;
	}
}
