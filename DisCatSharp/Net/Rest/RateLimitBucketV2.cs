using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Net.V2;

public sealed class RateLimitBucketV2 : IDisposable
{
	private readonly AsyncManualResetEvent _resetEvent = new(true);
	private int _isDisposed;
	private int _remaining;
	private long _resetAfterTicks;
	private long _resetTicks;

	public RateLimitBucketV2(string bucketId, ILogger logger)
	{
		this.BucketId = bucketId;
		this.Logger = logger;
	}

	public string BucketId { get; }
	public int Maximum { get; private set; }
	public DateTimeOffset LastAttempt { get; set; }
	public bool IsGlobal { get; set; }
	public bool IsUnlimited { get; private set; }
	public ILogger Logger { get; }

	public DateTimeOffset Reset => new(Interlocked.Read(ref this._resetTicks), TimeSpan.Zero);
	public DateTimeOffset ResetAfterOffset => new(Interlocked.Read(ref this._resetAfterTicks), TimeSpan.Zero);
	public TimeSpan ServerTimeDelta { get; set; }

	public void Dispose()
	{
		if (Interlocked.Exchange(ref this._isDisposed, 1) == 1)
			return;

		this._resetEvent.Reset();
		this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "[V2] Disposed bucket: {Bucket}", this.BucketId);
	}

	public async Task WaitAsync()
	{
		this.ThrowIfDisposed();
		await this._resetEvent.WaitAsync().ConfigureAwait(false);

		var now = DateTimeOffset.UtcNow + this.ServerTimeDelta;
		var reset = this.ResetAfterOffset > DateTimeOffset.MinValue ? this.ResetAfterOffset : this.Reset;

		if (Interlocked.Decrement(ref this._remaining) >= 0 && now < reset)
			return;

		var delay = reset - now;
		if (delay < TimeSpan.Zero)
			delay = TimeSpan.Zero;

		this.Logger.LogDebug(LoggerEvents.RatelimitPreemptive, "[V2] Rate limit wait: {Bucket} for {Delay}", this.BucketId, delay);
		await Task.Delay(delay).ConfigureAwait(false);
		this.Release();
	}

	public void Release()
	{
		this.ThrowIfDisposed();
		this._resetEvent.SetAsync().ConfigureAwait(false);
	}

	public void UpdateLimits(int remaining, int maximum, DateTimeOffset reset, TimeSpan? resetAfter = null)
	{
		this.ThrowIfDisposed();
		this.Maximum = maximum;
		Interlocked.Exchange(ref this._remaining, remaining);
		this.SetReset(reset);

		if (resetAfter.HasValue)
			Interlocked.Exchange(ref this._resetAfterTicks,
				DateTimeOffset.UtcNow.Add(resetAfter.Value).UtcTicks);

		this.LastAttempt = DateTimeOffset.UtcNow;
		this.IsUnlimited = maximum == 0;
		this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "[V2] Updated bucket {Bucket}: {BucketString}", this.BucketId, this.ToString());
	}

	/// <inheritdoc />
	public override string ToString()
		=> $"{(this.IsGlobal ? "Global" : "Route")} bucket {this.BucketId} [{this._remaining}/{(this.Maximum is not 0 ? this.Maximum : "unlimited")}] - Resets after: {this.ResetAfterOffset} - Reset: {this.Reset} - Last Attempt: {this.LastAttempt}";

	public void SetReset(DateTimeOffset reset)
	{
		this.ThrowIfDisposed();
		Interlocked.Exchange(ref this._resetTicks, reset.UtcTicks);
	}

	private void ThrowIfDisposed()
	{
		if (Interlocked.CompareExchange(ref this._isDisposed, 0, 0) == 1)
			throw new ObjectDisposedException($"Bucket {this.BucketId} is disposed");
	}
}
