using System;
using System.Linq;
using System.Reflection;

using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DisCatSharp.Entities;

/// <summary>
/// Helper methods for instantiating an <see cref="Optional{T}"/>.
/// </summary>
/// <remarks>
/// This class only serves to provide <see cref="Some{T}"/> and <see cref="None"/>
/// as utility that supports type inference.
/// </remarks>
public static class Optional
{
	/// <summary>
	/// Provided for easy creation of empty <see cref="Optional{T}"/>s.
	/// </summary>
	public static readonly None None = new();

	/// <summary>
	/// Creates a new <see cref="Optional{T}"/> with specified value and valid state.
	/// </summary>
	/// <param name="value">Value to populate the optional with.</param>
	/// <typeparam name="T">Type of the value.</typeparam>
	/// <returns>Created optional.</returns>
	public static Optional<T> Some<T>(T value)
		=> value;

	/// <summary>
	///
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <returns></returns>
	public static Optional<T> FromNullable<T>(T value)
		=> value == null
			? None
			: value;

	/// <summary>
	/// Creates a new <see cref="Optional{T}"/> with specified value and valid state.
	/// </summary>
	/// <param name="value">Value to populate the optional with.</param>
	/// <typeparam name="T">Type of the value.</typeparam>
	/// <returns>Created optional.</returns>
	[Obsolete("Renamed to Some.")]
	public static Optional<T> FromValue<T>(T value)
		=> value;

	/// <summary>
	/// Creates a new empty <see cref="Optional{T}"/> with no value and invalid state.
	/// </summary>
	/// <typeparam name="T">The type that the created instance is wrapping around.</typeparam>
	/// <returns>Created optional.</returns>
	[Obsolete("Use None.")]
	public static Optional<T> FromNoValue<T>()
		=> default;
}

/// <summary>
/// Unit type for creating an empty <see cref="Optional{T}"/>s.
/// </summary>
public struct None
{ }

/// <summary>
/// Used internally to make serialization more convenient, do NOT change this, do NOT implement this yourself.
/// </summary>
public interface IOptional
{
	/// <summary>
	/// Gets a whether it has a value.
	/// </summary>
	bool HasValue { get; }

	/// <summary>
	/// Gets the raw value.
	/// </summary>
	/// <remarks>
	/// Must NOT throw InvalidOperationException.
	/// </remarks>
	object RawValue { get; }
}

