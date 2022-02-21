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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp
{
	/// <summary>
	/// A Discord API wrapper.
	/// </summary>
	public sealed partial class DiscordClient
	{
		private readonly Dictionary<(object?, Type, bool), List<List<(EventInfo, Delegate)>>> _registrationToDelegate = new();

		/// <summary>
		/// Registers all methods annotated with <see cref="Event"/> from the given object.
		/// </summary>
		/// <param name="handler">The event handler object.</param>
		/// <param name="registerStatic">Whether to consider static methods.</param>
		public void RegisterEventHandler(object handler, bool registerStatic = false)
			=> this.RegisterEventHandlerImpl(handler, handler.GetType(), registerStatic);

		/// <summary>
		/// Registers all static methods annoated with <see cref="Event"/> from the given type.
		/// </summary>
		/// <param name="t">The static event handler type.</param>
		public void RegisterStaticEventHandler(Type t)
			=> this.RegisterEventHandlerImpl(null, t);

		/// <see cref="RegisterStaticEventHandler(Type)"/>
		public void RegisterStaticEventHandler<T>() => this.RegisterStaticEventHandler(typeof(T));

		/// <summary>
		/// <para>If abstract, registers all static methods of the type.</para>
		/// <para>If non-abstract, tries to instantiate it, optionally using the provided <see cref="DiscordConfiguration.ServiceProvider"/>
		/// and registers all instance and static methods.</para>
		/// </summary>
		/// <param name="type"></param>
		public void RegisterEventHandler(Type type)
		{
			if (type.IsAbstract)
			{
				this.RegisterStaticEventHandler(type);
			}
			else
			{
				this.RegisterEventHandlerImpl(this.Configuration.ServiceProvider is not null
					? ActivatorUtilities.CreateInstance(this.Configuration.ServiceProvider, type)
					: Activator.CreateInstance(type), type);
			}
		}

		/// <see cref="RegisterEventHandler(Type)"/>
		public void RegisterEventHandler<T>() => this.RegisterEventHandler(typeof(T));

		/// <summary>
		/// Registers all types associated with the provided assembly that have the <see cref="EventHandler"/> attribute.
		/// </summary>
		/// <param name="assembly">The assembly from which to get the types.</param>
		public void RegisterEventHandlers(Assembly assembly)
		{
			foreach (Type t in this.getEventHandlersFromAssembly(assembly))
			{
				this.RegisterEventHandler(t);
			}
		}

		public void UnregisterEventHandler(object handler, bool wasRegisteredWithStatic = false)
			=> this.UnregisterEventHandlerImpl(handler, handler.GetType(), wasRegisteredWithStatic);

		public void UnregisterStaticEventHandler(Type t) => this.UnregisterEventHandlerImpl(null, t);

		public void UnregisterStaticEventHandler<T>() => this.UnregisterEventHandler(typeof(T));

		public void UnregisterEventHandler(Type t)
			=> this.UnregisterEventHandlerImpl(null, t);

		public void UnregisterEventHandler<T>() => this.UnregisterEventHandler(typeof(T));

		public void UnregisterEventHandlers(Assembly assembly)
		{
			foreach (Type t in this.getEventHandlersFromAssembly(assembly))
			{
				this.UnregisterEventHandler(t);
			}
		}

		private IEnumerable<Type> getEventHandlersFromAssembly(Assembly assembly)
			=> assembly.GetTypes()
				.Where(t => t.GetCustomAttribute<EventHandler>() is not null);

		private void UnregisterEventHandlerImpl(object? handler, Type type, bool registerStatic = true)
		{
			if (!this._registrationToDelegate.TryGetValue((handler, type, registerStatic), out var delegateLists)
				|| delegateLists.Count == 0)
			{
				throw new InvalidOperationException("Event handler was not registered!");
			}

			foreach (var (evtn, dlgt) in delegateLists[0])
			{
				evtn.RemoveEventHandler(this, dlgt);
			}
			delegateLists.RemoveAt(0);
			if (delegateLists.Count == 0)
			{
				this._registrationToDelegate.Remove((handler, type, registerStatic));
			}
		}

		private void RegisterEventHandlerImpl(object? handler, Type type, bool registerStatic = true)
		{
			var delegates = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
							   .Select(method => (method, attribute: method.GetCustomAttribute<Event>()))
							   .Where(m => m.attribute is not null && ((registerStatic && m.method.IsStatic) || handler is not null))
							   .Select(m => {
								   var eventName = m.attribute!.EventName ?? m.method.Name;
								   return (m.method, evtn: this.GetType().GetEvent(eventName)
										?? throw new InvalidOperationException($"Tried to register handler to non-existent event "
																			 + $"\"{eventName}\"")); })
							   .Select(m => (m.evtn, dlgt: m.method.IsStatic
									? Delegate.CreateDelegate(m.evtn.EventHandlerType, m.method)
									: Delegate.CreateDelegate(m.evtn.EventHandlerType, handler, m.method))).ToList();

			this._registrationToDelegate[(handler, type, registerStatic)]
				= this._registrationToDelegate.TryGetValue((handler, type, registerStatic), out var delList) ? delList : (delList = new());

			delList.Add(delegates);

			foreach (var (evtn, dlgt) in delegates)
			{
				evtn.AddEventHandler(this, dlgt);
			}
		}
	}
}
