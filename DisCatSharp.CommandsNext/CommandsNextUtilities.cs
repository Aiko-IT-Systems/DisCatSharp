// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.CommandsNext.Converters;
using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.CommandsNext
{
    /// <summary>
    /// Various CommandsNext-related utilities.
    /// </summary>
    public static class CommandsNextUtilities
    {
        /// <summary>
        /// Gets the user regex.
        /// </summary>
        private static Regex UserRegex { get; } = DiscordRegEx.User;

        /// <summary>
        /// Checks whether the message has a specified string prefix.
        /// </summary>
        /// <param name="Msg">Message to check.</param>
        /// <param name="Str">String to check for.</param>
        /// <param name="ComparisonType">Method of string comparison for the purposes of finding prefixes.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int GetStringPrefixLength(this DiscordMessage Msg, string Str, StringComparison ComparisonType = StringComparison.Ordinal)
        {
            var content = Msg.Content;
            return Str.Length >= content.Length ? -1 : !content.StartsWith(Str, ComparisonType) ? -1 : Str.Length;
        }

        /// <summary>
        /// Checks whether the message contains a specified mention prefix.
        /// </summary>
        /// <param name="Msg">Message to check.</param>
        /// <param name="User">User to check for.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int GetMentionPrefixLength(this DiscordMessage Msg, DiscordUser User)
        {
            var content = Msg.Content;
            if (!content.StartsWith("<@"))
                return -1;

            var cni = content.IndexOf('>');
            if (cni == -1 || content.Length <= cni + 2)
                return -1;

            var cnp = content[..(cni + 2)];
            var m = UserRegex.Match(cnp);
            if (!m.Success)
                return -1;

            var userId = ulong.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            return User.Id != userId ? -1 : m.Value.Length;
        }

        //internal static string ExtractNextArgument(string str, out string remainder)
        /// <summary>
        /// Extracts the next argument.
        /// </summary>
        /// <param name="Str">The string.</param>
        /// <param name="StartPos">The start position.</param>
        internal static string ExtractNextArgument(this string Str, ref int StartPos)
        {
            if (string.IsNullOrWhiteSpace(Str))
                return null;

            var inBacktick = false;
            var inTripleBacktick = false;
            var inQuote = false;
            var inEscape = false;
            var removeIndices = new List<int>(Str.Length - StartPos);

            var i = StartPos;
            for (; i < Str.Length; i++)
                if (!char.IsWhiteSpace(Str[i]))
                    break;
            StartPos = i;

            var endPosition = -1;
            var startPosition = StartPos;
            for (i = startPosition; i < Str.Length; i++)
            {
                if (char.IsWhiteSpace(Str[i]) && !inQuote && !inTripleBacktick && !inBacktick && !inEscape)
                    endPosition = i;

                if (Str[i] == '\\' && Str.Length > i + 1)
                {
                    if (!inEscape && !inBacktick && !inTripleBacktick)
                    {
                        inEscape = true;
                        if (Str.IndexOf("\\`", i) == i || Str.IndexOf("\\\"", i) == i || Str.IndexOf("\\\\", i) == i || (Str.Length >= i && char.IsWhiteSpace(Str[i + 1])))
                            removeIndices.Add(i - startPosition);
                        i++;
                    }
                    else if ((inBacktick || inTripleBacktick) && Str.IndexOf("\\`", i) == i)
                    {
                        inEscape = true;
                        removeIndices.Add(i - startPosition);
                        i++;
                    }
                }

                if (Str[i] == '`' && !inEscape)
                {
                    var tripleBacktick = Str.IndexOf("```", i) == i;
                    if (inTripleBacktick && tripleBacktick)
                    {
                        inTripleBacktick = false;
                        i += 2;
                    }
                    else if (!inBacktick && tripleBacktick)
                    {
                        inTripleBacktick = true;
                        i += 2;
                    }

                    if (inBacktick && !tripleBacktick)
                        inBacktick = false;
                    else if (!inTripleBacktick && tripleBacktick)
                        inBacktick = true;
                }

                if (Str[i] == '"' && !inEscape && !inBacktick && !inTripleBacktick)
                {
                    removeIndices.Add(i - startPosition);

                    inQuote = !inQuote;
                }

                if (inEscape)
                    inEscape = false;

                if (endPosition != -1)
                {
                    StartPos = endPosition;
                    return startPosition != endPosition ? Str[startPosition..endPosition].CleanupString(removeIndices) : null;
                }
            }

            StartPos = Str.Length;
            return StartPos != startPosition ? Str[startPosition..].CleanupString(removeIndices) : null;
        }

        /// <summary>
        /// Cleanups the string.
        /// </summary>
        /// <param name="S">The string.</param>
        /// <param name="Indices">The indices.</param>
        internal static string CleanupString(this string S, IList<int> Indices)
        {
            if (!Indices.Any())
                return S;

            var li = Indices.Last();
            var ll = 1;
            for (var x = Indices.Count - 2; x >= 0; x--)
            {
                if (li - Indices[x] == ll)
                {
                    ll++;
                    continue;
                }

                S = S.Remove(li - ll + 1, ll);
                li = Indices[x];
                ll = 1;
            }

            return S.Remove(li - ll + 1, ll);
        }

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Binds the arguments.
        /// </summary>
        /// <param name="Ctx">The command context.</param>
        /// <param name="IgnoreSurplus">If true, ignore further text in string.</param>
        internal static async Task<ArgumentBindingResult> BindArgumentsAsync(CommandContext Ctx, bool IgnoreSurplus)
#pragma warning restore IDE1006 // Naming Styles
        {
            var command = Ctx.Command;
            var overload = Ctx.Overload;

            var args = new object[overload.Arguments.Count + 2];
            args[1] = Ctx;
            var rawArgumentList = new List<string>(overload.Arguments.Count);

            var argString = Ctx.RawArgumentString;
            var foundAt = 0;
            var argValue = "";
            for (var i = 0; i < overload.Arguments.Count; i++)
            {
                var arg = overload.Arguments[i];
                if (arg.IsCatchAll)
                {
                    if (arg.IsArray)
                    {
                        while (true)
                        {
                            argValue = ExtractNextArgument(argString, ref foundAt);
                            if (argValue == null)
                                break;

                            rawArgumentList.Add(argValue);
                        }

                        break;
                    }
                    else
                    {
                        if (argString == null)
                            break;

                        argValue = argString[foundAt..].Trim();
                        argValue = argValue == "" ? null : argValue;
                        foundAt = argString.Length;

                        rawArgumentList.Add(argValue);
                        break;
                    }
                }
                else
                {
                    argValue = ExtractNextArgument(argString, ref foundAt);
                    rawArgumentList.Add(argValue);
                }

                if (argValue == null && !arg.IsOptional && !arg.IsCatchAll)
                    return new ArgumentBindingResult(new ArgumentException("Not enough arguments supplied to the command."));
                else if (argValue == null)
                    rawArgumentList.Add(null);
            }

            if (!IgnoreSurplus && foundAt < argString.Length)
                return new ArgumentBindingResult(new ArgumentException("Too many arguments were supplied to this command."));

            for (var i = 0; i < overload.Arguments.Count; i++)
            {
                var arg = overload.Arguments[i];
                if (arg.IsCatchAll && arg.IsArray)
                {
                    var array = Array.CreateInstance(arg.Type, rawArgumentList.Count - i);
                    var start = i;
                    while (i < rawArgumentList.Count)
                    {
                        try
                        {
                            array.SetValue(await Ctx.CommandsNext.ConvertArgumentAsync(rawArgumentList[i], Ctx, arg.Type).ConfigureAwait(false), i - start);
                        }
                        catch (Exception ex)
                        {
                            return new ArgumentBindingResult(ex);
                        }
                        i++;
                    }

                    args[start + 2] = array;
                    break;
                }
                else
                {
                    try
                    {
                        args[i + 2] = rawArgumentList[i] != null ? await Ctx.CommandsNext.ConvertArgumentAsync(rawArgumentList[i], Ctx, arg.Type).ConfigureAwait(false) : arg.DefaultValue;
                    }
                    catch (Exception ex)
                    {
                        return new ArgumentBindingResult(ex);
                    }
                }
            }

            return new ArgumentBindingResult(args, rawArgumentList);
        }

        /// <summary>
        /// Whether this module is a candidate type.
        /// </summary>
        /// <param name="Type">The type.</param>
        internal static bool IsModuleCandidateType(this Type Type)
            => Type.GetTypeInfo().IsModuleCandidateType();

        /// <summary>
        /// Whether this module is a candidate type.
        /// </summary>
        /// <param name="Ti">The type info.</param>
        internal static bool IsModuleCandidateType(this TypeInfo Ti)
        {
            // check if compiler-generated
            if (Ti.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
                return false;

            // check if derives from the required base class
            var tmodule = typeof(BaseCommandModule);
            var timodule = tmodule.GetTypeInfo();
            if (!timodule.IsAssignableFrom(Ti))
                return false;

            // check if anonymous
            if (Ti.IsGenericType && Ti.Name.Contains("AnonymousType") && (Ti.Name.StartsWith("<>") || Ti.Name.StartsWith("VB$")) && (Ti.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
                return false;

            // check if abstract, static, or not a class
            if (!Ti.IsClass || Ti.IsAbstract)
                return false;

            // check if delegate type
            var tdelegate = typeof(Delegate).GetTypeInfo();
            if (tdelegate.IsAssignableFrom(Ti))
                return false;

            // qualifies if any method or type qualifies
            return Ti.DeclaredMethods.Any(Xmi => Xmi.IsCommandCandidate(out _)) || Ti.DeclaredNestedTypes.Any(Xti => Xti.IsModuleCandidateType());
        }

        /// <summary>
        /// Whether this is a command candidate.
        /// </summary>
        /// <param name="Method">The method.</param>
        /// <param name="Parameters">The parameters.</param>
        internal static bool IsCommandCandidate(this MethodInfo Method, out ParameterInfo[] Parameters)
        {
            Parameters = null;
            // check if exists
            if (Method == null)
                return false;

            // check if static, non-public, abstract, a constructor, or a special name
            if (Method.IsStatic || Method.IsAbstract || Method.IsConstructor || Method.IsSpecialName)
                return false;

            // check if appropriate return and arguments
            Parameters = Method.GetParameters();
            if (!Parameters.Any() || Parameters.First().ParameterType != typeof(CommandContext) || Method.ReturnType != typeof(Task))
                return false;

            // qualifies
            return true;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="T">The type.</param>
        /// <param name="Services">The services provider.</param>
        internal static object CreateInstance(this Type T, IServiceProvider Services)
        {
            var ti = T.GetTypeInfo();
            var constructors = ti.DeclaredConstructors
                .Where(Xci => Xci.IsPublic)
                .ToArray();

            if (constructors.Length != 1)
                throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");

            var constructor = constructors[0];
            var constructorArgs = constructor.GetParameters();
            var args = new object[constructorArgs.Length];

            if (constructorArgs.Length != 0 && Services == null)
                throw new InvalidOperationException("Dependency collection needs to be specified for parameterized constructors.");

            // inject via constructor
            if (constructorArgs.Length != 0)
                for (var i = 0; i < args.Length; i++)
                    args[i] = Services.GetRequiredService(constructorArgs[i].ParameterType);

            var moduleInstance = Activator.CreateInstance(T, args);

            // inject into properties
            var props = T.GetRuntimeProperties().Where(Xp => Xp.CanWrite && Xp.SetMethod != null && !Xp.SetMethod.IsStatic && Xp.SetMethod.IsPublic);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = Services.GetService(prop.PropertyType);
                if (service == null)
                    continue;

                prop.SetValue(moduleInstance, service);
            }

            // inject into fields
            var fields = T.GetRuntimeFields().Where(Xf => !Xf.IsInitOnly && !Xf.IsStatic && Xf.IsPublic);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = Services.GetService(field.FieldType);
                if (service == null)
                    continue;

                field.SetValue(moduleInstance, service);
            }

            return moduleInstance;
        }
    }
}
