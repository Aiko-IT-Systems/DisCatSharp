using System;

namespace DisCatSharp.Experimental;

public class Program
{
	public static void Main(string[] args = null)
	{
		Test2();
	}


	[Experimental("Test2")]
	public static void Test2()
	{
		Console.WriteLine("Test2");
	}
}
