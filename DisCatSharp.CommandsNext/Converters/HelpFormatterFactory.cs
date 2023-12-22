using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents the help formatter factory.
/// </summary>
internal class HelpFormatterFactory
{
	/// <summary>
	/// Gets or sets the factory.
	/// </summary>
	private ObjectFactory _factory;

	/// <summary>
	/// Initializes a new instance of the <see cref="HelpFormatterFactory"/> class.
	/// </summary>
	public HelpFormatterFactory()
	{ }

	/// <summary>
	/// Sets the formatter type.
	/// </summary>
	public void SetFormatterType<T>() where T : BaseHelpFormatter => this._factory = ActivatorUtilities.CreateFactory(typeof(T), [typeof(CommandContext)]);

	/// <summary>
	/// Creates the help formatter.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	public BaseHelpFormatter Create(CommandContext ctx) => this._factory(ctx.Services, [ctx]) as BaseHelpFormatter;
}
