using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Experimental;

internal class Program
{
	internal static void Main(string[] args = null)
	{
		var test = new Test("test");
		test.Invoke();
		var str = test.TestString;
	}
}

[Experimental("class")]
internal class Test
{
	[Experimental("property")]
	internal string TestString { get; set; }

	[Experimental("construct")]
	internal Test([Experimental("test arg")] string test = null)
	{
		this.TestString = test;
	}

	[Experimental("construct 2")]
	internal Test()
	{ }

	[Experimental("method")]
	internal void Invoke()
		=> Console.WriteLine(this.TestString);
}
