// This file is a part of DSharpPlusNextGen.Common project.
// 
// Copyright 2020 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;

namespace DSharpPlusNextGen.Common
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

        private string DebuggerDisplay
            => this.HasValue ? $"Optional<{typeof(T)}> <has value> {this._value?.ToString() ?? "<null>"}" : $"Optional<{typeof(T)}> <has no value>";

        /// <summary>
        /// Creates a new <see cref="Optional{T}"/> property with specified value.
        /// </summary>
        /// <param name="value">Value of this property.</param>
        public Optional(T value)
        {
            this.HasValue = true;
            this._value = value;
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
        /// <param name="obj">Object to compare against.</param>
        /// <returns>Whether the supplied object is equal to the value of this property.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Optional<T> opt)
                return this.Equals(opt);

            if (obj is T val)
                return this.Equals(val);

            if (!this.HasValue && obj == null)
                return true;

            if (this.HasValue)
                return object.Equals(this._value, obj);

            return false;
        }

        /// <summary>
        /// Checks whether this property is equal to another property.
        /// </summary>
        /// <param name="other">Property to compare against.</param>
        /// <returns>Whether the supplied property is equal to this property.</returns>
        public bool Equals(Optional<T> other)
        {
            if (!this.HasValue && !other.HasValue)
                return true;
            else if (this.HasValue != other.HasValue)
                return false;
            else
                return object.Equals(this._value, other._value);
        }

        /// <summary>
        /// Checks whether this proerty's value is equal to another value.
        /// </summary>
        /// <param name="other">Value to compare this property's value against.</param>
        /// <returns>Whether the supplied value is equal to the value of this property.</returns>
        public bool Equals(T other)
        {
            if (!this.HasValue && other == null)
                return true;

            if (this.HasValue)
                return object.Equals(this._value, other);

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
        /// <param name="value">Value to convert into an optional property.</param>
        public static implicit operator Optional<T>(T value)
            => new Optional<T>(value);

        /// <summary>
        /// Compares two properties and returns whether they are equal.
        /// </summary>
        /// <param name="left">Property to compare against.</param>
        /// <param name="right">Property to compare.</param>
        /// <returns>Whether the two properties are equal.</returns>
        public static bool operator ==(Optional<T> left, Optional<T> right)
            => left.Equals(right);

        /// <summary>
        /// Compares two properties and returns whether they are not equal.
        /// </summary>
        /// <param name="left">Property to compare against.</param>
        /// <param name="right">Property to compare.</param>
        /// <returns>Whether the two properties are not equal.</returns>
        public static bool operator !=(Optional<T> left, Optional<T> right)
            => !left.Equals(right);

        /// <summary>
        /// Compares a property's value against another value, and returns whether they are equal.
        /// </summary>
        /// <param name="left">Property to compare against.</param>
        /// <param name="right">Value to compare.</param>
        /// <returns>Whether the property's value is equal to the specified value.</returns>
        public static bool operator ==(Optional<T> left, T right)
            => left.Equals(right);

        /// <summary>
        /// Compares a property's value against another value, and returns whether they are not equal.
        /// </summary>
        /// <param name="left">Property to compare against.</param>
        /// <param name="right">Value to compare.</param>
        /// <returns>Whether this property's value is not equal to the specified value.</returns>
        public static bool operator !=(Optional<T> left, T right)
            => !left.Equals(right);

        /// <summary>
        /// Checks whether specified property has a value.
        /// </summary>
        /// <param name="opt">Property to check.</param>
        /// <returns>Whether the property has a value.</returns>
        public static bool operator true(Optional<T> opt)
            => opt.HasValue;

        /// <summary>
        /// Checks whether specified property has no value.
        /// </summary>
        /// <param name="opt">Property to check.</param>
        /// <returns>Whether the property has no value.</returns>
        public static bool operator false(Optional<T> opt)
            => !opt.HasValue;
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
        /// <param name="value">Value to set the property to.</param>
        /// <returns>Created optional property, which has a specified value set.</returns>
        public static Optional<T> FromValue<T>(T value)
            => new Optional<T>(value);

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
