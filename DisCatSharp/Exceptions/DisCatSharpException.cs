using System;

namespace DisCatSharp.Exceptions;

public class DisCatSharpException : Exception
{
	internal DisCatSharpException(string message)
		: base(message)
	{ }
}
