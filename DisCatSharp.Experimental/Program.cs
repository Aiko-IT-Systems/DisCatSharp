using System;

namespace DisCatSharp.Experimental;

public class Program
{
	public static void Main(string[] args = null)
	{
		Test();
	}


	[Experimental("Test")]
	public static void Test()
	{
		Console.WriteLine("Test");
	}
}
