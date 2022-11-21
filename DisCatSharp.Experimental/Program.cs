using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Experimental;

public class Program
{
	public static void Main(string[] args = null)
	{
		var test = new Test("test");
		test.Invoke();
	}
}

[Experimental("class")]
public class Test
{

	[Experimental("property")]
	public string TestString { get; set; }

	[Experimental("construct")]
	public Test(string test = null)
	{
		this.TestString = test;
	}

	[Experimental("method")]
	public void Invoke()
		=> Console.WriteLine(this.TestString);
}
