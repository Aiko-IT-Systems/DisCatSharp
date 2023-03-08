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

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DisCatSharp.Enums;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp;

/// <summary>
/// A Discord API wrapper.
/// </summary>
public sealed partial class DiscordClient
{
	private readonly Dictionary<(object?, Type, bool), List<(EventInfo, Delegate)[]>> _registrationToDelegate = new();
	private readonly Dictionary<Type, List<object>> _typeToAnonymousHandlers = new();

	/// <summary>
	/// Registers all methods annotated with <see cref="EventAttribute"/> from the given object.
	/// </summary>
	/// <param name="handler">The event handler object.</param>
	/// <param name="registerStatic">Whether to consider static methods.</param>
	public void RegisterEventHandler(object handler, bool registerStatic = false)
		=> this.RegisterEventHandlerImpl(handler, handler.GetType(), registerStatic);

	/// <summary>
	/// Registers all static methods annotated with <see cref="EventAttribute"/> from the given type.
	/// </summary>
	/// <param name="t">The static event handler type.</param>
	public void RegisterStaticEventHandler(Type t)
		=> this.RegisterEventHandlerImpl(null, t);

	/// <summary>
	/// <see cref="RegisterStaticEventHandler(Type)"/>.
	/// </summary>
	/// <typeparam name="T">Type to register.</typeparam>
	public void RegisterStaticEventHandler<T>()
		=> this.RegisterStaticEventHandler(typeof(T));

	/// <summary>
	/// <para>If abstract, registers all static methods of the type.</para>
	/// <para>If non-abstract, tries to instantiate it, optionally using the provided <see cref="DiscordConfiguration.ServiceProvider"/>
	/// and registers all instance and static methods.</para>
	/// </summary>
	/// <param name="type">Type to register.</param>
	public void RegisterEventHandler(Type type)
	{
		if (type.IsAbstract)
			this.RegisterStaticEventHandler(type);
		else
		{
			var anon = ActivatorUtilities.CreateInstance(this.Configuration.ServiceProvider, type);

			this._typeToAnonymousHandlers[type] = this._typeToAnonymousHandlers.TryGetValue(type, out var anonObjs) ? anonObjs : (anonObjs = new());

			anonObjs.Add(anon);

			this.RegisterEventHandlerImpl(anon, type);
		}
	}

	/// <summary>
	/// <see cref="RegisterEventHandler(Type)"/>.
	/// </summary>
	/// <typeparam name="T">Type to register.</typeparam>
	public void RegisterEventHandler<T>()
		=> this.RegisterEventHandler(typeof(T));

	/// <summary>
	/// Registers all types associated with the provided assembly that have the <see cref="EventHandler"/> attribute.
	/// </summary>
	/// <param name="assembly">The assembly from which to get the types.</param>
	public void RegisterEventHandlers(Assembly assembly)
	{
		foreach (var t in GetEventHandlersFromAssembly(assembly))
			this.RegisterEventHandler(t);
	}

	/// <summary>
	/// Perfectly mirrors <see cref="RegisterEventHandler(object, bool)"/>.
	/// </summary>
	/// <param name="handler">The event handler object.</param>
	/// <param name="wasRegisteredWithStatic">Whether it considered static methods.</param>
	public void UnregisterEventHandler(object handler, bool wasRegisteredWithStatic = false)
		=> this.UnregisterEventHandlerImpl(handler, handler.GetType(), wasRegisteredWithStatic);

	/// <summary>
	/// Perfectly mirrors <see cref="RegisterStaticEventHandler(Type)"/>.
	/// </summary>
	/// <param name="t">Type to unregister.</param>
	public void UnregisterStaticEventHandler(Type t)
		=> this.UnregisterEventHandlerImpl(null, t);

	/// <summary>
	/// Perfectly mirrors <see cref="RegisterStaticEventHandler{T}()"/>.
	/// </summary>
	/// <typeparam name="T">Type to unregister.</typeparam>
	public void UnregisterStaticEventHandler<T>()
		=> this.UnregisterEventHandler(typeof(T));

