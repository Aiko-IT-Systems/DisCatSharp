// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.Dispose")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.VoiceWS_SocketMessage(DisCatSharp.Net.WebSocket.IWebSocketClient,DisCatSharp.EventArgs.SocketMessageEventArgs)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.WsSendAsync(System.String)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Interop.OpusNative.OpusGetLastPacketDuration(System.IntPtr,System.Int32@)")]
[assembly: SuppressMessage("Usage", "CA2253:Named placeholders should not be numeric values", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.HandleDispatch(Newtonsoft.Json.Linq.JObject)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2253:Named placeholders should not be numeric values", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.ProcessKeepalive(System.Byte[])")]
[assembly: SuppressMessage("Usage", "CA2253:Named placeholders should not be numeric values", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.Stage1(DisCatSharp.Voice.Entities.VoiceReadyPayload)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2253:Named placeholders should not be numeric values", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.Stage2(DisCatSharp.Voice.Entities.VoiceSessionDescriptionPayload)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2253:Named placeholders should not be numeric values", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.VoiceSenderTask~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2253:Named placeholders should not be numeric values", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.VoiceConnection.VoiceWS_SocketClosed(DisCatSharp.Net.WebSocket.IWebSocketClient,DisCatSharp.EventArgs.SocketCloseEventArgs)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Opus.GetLastPacketSampleCount(DisCatSharp.Voice.Codec.OpusDecoder)~System.Int32")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Opus.ProcessPacketLoss(DisCatSharp.Voice.Codec.OpusDecoder,System.Int32,System.Span{System.Byte}@)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Rtp.CalculatePacketSize(System.Int32,DisCatSharp.Voice.Codec.EncryptionMode)~System.Int32")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Rtp.DecodeHeader(System.ReadOnlySpan{System.Byte},System.UInt16@,System.UInt32@,System.UInt32@,System.Boolean@)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Rtp.EncodeHeader(System.UInt16,System.UInt32,System.UInt32,System.Span{System.Byte})")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Rtp.GetDataFromPacket(System.ReadOnlySpan{System.Byte},System.ReadOnlySpan{System.Byte}@,DisCatSharp.Voice.Codec.EncryptionMode)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Rtp.IsRtpHeader(System.ReadOnlySpan{System.Byte})~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Sodium.AppendNonce(System.ReadOnlySpan{System.Byte},System.Span{System.Byte},DisCatSharp.Voice.Codec.EncryptionMode)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Sodium.GenerateNonce(System.ReadOnlySpan{System.Byte},System.Span{System.Byte})")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Sodium.GenerateNonce(System.UInt32,System.Span{System.Byte})")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Codec.Sodium.GetNonce(System.ReadOnlySpan{System.Byte},System.Span{System.Byte},DisCatSharp.Voice.Codec.EncryptionMode)")]
[assembly: SuppressMessage("Usage", "DCS0102:[Discord] Deprecated", Justification = "<Pending>")]
[assembly: SuppressMessage("Usage", "DCS0101:[Discord] InExperiment", Justification = "<Pending>")]
[assembly: SuppressMessage("Usage", "DCS0103:[Discord] InExperiment", Justification = "<Pending>")]
[assembly: SuppressMessage("Usage", "DCS0001:[Discord] InExperiment", Justification = "<Pending>")]
[assembly: SuppressMessage("Usage", "DCS0002:[Discord] InExperiment", Justification = "<Pending>")]
[assembly: SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "<Pending>")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Entities.AudioSender.#ctor(System.UInt32,DisCatSharp.Voice.Codec.OpusDecoder)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Entities.VoicePacket.#ctor(System.ReadOnlyMemory{System.Byte},System.Int32,System.Boolean)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Net.RawVoicePacket.#ctor(System.Memory{System.Byte},System.Int32,System.Boolean)")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.Voice.Entities.AudioSender.GetTrueSequenceAfterWrapping(System.UInt16)~System.UInt64")]
