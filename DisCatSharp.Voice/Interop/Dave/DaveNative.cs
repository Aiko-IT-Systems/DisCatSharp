using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.Voice.Interop.Dave;

/// <summary>
///     P/Invoke declarations for the native libdave library.
///     All functions map directly to the C API declared in <c>dave.h</c>.
/// </summary>
/// <remarks>
///     No logic belongs in this class — declarations only.
///     Memory ownership: any <c>byte**</c> output buffer is allocated by libdave
///     and must be freed with <see cref="Free"/>.
/// </remarks>
internal static unsafe class DaveNative
{
	private const string LibName = "libdave";

	// -------------------------------------------------------------------------
	// Session
	// -------------------------------------------------------------------------

	/// <summary>Creates a new libdave MLS session. The returned handle must be destroyed with <see cref="SessionDestroy"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveSessionCreate", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern DaveSessionSafeHandle SessionCreate(
		IntPtr context,
		[MarshalAs(UnmanagedType.LPUTF8Str)] string authSessionId,
		IntPtr failureCallback,
		IntPtr userData);

	/// <summary>Destroys a libdave MLS session and frees all associated native memory. Called automatically by <see cref="DaveSessionSafeHandle"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveSessionDestroy", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void SessionDestroy(IntPtr session);

	/// <summary>Initialises a libdave MLS session for the given protocol version, group (channel) ID, and local user ID.</summary>
	[DllImport(LibName, EntryPoint = "daveSessionInit", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void SessionInit(
		DaveSessionSafeHandle session,
		ushort protocolVersion,
		ulong groupId,
		[MarshalAs(UnmanagedType.LPUTF8Str)] string selfUserId);

	/// <summary>Resets the MLS session state without destroying the session handle. Used on reconnect or invalid commit.</summary>
	[DllImport(LibName, EntryPoint = "daveSessionReset", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void SessionReset(DaveSessionSafeHandle session);

	/// <summary>Passes the external sender credential (binary OP 25 payload) to the libdave session.</summary>
	[DllImport(LibName, EntryPoint = "daveSessionSetExternalSender", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void SessionSetExternalSender(
		DaveSessionSafeHandle session,
		byte* data,
		nuint length);

	/// <summary>
	///     Retrieves the serialised MLS key package from the session.
	///     The output buffer is allocated by libdave and must be freed with <see cref="Free"/>.
	/// </summary>
	[DllImport(LibName, EntryPoint = "daveSessionGetMarshalledKeyPackage", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void SessionGetMarshalledKeyPackage(
		DaveSessionSafeHandle session,
		out byte* keyPackage,
		out nuint length);

	/// <summary>
	///     Processes MLS proposals (binary OP 27) and generates a commit+welcome bundle.
	///     The output <c>commitWelcome</c> buffer is allocated by libdave and must be freed with <see cref="Free"/>.
	/// </summary>
	[DllImport(LibName, EntryPoint = "daveSessionProcessProposals", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void SessionProcessProposals(
		DaveSessionSafeHandle session,
		byte* proposals,
		nuint proposalsLength,
		byte** userIds,
		nuint userCount,
		out byte* commitWelcome,
		out nuint commitWelcomeLength);

	/// <summary>
	///     Applies an MLS commit (binary OP 29) and returns an opaque result handle.
	///     The result handle must be freed with <see cref="CommitResultDestroy"/>.
	/// </summary>
	[DllImport(LibName, EntryPoint = "daveSessionProcessCommit", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern IntPtr SessionProcessCommit(
		DaveSessionSafeHandle session,
		byte* commit,
		nuint commitLength);

	/// <summary>
	///     Processes an MLS welcome message (binary OP 30) and returns an opaque result handle.
	///     The result handle must be freed with <see cref="WelcomeResultDestroy"/>.
	/// </summary>
	[DllImport(LibName, EntryPoint = "daveSessionProcessWelcome", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern IntPtr SessionProcessWelcome(
		DaveSessionSafeHandle session,
		byte* welcome,
		nuint welcomeLength,
		byte** userIds,
		nuint userCount);

	/// <summary>
	///     Returns the key ratchet handle for the specified user ID after a commit or welcome has been applied.
	///     Ownership of the handle transfers to the caller; it must be destroyed with <see cref="KeyRatchetDestroy"/>.
	/// </summary>
	[DllImport(LibName, EntryPoint = "daveSessionGetKeyRatchet", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern DaveKeyRatchetSafeHandle SessionGetKeyRatchet(
		DaveSessionSafeHandle session,
		[MarshalAs(UnmanagedType.LPUTF8Str)] string userId);

	// -------------------------------------------------------------------------
	// Key ratchet
	// -------------------------------------------------------------------------

	/// <summary>Destroys a libdave key ratchet handle. Called automatically by <see cref="DaveKeyRatchetSafeHandle"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveKeyRatchetDestroy", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void KeyRatchetDestroy(IntPtr handle);

	// -------------------------------------------------------------------------
	// Commit result
	// -------------------------------------------------------------------------

	/// <summary>Returns <see langword="true"/> when the commit result indicates a failure.</summary>
	[DllImport(LibName, EntryPoint = "daveCommitResultIsFailed", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool CommitResultIsFailed(IntPtr handle);

	/// <summary>Returns <see langword="true"/> when the commit result was ignored (e.g., duplicate or out-of-order).</summary>
	[DllImport(LibName, EntryPoint = "daveCommitResultIsIgnored", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool CommitResultIsIgnored(IntPtr handle);

	/// <summary>Destroys the opaque commit result handle returned by <see cref="SessionProcessCommit"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveCommitResultDestroy", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void CommitResultDestroy(IntPtr handle);

	// -------------------------------------------------------------------------
	// Welcome result
	// -------------------------------------------------------------------------

	/// <summary>Destroys the opaque welcome result handle returned by <see cref="SessionProcessWelcome"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveWelcomeResultDestroy", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void WelcomeResultDestroy(IntPtr handle);

	// -------------------------------------------------------------------------
	// Encryptor
	// -------------------------------------------------------------------------

	/// <summary>Creates a new native libdave encryptor. The returned handle must be destroyed with <see cref="EncryptorDestroy"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveEncryptorCreate", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern DaveEncryptorSafeHandle EncryptorCreate();

	/// <summary>Destroys a libdave encryptor and frees all associated native memory. Called automatically by <see cref="DaveEncryptorSafeHandle"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveEncryptorDestroy", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void EncryptorDestroy(IntPtr handle);

	/// <summary>Installs a key ratchet into the encryptor, enabling frame encryption. libdave copies the ratchet internally.</summary>
	[DllImport(LibName, EntryPoint = "daveEncryptorSetKeyRatchet", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void EncryptorSetKeyRatchet(
		DaveEncryptorSafeHandle encryptor,
		DaveKeyRatchetSafeHandle ratchet);

	/// <summary>Associates an SSRC with a codec type so the encryptor applies correct frame partitioning. Must be called before the first <see cref="EncryptorEncrypt"/> for the given SSRC.</summary>
	[DllImport(LibName, EntryPoint = "daveEncryptorAssignSsrcToCodec", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void EncryptorAssignSsrcToCodec(
		DaveEncryptorSafeHandle encryptor,
		uint ssrc,
		int codecType);

	/// <summary>Enables or disables passthrough mode on the encryptor. In passthrough mode frames are forwarded unencrypted.</summary>
	[DllImport(LibName, EntryPoint = "daveEncryptorSetPassthroughMode", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void EncryptorSetPassthroughMode(
		DaveEncryptorSafeHandle encryptor,
		[MarshalAs(UnmanagedType.I1)] bool passthrough);

	/// <summary>
	///     Encrypts a single media frame. Returns 0 on success with <paramref name="bytesWritten"/> set to the output length.
	///     The <paramref name="ssrc"/> is embedded in the SFrame header for sender identification.
	/// </summary>
	[DllImport(LibName, EntryPoint = "daveEncryptorEncrypt", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern int EncryptorEncrypt(
		DaveEncryptorSafeHandle encryptor,
		int mediaType,
		uint ssrc,
		byte* frame,
		nuint frameLength,
		byte* outFrame,
		nuint outCapacity,
		out nuint bytesWritten);

	// -------------------------------------------------------------------------
	// Decryptor
	// -------------------------------------------------------------------------

	/// <summary>Creates a new native libdave decryptor. The returned handle must be destroyed with <see cref="DecryptorDestroy"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveDecryptorCreate", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern DaveDecryptorSafeHandle DecryptorCreate();

	/// <summary>Destroys a libdave decryptor and frees all associated native memory. Called automatically by <see cref="DaveDecryptorSafeHandle"/>.</summary>
	[DllImport(LibName, EntryPoint = "daveDecryptorDestroy", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void DecryptorDestroy(IntPtr handle);

	/// <summary>Transitions the decryptor to a new key ratchet epoch. The previous ratchet is retained briefly for in-flight frames.</summary>
	[DllImport(LibName, EntryPoint = "daveDecryptorTransitionToKeyRatchet", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void DecryptorTransitionToKeyRatchet(
		DaveDecryptorSafeHandle decryptor,
		DaveKeyRatchetSafeHandle ratchet);

	/// <summary>Enables or disables passthrough mode on the decryptor. In passthrough mode frames that lack a DAVE footer are forwarded as-is.</summary>
	[DllImport(LibName, EntryPoint = "daveDecryptorTransitionToPassthroughMode", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void DecryptorTransitionToPassthroughMode(
		DaveDecryptorSafeHandle decryptor,
		[MarshalAs(UnmanagedType.I1)] bool passthrough);

	/// <summary>
	///     Decrypts a single media frame. Returns 0 on success with <paramref name="bytesWritten"/> set to the output length.
	/// </summary>
	[DllImport(LibName, EntryPoint = "daveDecryptorDecrypt", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern int DecryptorDecrypt(
		DaveDecryptorSafeHandle decryptor,
		int mediaType,
		byte* frame,
		nuint frameLength,
		byte* outFrame,
		nuint outCapacity,
		out nuint bytesWritten);

	// -------------------------------------------------------------------------
	// Memory
	// -------------------------------------------------------------------------

	/// <summary>Frees a buffer that was allocated by libdave (e.g., the key package, commit/welcome bytes).</summary>
	[DllImport(LibName, EntryPoint = "daveFree", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void Free(IntPtr ptr);
}
