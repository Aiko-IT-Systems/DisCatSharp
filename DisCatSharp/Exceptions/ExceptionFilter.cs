using System;

using Sentry.Extensibility;

namespace DisCatSharp.Exceptions;

public class DisCatSharpExceptionFilter : IExceptionFilter
{
	internal DiscordConfiguration Config { get; set; }

	public bool Filter(Exception ex)
		=> !this.Config.TrackExceptions.Contains(ex.GetType());

	internal DisCatSharpExceptionFilter(DiscordConfiguration configuration)
	{
		this.Config = configuration;
	}
}
