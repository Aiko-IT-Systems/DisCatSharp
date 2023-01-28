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

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DisCatSharp.Common;

/// <summary>
/// Provides a cryptographically-secure pseudorandom number generator (CSPRNG) implementation compatible with <see cref="Random"/>.
/// </summary>
public sealed class SecureRandom : Random, IDisposable
{
	/// <summary>
	/// Gets the r n g.
	/// </summary>
	private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

	private volatile bool _isDisposed;

	/// <summary>
	/// Creates a new instance of <see cref="SecureRandom"/>.
	/// </summary>
	public SecureRandom()
	{ }

	/// <summary>
	/// Finalizes this <see cref="SecureRandom"/> instance by disposing it.
	/// </summary>
	~SecureRandom()
	{
		this.Dispose();
	}

	/// <summary>
	/// Fills a supplied buffer with random bytes.
	/// </summary>
	/// <param name="buffer">Buffer to fill with random bytes.</param>
	public void GetBytes(byte[] buffer) => this._rng.GetBytes(buffer);

	/// <summary>
	/// Fills a supplied buffer with random nonzero bytes.
	/// </summary>
	/// <param name="buffer">Buffer to fill with random nonzero bytes.</param>
	public void GetNonZeroBytes(byte[] buffer) => this._rng.GetNonZeroBytes(buffer);

