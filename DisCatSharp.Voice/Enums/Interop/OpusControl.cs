namespace DisCatSharp.Voice.Enums.Interop;

/// <summary>
///     The opus control.
/// </summary>
internal enum OpusControl
{
	SetBitrate = 4002,
	SetBandwidth = 4008,
	SetInBandFec = 4012,
	SetPacketLossPercent = 4014,
	SetSignal = 4024,
	ResetState = 4028,
	GetLastPacketDuration = 4039
}
