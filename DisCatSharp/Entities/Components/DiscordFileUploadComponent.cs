using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     A file upload component.
/// </summary>
public sealed class DiscordFileUploadComponent : DiscordComponent, ILabelComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordFileUploadComponent" />.
	/// </summary>
	internal DiscordFileUploadComponent()
	{
		this.Type = ComponentType.FileUpload;
	}

	/// <summary>
	///     Constructs a new <see cref="DiscordFileUploadComponent" />.
	/// </summary>
	/// <param name="customId">The Id to assign to the file upload component.</param>
	/// <param name="minOptions">Minimum count of files allowed to upload.</param>
	/// <param name="maxOptions">Maximum count of files allowed to upload.</param>
	/// <param name="required">Whether the file upload is required.</param>
	public DiscordFileUploadComponent(string customId = null, int minOptions = 1, int maxOptions = 1, bool required = true)
		: this()
	{
		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.MinimumValues = minOptions;
		this.MaximumValues = maxOptions;
		this.Required = required;
	}

	/// <summary>
	///     The custom Id of this file upload component. This is sent back when files are uploaded.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public override string? CustomId { get; internal set; } = Guid.NewGuid().ToString();

	/// <summary>
	///     Whether this file upload component is required. For modals.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Required { get; internal set; }

	/// <summary>
	///     The minimum amount of files that can be uploaded. Must be less than or equal to
	///     <see cref="MaximumValues" />. Defaults to <c>1</c>.
	/// </summary>
	[JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinimumValues { get; internal set; } = 1;

	/// <summary>
	///     The maximum amount of files that can be uploaded. Must be greater than or equal to <c>1</c> or
	///     <see cref="MinimumValues" />. Defaults to <c>1</c>.
	/// </summary>
	[JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaximumValues { get; internal set; } = 1;

	/// <summary>
	///     Whether this component can be used.
	/// </summary>
	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; internal set; }

	/// <summary>
	///     Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordFileUploadComponent Enable()
		=> this.SetState(false);

	/// <summary>
	///     Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordFileUploadComponent Disable()
		=> this.SetState(true);

	/// <summary>
	///     Enables or disables this component.
	/// </summary>
	/// <param name="disabled">Whether this component should be disabled.</param>
	/// <returns>The current component.</returns>
	public DiscordFileUploadComponent SetState(bool disabled)
	{
		this.Disabled = disabled;
		return this;
	}

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordFileUploadComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}

	/// <summary>
	///     The submitted values of this file upload component. The values are attachment ids.
	///     This is only available in <see cref="DiscordInteractionData" />.
	/// </summary>
	[JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
	public string[]? Values { get; internal set; }
}
