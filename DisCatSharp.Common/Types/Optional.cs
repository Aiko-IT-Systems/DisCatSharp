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
using System.Diagnostics;

namespace DisCatSharp.Common
{
    /// <summary>
    /// Represents a property with an optional value.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    public struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>
    {
        /// <summary>
        /// Gets an initialized instance of <see cref="Optional{T}"/> which has no value set.
        /// </summary>
        public static Optional<T> Default { get; } = new Optional<T>();

        /// <summary>
        /// Gets whether the value of this <see cref="Optional{T}"/> is present.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the value of this <see cref="Optional{T}"/>. Will throw if a value is not present.
        /// </summary>
        public T Value
            => this.HasValue ? this._value : throw new InvalidOperationException("This property has no value set.");
        private readonly T _value;

        /// <summary>
        /// Gets the debugger display.
        /// </summary>
        private string DebuggerDisplay
            => this.HasValue ? $"Optional<{typeof(T)}> <has value> {this._value?.ToString() ?? "<null>"}" : $"Optional<{typeof(T)}> <has no value>";

        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> property with specified value.
        /// </summary>
        /// <param name="Value">Value of this property.</param>
        public Optional(T Value)
        {
            this.HasValue = true;
            this._value = Value;
        }

        /// <summary>
        /// Returns hash code of the underlying value.
        /// </summary>
        /// <returns>Hash code of the underlying value.</returns>
        public override int GetHashCode()
            => this.HasValue ? this._value?.GetHashCode() ?? 0 : 0;

        /// <summary>
        /// Checks whether the value of this property is equal to another value.
        /// </summary>
        /// <param name="Obj">Object to compare against.</param>
        /// <returns>Whether the supplied object is equal to the value of this property.</returns>
        public override bool Equals(object Obj)
        {
            if (Obj is Optional<T> opt)
                return this.Equals(opt);

            if (Obj is T val)
                return this.Equals(val);

            if (!this.HasValue && Obj == null)
                return true;

            if (this.HasValue)
                return object.Equals(this._value, Obj);

            return false;
        }

        /// <summary>
        /// Checks whether this property is equal to another property.
        /// </summary>
        /// <param name="Other">Property to compare against.</param>
        /// <returns>Whether the supplied property is equal to this property.</returns>
        public bool Equals(Optional<T> Other)
        {
            if (!this.HasValue && !Other.HasValue)
                return true;
            else if (this.HasValue != Other.HasValue)
                return false;
            else
                return object.Equals(this._value, Other._value);
        }

        /// <summary>
        /// Checks whether this proerty's value is equal to another value.
        /// </summary>
        /// <param name="Other">Value to compare this property's value against.</param>
        /// <returns>Whether the supplied value is equal to the value of this property.</returns>
        public bool Equals(T Other)
        {
            if (!this.HasValue && Other == null)
                return true;

            if (this.HasValue)
                return object.Equals(this._value, Other);

            return false;
        }

        /// <summary>
        /// Returns a string representation of the underlying value, if present.
        /// </summary>
        /// <returns>String representation of the underlying value, if present.</returns>
        public override string ToString()
            => this.HasValue ? this._value?.ToString() : "<no value>";

        /// <summary>
        /// Converts a specified value into an optional property of the value's type. The resulting property will have
        /// its value set to the supplied one.
        /// </summary>
        /// <param name="Value">Value to convert into an optional property.</param>
        public static implicit operator Optional<T>(T Value)
            => new Optional<T>(Value);

        /// <summary>
        /// Compares two properties and returns whether they are equal.
        /// </summary>
        /// <param name="Left">Property to compare against.</param>
        /// <param name="Right">Property to compare.</param>
        /// <returns>Whether the two properties are equal.</returns>
        public static bool operator ==(Optional<T> Left, Optional<T> Right)
            => Left.Equals(Right);

        /// <summary>
        /// Compares two properties and returns whether they are not equal.
        /// </summary>
        /// <param name="Left">Property to compare against.</param>
        /// <param name="Right">Property to compare.</param>
        /// <returns>Whether the two properties are not equal.</returns>
        public static bool operator !=(Optional<T> Left, Optional<T> Right)
            => !Left.Equals(Right);

        /// <summary>
        /// Compares a property's value against another value, and returns whether they are equal.
        /// </summary>
        /// <param name="Left">Property to compare against.</param>
        /// <param name="Right">Value to compare.</param>
        /// <returns>Whether the property's value is equal to the specified value.</returns>
        public static bool operator ==(Optional<T> Left, T Right)
            => Left.Equals(Right);

        /// <summary>
        /// Compares a property's value against another value, and returns whether they are not equal.
        /// </summary>
        /// <param name="Left">Property to compare against.</param>
        /// <param name="Right">Value to compare.</param>
        /// <returns>Whether this property's value is not equal to the specified value.</returns>
        public static bool operator !=(Optional<T> Left, T Right)
            => !Left.Equals(Right);

        /// <summary>
        /// Checks whether specified property has a value.
        /// </summary>
        /// <param name="Opt">Property to check.</param>
        /// <returns>Whether the property has a value.</returns>
        public static bool operator true(Optional<T> Opt)
            => Opt.HasValue;

        /// <summary>
        /// Checks whether specified property has no value.
        /// </summary>
        /// <param name="Opt">Property to check.</param>
        /// <returns>Whether the property has no value.</returns>
        public static bool operator false(Optional<T> Opt)
            => !Opt.HasValue;
    }

    /// <summary>
    /// Utilities for creation of optional properties.
    /// </summary>
    public static class Optional
    {
        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> from a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value to create an optional property for.</typeparam>
        /// <param name="Value">Value to set the property to.</param>
        /// <returns>Created optional property, which has a specified value set.</returns>
        public static Optional<T> FromValue<T>(T Value)
            => new Optional<T>(Value);

        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> from a default value for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value to create an optional property for.</typeparam>
        /// <returns>Created optional property, which has a default value for <typeparamref name="T"/> set.</returns>
        public static Optional<T> FromDefaultValue<T>()
            => new Optional<T>(default);

        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> which has no value.
        /// </summary>
        /// <typeparam name="T">Type of the value to create an optional property for.</typeparam>
        /// <returns>Created optional property, which has no value set.</returns>
        public static Optional<T> FromNoValue<T>()
            => Optional<T>.Default;
    }
}
