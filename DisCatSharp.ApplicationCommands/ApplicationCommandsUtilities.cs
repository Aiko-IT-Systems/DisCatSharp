using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.ApplicationCommands;

/// <summary>
/// Provides a set of utility methods for application commands.
/// </summary>
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
	/// <param name="ti">The type info.</param>
	internal static bool IsModuleCandidateType(this TypeInfo ti)
	{
		if (ApplicationCommandsExtension.DebugEnabled)
			ApplicationCommandsExtension.Logger.LogDebug("Checking type {name}", ti.FullName);
		// check if compiler-generated
		if (ti.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
			return false;

		// check if derives from the required base class
		var tmodule = typeof(ApplicationCommandsModule);
		var timodule = tmodule.GetTypeInfo();
		if (!timodule.IsAssignableFrom(ti))
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Not assignable from type");
			return false;
		}

		// check if anonymous
		if (ti.IsGenericType && ti.Name.Contains("AnonymousType") && (ti.Name.StartsWith("<>", StringComparison.Ordinal) || ti.Name.StartsWith("VB$", StringComparison.Ordinal)) && (ti.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Anonymous");
			return false;
		}

		// check if abstract, or not a class
		if (!ti.IsClass || ti.IsAbstract)
			return false;

		// check if delegate type
		var tdelegate = typeof(Delegate).GetTypeInfo();
		if (tdelegate.IsAssignableFrom(ti))
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("Delegated");
			return false;
		}

		if (ApplicationCommandsExtension.DebugEnabled)
			ApplicationCommandsExtension.Logger.LogDebug("Checking qualifying methods");
		// qualifies if any method or type qualifies
		return ti.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _)) || ti.DeclaredNestedTypes.Any(xti => xti.IsModuleCandidateType());
	}

	/// <summary>
	/// Whether this is a command candidate.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="parameters">The parameters.</param>
	internal static bool IsCommandCandidate(this MethodInfo method, out ParameterInfo[] parameters)
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

		// check if non-public, abstract, a constructor, or a special name
		if (method.IsAbstract || method.IsConstructor || method.IsSpecialName)
		{
			if (ApplicationCommandsExtension.DebugEnabled)
				ApplicationCommandsExtension.Logger.LogDebug("abstract, constructor or special name");
			return false;
		}

		// check if appropriate return and arguments
		parameters = method.GetParameters();
		if (parameters.Length == 0 || (parameters.First().ParameterType != typeof(ContextMenuContext) && parameters.First().ParameterType != typeof(InteractionContext)) || method.ReturnType != typeof(Task))
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
