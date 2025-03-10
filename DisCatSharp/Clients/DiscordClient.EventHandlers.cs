using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DisCatSharp.Enums;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp;

/// <summary>
///     A Discord API wrapper.
/// </summary>
public sealed partial class DiscordClient
{
	/// <summary>
	///     Stores the registered event handlers.
	/// </summary>
	private readonly Dictionary<Type, object> _registeredEventhandlers = [];

	/// <summary>
	///     Maps a tuple of handler, type, and a boolean indicating whether static methods were registered to a list of event
	///     info and delegate arrays.
	/// </summary>
	private readonly Dictionary<(object?, Type, bool), List<(EventInfo, Delegate)[]>> _registrationToDelegate = [];

	/// <summary>
	///     Maps a type to a list of anonymous handler objects.
	/// </summary>
	private readonly Dictionary<Type, List<object>> _typeToAnonymousHandlers = [];

	/// <summary>
	///     Gets all the registered event handlers.
	/// </summary>
	public IReadOnlyDictionary<Type, object> RegisteredEventhandlers => this._registeredEventhandlers.AsReadOnly();

	/// <summary>
	///     Registers all methods annotated with <see cref="EventAttribute" /> from the given object.
	/// </summary>
	/// <param name="handler">The event handler object.</param>
	/// <param name="registerStatic">Whether to consider static methods.</param>
	public void RegisterEventHandler(object handler, bool registerStatic = false)
		=> this.RegisterEventHandlerImpl(handler, handler.GetType(), registerStatic);

	/// <summary>
	///     Registers all static methods annotated with <see cref="EventAttribute" /> from the given type.
	/// </summary>
	/// <param name="t">The static event handler type.</param>
	public void RegisterStaticEventHandler(Type t)
		=> this.RegisterEventHandlerImpl(null, t);

	/// <summary>
	///     <see cref="RegisterStaticEventHandler(Type)" />.
	/// </summary>
	/// <typeparam name="T">Type to register.</typeparam>
	public void RegisterStaticEventHandler<T>()
		=> this.RegisterStaticEventHandler(typeof(T));

	/// <summary>
	///     <para>If abstract, registers all static methods of the type.</para>
	///     <para>
	///         If non-abstract, tries to instantiate it, optionally using the provided
	///         <see cref="DiscordConfiguration.ServiceProvider" />
	///         and registers all instance and static methods.
	///     </para>
	/// </summary>
	/// <param name="type">Type to register.</param>
	public void RegisterEventHandler(Type type)
	{
		if (type.IsAbstract)
			this.RegisterStaticEventHandler(type);
		else
		{
			var anon = ActivatorUtilities.CreateInstance(this.Configuration.ServiceProvider, type);

			this._typeToAnonymousHandlers[type] = this._typeToAnonymousHandlers.TryGetValue(type, out var anonObjs) ? anonObjs : anonObjs = [];

			anonObjs.Add(anon);

			this.RegisterEventHandlerImpl(anon, type);
		}
	}

	/// <summary>
	///     <see cref="RegisterEventHandler(Type)" />.
	/// </summary>
	/// <typeparam name="T">Type to register.</typeparam>
	public void RegisterEventHandler<T>()
		=> this.RegisterEventHandler(typeof(T));

	/// <summary>
	///     Registers all types associated with the provided assembly that have the <see cref="EventHandler" /> attribute.
	/// </summary>
	/// <param name="assembly">The assembly from which to get the types.</param>
	public void RegisterEventHandlers(Assembly assembly)
	{
		foreach (var t in GetEventHandlersFromAssembly(assembly))
			this.RegisterEventHandler(t);
	}

	/// <summary>
	///     Perfectly mirrors <see cref="RegisterEventHandler(object, bool)" />.
	/// </summary>
	/// <param name="handler">The event handler object.</param>
	/// <param name="wasRegisteredWithStatic">Whether it considered static methods.</param>
	public void UnregisterEventHandler(object handler, bool wasRegisteredWithStatic = false)
		=> this.UnregisterEventHandlerImpl(handler, handler.GetType(), wasRegisteredWithStatic);

	/// <summary>
	///     Perfectly mirrors <see cref="RegisterStaticEventHandler(Type)" />.
	/// </summary>
	/// <param name="t">Type to unregister.</param>
	public void UnregisterStaticEventHandler(Type t)
		=> this.UnregisterEventHandlerImpl(null, t);

	/// <summary>
	///     Perfectly mirrors <see cref="RegisterStaticEventHandler{T}()" />.
	/// </summary>
	/// <typeparam name="T">Type to unregister.</typeparam>
	public void UnregisterStaticEventHandler<T>()
		=> this.UnregisterEventHandler(typeof(T));

	/// <summary>
	///     Perfectly mirrors <see cref="RegisterEventHandler(Type)" />.
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
	///     Perfectly mirrors <see cref="RegisterEventHandler{T}()" />.
	/// </summary>
	/// <typeparam name="T">The type to unregister</typeparam>
	public void UnregisterEventHandler<T>() => this.UnregisterEventHandler(typeof(T));

	/// <summary>
	///     Perfectly mirrors <see cref="RegisterEventHandlers(Assembly)" />.
	/// </summary>
	/// <param name="assembly">The assembly to unregister.</param>
	public void UnregisterEventHandlers(Assembly assembly)
	{
		foreach (var t in GetEventHandlersFromAssembly(assembly))
			this.UnregisterEventHandler(t);
	}

	/// <summary>
	///     Gets the event handlers from the assembly.
	/// </summary>
	/// <param name="assembly">The assembly to get the event handlers from.</param>
	private static IEnumerable<Type> GetEventHandlersFromAssembly(Assembly assembly)
		=> assembly.GetTypes().Where(t => t.GetCustomAttribute<EventHandlerAttribute>() is not null);

	/// <summary>
	///     Unregisters event handler implementations.
	/// </summary>
	/// <param name="handler">The event handler object.</param>
	/// <param name="type">The type.</param>
	/// <param name="wasRegisteredWithStatic">Whether it considereded static methods.</param>
	private void UnregisterEventHandlerImpl(object? handler, Type type, bool wasRegisteredWithStatic = true)
	{
		if (this._registeredEventhandlers.ContainsKey(type))
			this._registeredEventhandlers.Remove(type);

		if (!this._registrationToDelegate.TryGetValue((handler, type, wasRegisteredWithStatic), out var delegateLists) || delegateLists.Count == 0)
			return;

		foreach (var (evnt, dlgt) in delegateLists[0])
			evnt.RemoveEventHandler(this, dlgt);

		delegateLists.RemoveAt(0);
		if (delegateLists.Count == 0)
			this._registrationToDelegate.Remove((handler, type, wasRegisteredWithStatic));
	}

	/// <summary>
	///     Rregisters event handler implementations.
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

		this._registrationToDelegate[(handler, type, registerStatic)] = this._registrationToDelegate.TryGetValue((handler, type, registerStatic), out var delList) ? delList : delList = [];

		delList.Add(delegates);
		this._registeredEventhandlers.Add(type, handler);

		foreach (var (evnt, dlgt) in delegates)
			evnt.AddEventHandler(this, dlgt);
	}
}