	/// <summary>
	/// Perfectly mirrors <see cref="RegisterEventHandler(Type)"/>.
	/// </summary>
	/// <param name="t">Type to unregister.</param>
	public void UnregisterEventHandler(Type t)
	{
		if (t.IsAbstract)
			this.UnregisterStaticEventHandler(t);
		else
		{
			if (!this._typeToAnonymousHandlers.TryGetValue(t, out var anonObjs) || anonObjs.Count == 0)
				return;

			var anon = anonObjs[0];
			anonObjs.RemoveAt(0);

			if (anonObjs.Count == 0)
				this._typeToAnonymousHandlers.Remove(t);

			this.UnregisterEventHandlerImpl(anon, t);
		}
	}

	/// <summary>
	/// Perfectly mirrors <see cref="RegisterEventHandler{T}()"/>.
	/// </summary>
	/// <typeparam name="T">The type to unregister</typeparam>
	public void UnregisterEventHandler<T>() => this.UnregisterEventHandler(typeof(T));

	/// <summary>
	/// Perfectly mirrors <see cref="RegisterEventHandlers(Assembly)"/>.
	/// </summary>
	/// <param name="assembly">The assembly to unregister.</param>
	public void UnregisterEventHandlers(Assembly assembly)
	{
		foreach (var t in GetEventHandlersFromAssembly(assembly))
			this.UnregisterEventHandler(t);
	}

	/// <summary>
	/// Gets the event handlers from the assembly.
	/// </summary>
	/// <param name="assembly">The assembly to get the event handlers from.</param>
	private static IEnumerable<Type> GetEventHandlersFromAssembly(Assembly assembly)
		=> assembly.GetTypes().Where(t => t.GetCustomAttribute<EventHandlerAttribute>() is not null);

	/// <summary>
	/// Unregisters event handler implementations.
	/// </summary>
	/// <param name="handler">The event handler object.</param>
	/// <param name="type">The type.</param>
	/// <param name="wasRegisteredWithStatic">Whether it considereded static methods.</param>

	private void UnregisterEventHandlerImpl(object? handler, Type type, bool wasRegisteredWithStatic = true)
	{
		if (!this._registrationToDelegate.TryGetValue((handler, type, wasRegisteredWithStatic), out var delegateLists) || delegateLists.Count == 0)
			return;

		foreach (var (evnt, dlgt) in delegateLists[0])
			evnt.RemoveEventHandler(this, dlgt);

		delegateLists.RemoveAt(0);
		if (delegateLists.Count == 0)
			this._registrationToDelegate.Remove((handler, type, wasRegisteredWithStatic));
	}

	/// <summary>
	/// Rregisters event handler implementations.
	/// </summary>
	/// <param name="handler">The event handler object.</param>
	/// <param name="type">The type.</param>
	/// <param name="registerStatic">Whether to consider static methods.</param>
	private void RegisterEventHandlerImpl(object? handler, Type type, bool registerStatic = true)
	{
		var delegates = (
			from method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			let attribute = method.GetCustomAttribute<EventAttribute>()
			where attribute is not null && ((registerStatic && method.IsStatic) || handler is not null)
			let eventName = attribute.EventName ?? method.Name
			let eventInfo = this.GetType().GetEvent(eventName)
				?? throw new ArgumentException($"Tried to register handler to non-existent event \"{eventName}\"")
			let eventHandlerType = eventInfo.EventHandlerType
			let dlgt = (method.IsStatic
				? Delegate.CreateDelegate(eventHandlerType, method, false)
				: Delegate.CreateDelegate(eventHandlerType, handler, method, false))
				?? throw new ArgumentException($"Method \"{method}\" does not adhere to event specification \"{eventHandlerType}\"")
			select (eventInfo, dlgt)
			).ToArray();

		this._registrationToDelegate[(handler, type, registerStatic)] = this._registrationToDelegate.TryGetValue((handler, type, registerStatic), out var delList) ? delList : (delList = new());

		delList.Add(delegates);

		foreach (var (evnt, dlgt) in delegates)
			evnt.AddEventHandler(this, dlgt);
	}
}
