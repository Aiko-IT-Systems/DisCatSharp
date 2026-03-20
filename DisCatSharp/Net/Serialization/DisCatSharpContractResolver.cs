using System;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DisCatSharp.Net.Serialization;

/// <summary>
///    Contract resolver that combines Optional and ShouldSerializeXy() support.
/// </summary>
public sealed class DisCatSharpContractResolver : DefaultContractResolver
{
	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		var property = base.CreateProperty(member, memberSerialization);
		var type = property.PropertyType;
		var declaringType = property.DeclaringType;

		// ShouldSerializeXy() support
		if (declaringType != null)
		{
			var shouldSerializeMethod = declaringType.GetMethod($"ShouldSerialize{property.PropertyName}", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (shouldSerializeMethod != null && shouldSerializeMethod.ReturnType == typeof(bool) && shouldSerializeMethod.GetParameters().Length == 0)
			{
				property.ShouldSerialize = instance =>
				{
					var result = shouldSerializeMethod.Invoke(instance, null);
					return result is bool b && b;
				};
				property.NullValueHandling = NullValueHandling.Include;
			}
		}

		// Optional<T> support
		if (type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(DisCatSharp.Entities.IOptional)))
		{
			var declaringMember = property.DeclaringType.GetTypeInfo().DeclaredMembers
				.FirstOrDefault(e => e.Name == property.UnderlyingName);
			property.ShouldSerialize = declaringMember switch
			{
				PropertyInfo declaringProp => instance =>
									{
										var optionalValue = declaringProp.GetValue(instance);
										return (optionalValue as DisCatSharp.Entities.IOptional).HasValue;
									}

				,
				FieldInfo declaringField => instance =>
					{
						var optionalValue = declaringField.GetValue(instance);
						return (optionalValue as DisCatSharp.Entities.IOptional).HasValue;
					}

				,
				_ => throw new InvalidOperationException("Can only serialize Optional<T> members that are fields or properties"),
			};
		}

		return property;
	}
}
