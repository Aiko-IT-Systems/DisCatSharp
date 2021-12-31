// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp.Common
{
    /// <summary>
    /// Provides a cryptographically-secure pseudorandom number generator (CSPRNG) implementation compatible with <see cref="System.Random"/>.
    /// </summary>
    public sealed class SecureRandom : Random, IDisposable
    {
        /// <summary>
        /// Gets the r n g.
        /// </summary>
        private RandomNumberGenerator Rng { get; } = RandomNumberGenerator.Create();

        private volatile bool _isDisposed = false;

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
        /// <param name="Buffer">Buffer to fill with random bytes.</param>
        public void GetBytes(byte[] Buffer)
        {
            this.Rng.GetBytes(Buffer);
        }

        /// <summary>
        /// Fills a supplied buffer with random nonzero bytes.
        /// </summary>
        /// <param name="Buffer">Buffer to fill with random nonzero bytes.</param>
        public void GetNonZeroBytes(byte[] Buffer)
        {
            this.Rng.GetNonZeroBytes(Buffer);
        }

        /// <summary>
        /// Fills a supplied memory region with random bytes.
        /// </summary>
        /// <param name="Buffer">Memmory region to fill with random bytes.</param>
        public void GetBytes(Span<byte> Buffer)
        {
#if NETCOREAPP
            this.RNG.GetBytes(buffer);
#else
            var buff = ArrayPool<byte>.Shared.Rent(Buffer.Length);
            try
            {
                var buffSpan = buff.AsSpan(0, Buffer.Length);
                this.Rng.GetBytes(buff);
                buffSpan.CopyTo(Buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buff);
            }
#endif
        }

        /// <summary>
        /// Fills a supplied memory region with random nonzero bytes.
        /// </summary>
        /// <param name="Buffer">Memmory region to fill with random nonzero bytes.</param>
        public void GetNonZeroBytes(Span<byte> Buffer)
        {
#if NETCOREAPP
            this.RNG.GetNonZeroBytes(buffer);
#else
            var buff = ArrayPool<byte>.Shared.Rent(Buffer.Length);
            try
            {
                var buffSpan = buff.AsSpan(0, Buffer.Length);
                this.Rng.GetNonZeroBytes(buff);
                buffSpan.CopyTo(Buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buff);
            }
#endif
        }

        /// <summary>
        /// Generates a signed 8-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="sbyte.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public sbyte GetInt8(sbyte Min = 0, sbyte Max = sbyte.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            var offset = (sbyte)(Min < 0 ? -Min : 0);
            Min += offset;
            Max += offset;

            return (sbyte)(Math.Abs(this.Generate<sbyte>()) % (Max - Min) + Min - offset);
        }

        /// <summary>
        /// Generates a unsigned 8-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="byte.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public byte GetUInt8(byte Min = 0, byte Max = byte.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            return (byte)(this.Generate<byte>() % (Max - Min) + Min);
        }

        /// <summary>
        /// Generates a signed 16-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="short.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public short GetInt16(short Min = 0, short Max = short.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            var offset = (short)(Min < 0 ? -Min : 0);
            Min += offset;
            Max += offset;

            return (short)(Math.Abs(this.Generate<short>()) % (Max - Min) + Min - offset);
        }

        /// <summary>
        /// Generates a unsigned 16-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="ushort.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public ushort GetUInt16(ushort Min = 0, ushort Max = ushort.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            return (ushort)(this.Generate<ushort>() % (Max - Min) + Min);
        }

        /// <summary>
        /// Generates a signed 32-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="int.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public int GetInt32(int Min = 0, int Max = int.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            var offset = Min < 0 ? -Min : 0;
            Min += offset;
            Max += offset;

            return Math.Abs(this.Generate<int>()) % (Max - Min) + Min - offset;
        }

        /// <summary>
        /// Generates a unsigned 32-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="uint.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public uint GetUInt32(uint Min = 0, uint Max = uint.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            return this.Generate<uint>() % (Max - Min) + Min;
        }

        /// <summary>
        /// Generates a signed 64-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="long.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public long GetInt64(long Min = 0, long Max = long.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            var offset = Min < 0 ? -Min : 0;
            Min += offset;
            Max += offset;

            return Math.Abs(this.Generate<long>()) % (Max - Min) + Min - offset;
        }

        /// <summary>
        /// Generates a unsigned 64-bit integer within specified range.
        /// </summary>
        /// <param name="Min">Minimum value to generate. Defaults to 0.</param>
        /// <param name="Max">Maximum value to generate. Defaults to <see cref="ulong.MaxValue"/>.</param>
        /// <returns>Generated random value.</returns>
        public ulong GetUInt64(ulong Min = 0, ulong Max = ulong.MaxValue)
        {
            if (Max <= Min)
                throw new ArgumentException("Maximum needs to be greater than minimum.", nameof(Max));

            return this.Generate<ulong>() % (Max - Min) + Min;
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
        /// Generates a 32-bit integer between 0 and <paramref name="MaxValue"/>. Upper end exclusive.
        /// </summary>
        /// <param name="MaxValue">Maximum value of the generated integer.</param>
        /// <returns>Generated 32-bit integer.</returns>
        public override int Next(int MaxValue)
            => this.GetInt32(0, MaxValue);

        /// <summary>
        /// Generates a 32-bit integer between <paramref name="MinValue"/> and <paramref name="MaxValue"/>. Upper end exclusive.
        /// </summary>
        /// <param name="MinValue">Minimum value of the generate integer.</param>
        /// <param name="MaxValue">Maximum value of the generated integer.</param>
        /// <returns>Generated 32-bit integer.</returns>
        public override int Next(int MinValue, int MaxValue)
            => this.GetInt32(MinValue, MaxValue);

        /// <summary>
        /// Generates a 64-bit floating-point number between 0.0 and 1.0. Upper end exclusive.
        /// </summary>
        /// <returns>Generated 64-bit floating-point number.</returns>
        public override double NextDouble()
            => this.GetDouble();

        /// <summary>
        /// Fills specified buffer with random bytes.
        /// </summary>
        /// <param name="Buffer">Buffer to fill with bytes.</param>
        public override void NextBytes(byte[] Buffer)
            => this.GetBytes(Buffer);

        /// <summary>
        /// Fills specified memory region with random bytes.
        /// </summary>
        /// <param name="Buffer">Memory region to fill with bytes.</param>
#if NETCOREAPP
        override
#endif
        public new void NextBytes(Span<byte> Buffer)
            => this.GetBytes(Buffer);

        /// <summary>
        /// Disposes this <see cref="SecureRandom"/> instance and its resources.
        /// </summary>
        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            this.Rng.Dispose();
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
}
