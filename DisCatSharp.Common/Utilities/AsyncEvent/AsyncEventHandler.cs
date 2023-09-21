using System.Threading.Tasks;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Handles an asynchronous event of type <see cref="AsyncEvent{TSender, TArgs}"/>. The handler will take an instance of <typeparamref name="TArgs"/> as its arguments.
/// </summary>
/// <typeparam name="TSender">Type of the object that dispatches this event.</typeparam>
/// <typeparam name="TArgs">Type of the object which holds arguments for this event.</typeparam>
/// <param name="sender">Object which raised this event.</param>
/// <param name="e">Arguments for this event.</param>
/// <returns></returns>
public delegate Task AsyncEventHandler<in TSender, in TArgs>(TSender sender, TArgs e) where TArgs : AsyncEventArgs;
