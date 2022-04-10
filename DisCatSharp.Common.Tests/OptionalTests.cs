using System.Numerics;

using DisCatSharp.Entities;

using Xunit;

namespace DisCatSharp.Common.Tests
{
	public class Tests
	{
		class TestClass { }

		struct TestStruct { }

		[Fact]
		public void EqualityTests()
		{
			var c = new TestClass();

			var c1 = Optional<TestClass>.None;
			var c2 = Optional.Some(c);
			Assert.NotEqual(c1, c2);

			var c3 = Optional.Some(c);
			Assert.Equal(c2, c3);

			var c4 = Optional.Some(new TestClass());
			Assert.NotEqual(c3, c4);

			var s1 = Optional<TestStruct>.None;
			var s2 = Optional.Some(new TestStruct());
			Assert.NotEqual(s1, s2);

			var s3 = Optional.Some(new TestStruct());
			Assert.Equal(s2, s3);

			var s4 = (Optional<TestStruct>)Optional.None;
			Assert.Equal(s1, s4);
		}

		[Fact]
		public void FromNullableTests()
		{
			var c1 = Optional.Some<TestClass>(null);
			Assert.True(c1.HasValue);
			Assert.Null(c1.Value);

			var c2 = Optional.FromNullable<TestClass>(null);
			Assert.False(c2.HasValue);

			var c3 = Optional.FromNullable(new TestClass());
			Assert.True(c3.HasValue);
			Assert.NotNull(c3.Value);
		}

		[Fact]
		public void MapTests()
		{
			var c1 = Optional<TestClass>.None;
			var s1 = c1.Map(x => new TestStruct());
			Assert.False(s1.HasValue);

			c1 = Optional.Some(new TestClass());
			s1 = c1.Map(x => new TestStruct());
			Assert.True(s1.HasValue);

			var mapped = false;
			T Map<T>(T o)
			{
				mapped = true;
				return o;
			}

			c1 = null;
			var c2 = c1.MapOrNull(Map);
			Assert.False(mapped);
			Assert.True(c2.HasValue);
			Assert.Null((TestClass)c2);

			c1 = new TestClass();
			c2 = c1.MapOrNull(Map);
			Assert.True(mapped);
			Assert.True(c2.HasValue);
			Assert.Equal(c1, c2);
			Assert.Equal(c1.Value, c2.Value);
		}
	}
}
