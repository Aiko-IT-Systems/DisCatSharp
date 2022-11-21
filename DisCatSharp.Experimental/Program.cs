using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Experimental;

public class Program
{
	public static void Main(string[] args = null)
	{
		Test2();
	}

	[Experimental("Something")]
	public static string TestString { get; set; }

	[Experimental]
	public static void Test2()
	{
		TestString = "Test";
		Console.WriteLine(TestString);
	}
}