	/// <summary>
	/// Fills a supplied memory region with random bytes.
	/// </summary>
	/// <param name="buffer">Memory region to fill with random bytes.</param>
	public void GetBytes(Span<byte> buffer)
	{
		var buff = ArrayPool<byte>.Shared.Rent(buffer.Length);
		try
		{
			var buffSpan = buff.AsSpan(0, buffer.Length);
			this._rng.GetBytes(buff);
			buffSpan.CopyTo(buffer);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buff);
		}
	}

	/// <summary>
	/// Fills a supplied memory region with random nonzero bytes.
	/// </summary>
	/// <param name="buffer">Memory region to fill with random nonzero bytes.</param>
	public void GetNonZeroBytes(Span<byte> buffer)
	{
		var buff = ArrayPool<byte>.Shared.Rent(buffer.Length);
		try
		{
			var buffSpan = buff.AsSpan(0, buffer.Length);
			this._rng.GetNonZeroBytes(buff);
			buffSpan.CopyTo(buffer);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buff);
		}
	}

	/// <summary>
	/// Generates a signed 8-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="sbyte.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public sbyte GetInt8(sbyte min = 0, sbyte max = sbyte.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		var offset = (sbyte)(min < 0 ? -min : 0);
		min += offset;
		max += offset;

		return (sbyte)(Math.Abs(this.Generate<sbyte>()) % (max - min) + min - offset);
	}

	/// <summary>
	/// Generates a unsigned 8-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="byte.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public byte GetUInt8(byte min = 0, byte max = byte.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		return (byte)(this.Generate<byte>() % (max - min) + min);
	}

	/// <summary>
	/// Generates a signed 16-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="short.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public short GetInt16(short min = 0, short max = short.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		var offset = (short)(min < 0 ? -min : 0);
		min += offset;
		max += offset;

		return (short)(Math.Abs(this.Generate<short>()) % (max - min) + min - offset);
	}

	/// <summary>
	/// Generates a unsigned 16-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="ushort.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public ushort GetUInt16(ushort min = 0, ushort max = ushort.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		return (ushort)(this.Generate<ushort>() % (max - min) + min);
	}

	/// <summary>
	/// Generates a signed 32-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="int.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public int GetInt32(int min = 0, int max = int.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		var offset = min < 0 ? -min : 0;
		min += offset;
		max += offset;

		return Math.Abs(this.Generate<int>()) % (max - min) + min - offset;
	}

	/// <summary>
	/// Generates a unsigned 32-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="uint.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public uint GetUInt32(uint min = 0, uint max = uint.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		return this.Generate<uint>() % (max - min) + min;
	}

	/// <summary>
	/// Generates a signed 64-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="long.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public long GetInt64(long min = 0, long max = long.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		var offset = min < 0 ? -min : 0;
		min += offset;
		max += offset;

		return Math.Abs(this.Generate<long>()) % (max - min) + min - offset;
	}

	/// <summary>
	/// Generates a unsigned 64-bit integer within specified range.
	/// </summary>
	/// <param name="min">Minimum value to generate. Defaults to 0.</param>
	/// <param name="max">Maximum value to generate. Defaults to <see cref="ulong.MaxValue"/>.</param>
	/// <returns>Generated random value.</returns>
	public ulong GetUInt64(ulong min = 0, ulong max = ulong.MaxValue)
	{
		if (max <= min)
			throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(max));

		return this.Generate<ulong>() % (max - min) + min;
	}

	/// <summary>
	/// Generates a 32-bit floating-point number between 0.0 and 1.0.
	/// </summary>
	/// <returns>Generated 32-bit floating-point number.</returns>
	public float GetSingle()
	{
		var (i1, i2) = ((float)this.GetInt32(), (float)this.GetInt32());
		return i1 / i2 % 1.0F;
	}

	/// <summary>
	/// Generates a 64-bit floating-point number between 0.0 and 1.0.
	/// </summary>
	/// <returns>Generated 64-bit floating-point number.</returns>
	public double GetDouble()
	{
		var (i1, i2) = ((double)this.GetInt64(), (double)this.GetInt64());
		return i1 / i2 % 1.0;
	}

	/// <summary>
	/// Generates a 32-bit integer between 0 and <see cref="int.MaxValue"/>. Upper end exclusive.
	/// </summary>
	/// <returns>Generated 32-bit integer.</returns>
	public override int Next()
		=> this.GetInt32();

	/// <summary>
	/// Generates a 32-bit integer between 0 and <paramref name="maxValue"/>. Upper end exclusive.
	/// </summary>
	/// <param name="maxValue">Maximum value of the generated integer.</param>
	/// <returns>Generated 32-bit integer.</returns>
	public override int Next(int maxValue)
		=> this.GetInt32(0, maxValue);

	/// <summary>
	/// Generates a 32-bit integer between <paramref name="minValue"/> and <paramref name="maxValue"/>. Upper end exclusive.
	/// </summary>
	/// <param name="minValue">Minimum value of the generate integer.</param>
	/// <param name="maxValue">Maximum value of the generated integer.</param>
	/// <returns>Generated 32-bit integer.</returns>
	public override int Next(int minValue, int maxValue)
		=> this.GetInt32(minValue, maxValue);

	/// <summary>
	/// Generates a 64-bit floating-point number between 0.0 and 1.0. Upper end exclusive.
	/// </summary>
	/// <returns>Generated 64-bit floating-point number.</returns>
	public override double NextDouble()
		=> this.GetDouble();

	/// <summary>
	/// Fills specified buffer with random bytes.
	/// </summary>
	/// <param name="buffer">Buffer to fill with bytes.</param>
	public override void NextBytes(byte[] buffer)
		=> this.GetBytes(buffer);

	/// <summary>
	/// Fills specified memory region with random bytes.
	/// </summary>
	/// <param name="buffer">Memory region to fill with bytes.</param>
	public new void NextBytes(Span<byte> buffer)
		=> this.GetBytes(buffer);

	/// <summary>
	/// Disposes this <see cref="SecureRandom"/> instance and its resources.
	/// </summary>
	public void Dispose()
	{
		if (this._isDisposed)
			return;

		this._isDisposed = true;
		this._rng.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Generates a random 64-bit floating-point number between 0.0 and 1.0. Upper end exclusive.
	/// </summary>
	/// <returns>Generated 64-bit floating-point number.</returns>
	protected override double Sample()
		=> this.GetDouble();

	/// <summary>
	/// Generates the.
	/// </summary>
	/// <returns>A T.</returns>
	private T Generate<T>() where T : struct
	{
		var size = Unsafe.SizeOf<T>();
		Span<byte> buff = stackalloc byte[size];
		this.GetBytes(buff);
		return MemoryMarshal.Read<T>(buff);
	}
}