/// <summary>
/// Represents a wrapper which may or may not have a value.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
[JsonConverter(typeof(OptionalJsonConverter))]
public readonly struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>, IOptional
{
	/// <summary>
	/// Static empty <see cref="Optional"/>.
	/// </summary>
	public static readonly Optional<T> None = default;

	/// <summary>
	/// Gets whether this <see cref="Optional{T}"/> has a value.
	/// </summary>
	public bool HasValue { get; }

	/// <summary>
	/// Gets the value of this <see cref="Optional{T}"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">If this <see cref="Optional{T}"/> has no value.</exception>
	public T Value => this.HasValue ? this._val : throw new InvalidOperationException("Value is not set.");

	/// <summary>
	/// Gets the raw value.
	/// </summary>
	object IOptional.RawValue => this._val;

	private readonly T _val;

	/// <summary>
	/// Creates a new <see cref="Optional{T}"/> with specified value.
	/// </summary>
	/// <param name="value">Value of this option.</param>
	[Obsolete("Use Optional.Some")]
	public Optional(T value)
	{
		this._val = value;
		this.HasValue = true;
	}

	[Obsolete("Renamed to Map")]
	public Optional<TOut> IfPresent<TOut>(Func<T, TOut> mapper)
		=> this.Map(mapper);

	/// <summary>
	/// Performs a mapping operation on the current <see cref="Optional{T}"/>, turning it into an Optional holding a
	/// <typeparamref name="TOut"/> instance if the source optional contains a value; otherwise, returns an
	/// <see cref="Optional{T}"/> of that same type with no value.
	/// </summary>
	/// <param name="mapper">The mapping function to apply on the current value if it exists</param>
	/// <typeparam name="TOut">The type of the target value returned by <paramref name="mapper"/></typeparam>
	/// <returns>
	/// An <see cref="Optional{T}"/> containing a value denoted by calling <paramref name="mapper"/> if the current
	/// <see cref="Optional{T}"/> contains a value; otherwise, an empty <see cref="Optional{T}"/> of the target
	/// type.
	/// </returns>
	public Optional<TOut> Map<TOut>(Func<T, TOut> mapper)
		=> this.HasValue
			? mapper(this._val)
			: Optional.None;

	/// <summary>
	/// Maps to <see cref="None"/> for <see cref="None"/>, to <code>default</code> for <code>null</code> and to the mapped value otherwise./>
	/// </summary>
	/// <typeparam name="TOut">The type to map to.</typeparam>
	/// <param name="mapper">The function that does the mapping of the non-null <typeparamref name="T"/>.</param>
	/// <returns>The mapped value.</returns>
	public Optional<TOut> MapOrNull<TOut>(Func<T, TOut> mapper)
		=> this.HasValue
			? this._val == null
				? default
				: mapper(this._val)
			: Optional.None;

	/// <summary>
	/// Gets the value of the <see cref="Optional{T}"/> or a specified value, if the <see cref="Optional{T}"/> has no value.
	/// </summary>
	/// <param name="other">The value to return if this has no value.</param>
	/// <returns>Either the value of the <see cref="Optional{T}"/> if present or the provided value.</returns>
	public T ValueOr(T other)
		=> this.HasValue
			? this._val
			: other;

	/// <summary>
	/// Gets the value of the <see cref="Optional{T}"/> or the default value for <typeparamref name="T"/>, if the
	/// <see cref="Optional{T}"/> has no value.
	/// </summary>
	/// <returns>Either the value of the <see cref="Optional{T}"/> if present or the type's default value.</returns>
	public T ValueOrDefault()
		=> this.ValueOr(default);

	/// <summary>
	/// Gets the <see cref="Optional"/>'s value, or throws the provided exception if it's empty.
	/// </summary>
	/// <param name="err">The exception to throw if the optional is empty.</param>
	/// <returns>The value of the <see cref="Optional"/>, if present.</returns>
	public T Expect(Exception err)
		=> !this.HasValue ? throw err : this._val;

	/// <summary>
	/// Gets the <see cref="Optional"/>'s value, or throws a standard exception with the provided string if it's
	/// empty.
	/// </summary>
	/// <param name="str">The string provided to the exception.</param>
	/// <returns>The value of the <see cref="Optional"/>, if present.</returns>
	public T Expect(string str) => this.Expect(new InvalidOperationException(str));

	/// <summary>
	/// Checks if this has a value and tests the predicate if it does.
	/// </summary>
	/// <param name="predicate">The predicate to test if this has a value.</param>
	/// <returns>True if this has a value and the predicate is fulfilled, false otherwise.</returns>
	public bool HasValueAnd(Predicate<T> predicate)
		=> this.HasValue && predicate(this._val);

	/// <summary>
	/// Returns a string representation of this optional value.
	/// </summary>
	/// <returns>String representation of this optional value.</returns>
	public override string ToString() => $"Optional<{typeof(T)}> ({this.Map(x => x.ToString()).ValueOr("<no value>")})";

	/// <summary>
	/// Checks whether this <see cref="Optional{T}"/> (or its value) are equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="Optional{T}"/> or its value.</returns>
	public override bool Equals(object obj) =>
		obj switch
		{
			T t => this.Equals(t),
			Optional<T> opt => this.Equals(opt),
			_ => false,
		};

	/// <summary>
	/// Checks whether this <see cref="Optional{T}"/> is equal to another <see cref="Optional{T}"/>.
	/// </summary>
	/// <param name="e"><see cref="Optional{T}"/> to compare to.</param>
	/// <returns>Whether the <see cref="Optional{T}"/> is equal to this <see cref="Optional{T}"/>.</returns>
	public bool Equals(Optional<T> e) => (!this.HasValue && !e.HasValue) || (this.HasValue == e.HasValue && this.Value.Equals(e.Value));

	/// <summary>
	/// Checks whether the value of this <see cref="Optional{T}"/> is equal to specified object.
	/// </summary>
	/// <param name="e">Object to compare to.</param>
	/// <returns>Whether the object is equal to the value of this <see cref="Optional{T}"/>.</returns>
	public bool Equals(T e)
		=> this.HasValue && ReferenceEquals(this.Value, e);

	/// <summary>
	/// Gets the hash code for this <see cref="Optional{T}"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="Optional{T}"/>.</returns>
	public override int GetHashCode()
		=> this.Map(x => x.GetHashCode()).ValueOrDefault();

	public static implicit operator Optional<T>(T val)
#pragma warning disable 0618
		=> new(val);
#pragma warning restore 0618

	public static explicit operator T(Optional<T> opt)
		=> opt.Value;

	/// <summary>
	/// Creates an empty optional.
	/// </summary>
	public static implicit operator Optional<T>(None _) => default;

	public static bool operator ==(Optional<T> opt1, Optional<T> opt2)
		=> opt1.Equals(opt2);

	public static bool operator !=(Optional<T> opt1, Optional<T> opt2)
		=> !opt1.Equals(opt2);

	public static bool operator ==(Optional<T> opt, T t)
		=> opt.Equals(t);

	public static bool operator !=(Optional<T> opt, T t)
		=> !opt.Equals(t);
}

