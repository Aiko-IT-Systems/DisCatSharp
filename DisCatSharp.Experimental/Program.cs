using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Experimental;

public class Program
{
	public static void Main(string[] args = null)
	{
		var test = new Test("test");
		test.Invoke();
		var str = test.TestString;
	}
}

[Experimental("class")]
public class Test
{
	[Experimental("property")]
	public string TestString { get; set; }

	[Experimental("construct")]
	public Test([Experimental("test arg")] string test = null)
	{
		this.TestString = test;
	}

	[Experimental("construct 2")]
	public Test()
	{ }

	[Experimental("method")]
	public void Invoke()
		=> Console.WriteLine(this.TestString);
}
