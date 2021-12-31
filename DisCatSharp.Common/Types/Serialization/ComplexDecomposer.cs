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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DisCatSharp.Common.Serialization
{
    /// <summary>
    /// Decomposes <see cref="System.Numerics.Complex"/> numbers into tuples (arrays of 2).
    /// </summary>
    public sealed class ComplexDecomposer : IDecomposer
    {
        /// <summary>
        /// Gets the t complex.
        /// </summary>
        private static Type Complex { get; } = typeof(Complex);
        /// <summary>
        /// Gets the t double array.
        /// </summary>
        private static Type DoubleArray { get; } = typeof(double[]);
        /// <summary>
        /// Gets the t double enumerable.
        /// </summary>
        private static Type DoubleEnumerable { get; } = typeof(IEnumerable<double>);
        /// <summary>
        /// Gets the t object array.
        /// </summary>
        private static Type ObjectArray { get; } = typeof(object[]);
        /// <summary>
        /// Gets the t object enumerable.
        /// </summary>
        private static Type ObjectEnumerable { get; } = typeof(IEnumerable<object>);

        /// <inheritdoc />
        public bool CanDecompose(Type T)
            => T == Complex;

        /// <inheritdoc />
        public bool CanRecompose(Type T)
            => T == DoubleArray
            || T == ObjectArray
            || DoubleEnumerable.IsAssignableFrom(T)
            || ObjectEnumerable.IsAssignableFrom(T);

        /// <inheritdoc />
        public bool TryDecompose(object Obj, Type Tobj, out object Decomposed, out Type Tdecomposed)
        {
            Decomposed = null;
            Tdecomposed = DoubleArray;

            if (Tobj != Complex || Obj is not Complex c)
                return false;

            Decomposed = new[] { c.Real, c.Imaginary };
            return true;
        }

        /// <inheritdoc />
        public bool TryRecompose(object Obj, Type Tobj, Type Trecomposed, out object Recomposed)
        {
            Recomposed = null;

            if (Trecomposed != Complex)
                return false;

            // ie<double>
            if (DoubleEnumerable.IsAssignableFrom(Tobj) && Obj is IEnumerable<double> ied)
            {
                if (ied.Count() < 2)
                    return false;

                var (real, imag) = ied.FirstTwoOrDefault();
                Recomposed = new Complex(real, imag);
                return true;
            }

            // ie<obj>
            if (ObjectEnumerable.IsAssignableFrom(Tobj) && Obj is IEnumerable<object> ieo)
            {
                if (ieo.Count() < 2)
                    return false;

                var (real, imag) = ieo.FirstTwoOrDefault();
                if (real is not double dreal || imag is not double dimag)
                    return false;

                Recomposed = new Complex(dreal, dimag);
                return true;
            }

            return false;
        }
    }
}
