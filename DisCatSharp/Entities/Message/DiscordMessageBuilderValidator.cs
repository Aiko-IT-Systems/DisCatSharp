using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;

namespace DisCatSharp.Entities;

public class DiscordMessageBuilderValidator
{
	public void Validate(DiscordMessageBuilder builder)
	{
		List<ValidationResult> validationErrors = [];

		this.ValidateContent(builder.Content, validationErrors);
		this.ValidateEmbeds(builder.EmbedsInternal, validationErrors);
		this.ValidateAllowedMentions(builder.Mentions, validationErrors);
		this.ValidateComponents(builder.ComponentsInternal, validationErrors);
		this.ValidateFiles(builder.FilesInternal, validationErrors);

		if (validationErrors.Count > 0)
			throw new ValidationException(typeof(DiscordMessageBuilder), nameof(DiscordMessageBuilder), validationErrors);
	}

	private void ValidateContent(string? content, List<ValidationResult> validationErrors)
	{
		if (!string.IsNullOrEmpty(content) && content.Length > 2000)
			validationErrors.Add(new(null, "Content", "Message content exceeds the 2000 character limit."));
	}

	private void ValidateEmbeds(List<DiscordEmbed> embeds, List<ValidationResult> validationErrors)
	{
		if (embeds == null) return;

		if (embeds.Count > 10)
			validationErrors.Add(new(null, "Embeds", "A message can contain up to 10 embeds."));

		var totalCharacters = 0;
		var embedIndex = 0;
		foreach (var embed in embeds)
		{
			if (embed.Title?.Length > 256)
				validationErrors.Add(new(embedIndex, "Embed.Title", "Embed title exceeds the 256 character limit."));

			if (embed.Description?.Length > 4096)
				validationErrors.Add(new(embedIndex, "Embed.Description", "Embed description exceeds the 4096 character limit."));

			if (embed.Footer?.Text?.Length > 2048)
				validationErrors.Add(new(embedIndex, "Embed.Footer.Text", "Embed footer text exceeds the 2048 character limit."));

			if (embed.Author?.Name?.Length > 256)
				validationErrors.Add(new(embedIndex, "Embed.Author.Name", "Embed author name exceeds the 256 character limit."));

			if (embed.Fields?.Count > 25)
				validationErrors.Add(new(embedIndex, "Embed.Fields", "Embed contains more than 25 fields."));

			if (embed.Fields is not null)
			{
				var fieldIndex = 0;
				foreach (var field in embed.Fields)
				{
					if (field.Name?.Length > 256)
						validationErrors.Add(new(fieldIndex, "Embed.Field.Name", "Embed field name exceeds the 256 character limit."));
					if (field.Value?.Length > 1024)
						validationErrors.Add(new(fieldIndex, "Embed.Field.Value", "Embed field value exceeds the 1024 character limit."));
					fieldIndex++;
				}
			}

			totalCharacters += (embed.Title?.Length ?? 0) + (embed.Description?.Length ?? 0)
			                                              + (embed.Footer?.Text?.Length ?? 0) + (embed.Author?.Name?.Length ?? 0)
			                                              + embed.Fields?.Sum(f => (f.Name?.Length ?? 0) + (f.Value?.Length ?? 0)) ?? 0;

			embedIndex++;
		}

		if (totalCharacters > 6000)
			validationErrors.Add(new(null, "Embeds.TotalCharacters", "Total characters across all embeds exceed the 6000 character limit."));
	}

	private void ValidateAllowedMentions(List<IMention> allowedMentions, List<ValidationResult> validationErrors)
	{
		if (allowedMentions == null) return;

		var userMentions = allowedMentions.OfType<UserMention>().Count();
		var roleMentions = allowedMentions.OfType<RoleMention>().Count();

		if (userMentions > 100)
			validationErrors.Add(new(null, "AllowedMentions.Users", "Allowed mentions exceed the 100 users limit."));

		if (roleMentions > 100)
			validationErrors.Add(new(null, "AllowedMentions.Roles", "Allowed mentions exceed the 100 roles limit."));
	}

	private void ValidateComponents(List<DiscordActionRowComponent> components, List<ValidationResult> validationErrors)
	{
		if (components == null) return;

		if (components.Count > 5)
			validationErrors.Add(new(null, "Components", "A message can only contain up to 5 action rows."));

		var actionRowIndex = 0;
		foreach (var actionRow in components)
		{
			if (actionRow.Components.Count > 5)
				validationErrors.Add(new(actionRowIndex, "ActionRow.Components", "An action row can only contain up to 5 components."));

			var hasButton = false;
			var hasSelectMenu = false;

			foreach (var component in actionRow.Components)
			{
				if (component is DiscordButtonComponent button)
				{
					hasButton = true;
					this.ValidateButton(button, validationErrors, actionRowIndex);
				}
				else if (component is DiscordLinkButtonComponent linkButton)
				{
					hasButton = true;
					this.ValidateLinkButton(linkButton, validationErrors, actionRowIndex);
				}
				else if (component is DiscordBaseSelectComponent selectMenu)
				{
					hasSelectMenu = true;
					this.ValidateSelectMenu(selectMenu, validationErrors, actionRowIndex);
				}

				if (hasButton && hasSelectMenu)
					validationErrors.Add(new(actionRowIndex, "ActionRow", "An action row cannot contain both buttons and select menus."));
			}

			actionRowIndex++;
		}
	}

	private void ValidateButton(DiscordButtonComponent button, List<ValidationResult> validationErrors, int actionRowIndex)
	{
		if (button.Style != ButtonStyle.Link && string.IsNullOrEmpty(button.CustomId))
			validationErrors.Add(new(actionRowIndex, "Button.CustomId", "Non-link buttons must have a custom_id."));

		if (button.Label?.Length > 80)
			validationErrors.Add(new(actionRowIndex, "Button.Label", "Button label exceeds the 80 character limit."));
	}

	private void ValidateLinkButton(DiscordLinkButtonComponent button, List<ValidationResult> validationErrors, int actionRowIndex)
	{
		if (button.Style == ButtonStyle.Link && string.IsNullOrEmpty(button.Url))
			validationErrors.Add(new(actionRowIndex, "Button.Url", "Link buttons must have a URL."));

		if (button.Label?.Length > 80)
			validationErrors.Add(new(actionRowIndex, "Button.Label", "Button label exceeds the 80 character limit."));
	}

	private void ValidateSelectMenu(DiscordBaseSelectComponent selectMenu, List<ValidationResult> validationErrors, int actionRowIndex)
	{
		if (selectMenu.CustomId?.Length > 100)
			validationErrors.Add(new(actionRowIndex, "SelectMenu.CustomId", "Select menu custom_id exceeds the 100 character limit."));

		if (selectMenu is DiscordStringSelectComponent stringSelectComponent)
			this.ValidateSelectMenuOptions(stringSelectComponent, validationErrors, actionRowIndex);
	}

	private void ValidateSelectMenuOptions(DiscordStringSelectComponent stringSelectMenu, List<ValidationResult> validationErrors, int actionRowIndex)
	{
		if (stringSelectMenu.Options.Count > 25)
			validationErrors.Add(new(actionRowIndex, "SelectMenu.Options", "Select menu options exceed the 25 options limit."));
	}

	private void ValidateFiles(List<DiscordMessageFile> files, List<ValidationResult> validationErrors)
	{
		if (files.Count > 10)
			validationErrors.Add(new(null, "Files", "A message can only contain up to 10 files."));
	}
}
