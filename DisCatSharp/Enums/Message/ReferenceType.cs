using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the message reference type.
/// </summary>
public enum ReferenceType
{
	/// <summary>
	/// A standard reference used by replies.
	/// </summary>
	Default = 0,

	/// <summary>
	/// Reference used to point to a message at a point in time.
	/// </summary>
	Forward = 1
}