/// <summary>
/// Represents an optional json contract resolver.
/// <seealso cref="DiscordJson.s_serializer"/>
/// </summary>
internal sealed class OptionalJsonContractResolver : DefaultContractResolver
{
	/// <summary>
	/// Creates the property.
	/// </summary>
	/// <param name="member">The member.</param>
	/// <param name="memberSerialization">The member serialization.</param>
	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		var property = base.CreateProperty(member, memberSerialization);

		var type = property.PropertyType;

		if (!type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IOptional)))
			return property;

		// we cache the PropertyInfo object here (it's captured in closure). we don't have direct
		// access to the property value so we have to reflect into it from the parent instance
		// we use UnderlyingName instead of PropertyName in case the C# name is different from the Json name.
		var declaringMember = property.DeclaringType.GetTypeInfo().DeclaredMembers
			.FirstOrDefault(e => e.Name == property.UnderlyingName);

		switch (declaringMember)
		{
			case PropertyInfo declaringProp:
				property.ShouldSerialize = instance => // instance here is the declaring (parent) type
				{
					var optionalValue = declaringProp.GetValue(instance);
					return (optionalValue as IOptional).HasValue;
				};
				return property;
			case FieldInfo declaringField:
				property.ShouldSerialize = instance => // instance here is the declaring (parent) type
				{
					var optionalValue = declaringField.GetValue(instance);
					return (optionalValue as IOptional).HasValue;
				};
				return property;
			default:
				throw new InvalidOperationException(
					"Can only serialize Optional<T> members that are fields or properties");
		}
	}
}

/// <summary>
/// Represents an optional json converter.
/// </summary>
internal sealed class OptionalJsonConverter : JsonConverter
{
	/// <summary>
	/// Writes the json.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The serializer.</param>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		// we don't check for HasValue here since it's checked in OptionalJsonContractResolver
		var val = (value as IOptional).RawValue;
		// JToken.FromObject will throw if `null` so we manually write a null value.
		if (val == null)
		{
			// you can read serializer.NullValueHandling here, but unfortunately you can **not** skip serialization
			// here, or else you will get a nasty JsonWriterException, so we just ignore its value and manually
			// write the null.
			writer.WriteToken(JsonToken.Null);
		}
		else
		{
			// convert the value to a JSON object and write it to the property value.
			JToken.FromObject(val).WriteTo(writer);
		}
	}

	/// <summary>
	/// Reads the json.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="existingValue">The existing value.</param>
	/// <param name="serializer">The serializer.</param>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
		JsonSerializer serializer)
	{
		var genericType = objectType.GenericTypeArguments[0];

		var constructor = objectType.GetTypeInfo().DeclaredConstructors
			.FirstOrDefault(e => e.GetParameters()[0].ParameterType == genericType);

		return constructor.Invoke(new[] { serializer.Deserialize(reader, genericType) });
	}

	/// <summary>
	/// Whether it can convert.
	/// </summary>
	/// <param name="objectType">The object type.</param>
	public override bool CanConvert(Type objectType) => objectType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IOptional));
}
