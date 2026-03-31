namespace DisCatSharp.Enums;

/// <summary>
///     Specifies how user event handlers are dispatched after ordered internal processing.
/// </summary>
/// <remarks>
///     Regardless of the selected mode, internal cache and state mutations are always processed in FIFO order per shard.
///     This setting only controls how user-facing event handlers (<c>Client.MessageCreated += …</c>) are invoked after
///     the internal processing completes.
/// </remarks>
public enum GatewayDispatchMode
{
	/// <summary>
	///     After internal processing completes for a dispatch event, user event handlers are fired concurrently
	///     (fire-and-forget). Multiple events may have their user handlers running at the same time.
	///     <para>This is the default mode and provides the best throughput for most bots.</para>
	/// </summary>
	ConcurrentHandlers = 0,

	/// <summary>
	///     After internal processing completes for a dispatch event, user event handlers are awaited before the next
	///     dispatch event is processed. This fully serializes both internal processing and user handler execution.
	///     <para>Use this mode when your handlers depend on strict event ordering and cannot tolerate overlap.</para>
	/// </summary>
	SequentialHandlers = 1
}
