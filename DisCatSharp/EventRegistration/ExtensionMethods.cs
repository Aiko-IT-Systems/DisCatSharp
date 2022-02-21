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

#nullable enable

using System;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.EventRegistration
{
	static class ExtensionMethods
	{
		/// <summary>
		/// Registers all methods annotated with <see cref="Event"/> from the given object.
		/// </summary>
		/// <param name="client">Client to register the event handler for.</param>
		/// <param name="handler">The event handler object.</param>
		/// <param name="registerStatic">Whether to consider static methods.</param>
		public static void RegisterEventHandler(this DiscordClient client, object handler, bool registerStatic = false)
			=> RegisterEventHandlerImpl(client, handler, handler.GetType(), registerStatic);

		/// <summary>
		/// Registers all static methods annoated with <see cref="Event"/> from the given type.
		/// </summary>
		/// <param name="client">Client to register the event handler for.</param>
		/// <param name="t">The static event handler type.</param>
		public static void RegisterStaticEventHandler(this DiscordClient client, Type t)
			=> RegisterEventHandlerImpl(client, null, t);

		/// <see cref="RegisterStaticEventHandler(DiscordClient, Type)"/>
		public static void RegisterStaticEventHandler<T>(this DiscordClient client)
			=> client.RegisterStaticEventHandler(typeof(T));

		/// <summary>
		/// <para>If abstract, registers all static methods of the type.</para>
		/// <para>If non-abstract, tries to instantiate it, optionally using the provided <see cref="DiscordConfiguration.ServiceProvider"/>
		/// and registers all instance and static methods.</para>
		/// </summary>
		/// <param name="client">Client to register the event handler for.</param>
		/// <param name="type"></param>
		public static void RegisterEventHandler(this DiscordClient client, Type type)
		{
			if (type.IsAbstract)
			{
				client.RegisterStaticEventHandler(type);
			}
			else
			{
				RegisterEventHandlerImpl(client, client.Configuration.ServiceProvider is not null
					? ActivatorUtilities.CreateInstance(client.Configuration.ServiceProvider, type)
					: Activator.CreateInstance(type), type);
			}
		}

		/// <see cref="RegisterEventHandler(DiscordClient, Type)"/>
		public static void RegisterEventHandler<T>(this DiscordClient client) => client.RegisterEventHandler(typeof(T));

		/// <summary>
		/// Registers all types associated with the provided assembly that have the <see cref="EventHandler"/> attribute.
		/// </summary>
		/// <param name="client">Client to register the event handler for.</param>
		/// <param name="assembly">The assembly from which to get the types.</param>
		public static void RegisterEventHandlers(this DiscordClient client, Assembly assembly)
		{
			foreach (Type t in assembly.GetTypes()
				.Where(t => t.GetCustomAttribute<EventHandler>() is not null))
			{
				client.RegisterEventHandler(t);
			}
		}

		private static void RegisterEventHandlerImpl(DiscordClient client, object? handler, Type type, bool registerStatic = true)
		{
			var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
							   .Select(method => (method, attribute: method.GetCustomAttribute<Event>()))
							   .Where(m => m.attribute is not null && ((registerStatic && m.method.IsStatic) || handler is not null))
							   .Select(m => {
								   var eventName = m.attribute!.EventName ?? m.method.Name;
								   return (m.method, client.GetType().GetEvent(eventName)
										?? throw new InvalidOperationException($"Tried to register handler to non-existent event "
																			 + $"\"{eventName}\""));
							   });

			foreach (var (method, evtn) in methods)
			{
				evtn.AddEventHandler(client, method.IsStatic
					? Delegate.CreateDelegate(evtn.EventHandlerType, method)
					: Delegate.CreateDelegate(evtn.EventHandlerType, handler, method));
			}
		}
	}
}
