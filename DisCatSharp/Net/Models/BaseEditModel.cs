namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents the base edit model.
/// </summary>
public class BaseEditModel
{
	/// <summary>
	/// Reason given in audit logs
	/// </summary>
	public string AuditLogReason { internal get; set; }
}
