// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{

	/// <summary>
	/// Gets the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, bool withLocalizations = false)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var querydict = new Dictionary<string, string>
		{
			["with_localizations"] = withLocalizations.ToString().ToLower()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Bulk overwrites the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commands">The commands.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, IEnumerable<DiscordApplicationCommand> commands)
	{
		var pld = new List<RestApplicationCommandCreatePayload>();
		foreach (var command in commands)
		{
			pld.Add(new RestApplicationCommandCreatePayload
			{
				Type = command.Type,
				Name = command.Name,
				Description = command.Description,
				Options = command.Options,
				NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
				DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
				DefaultMemberPermission = command.DefaultMemberPermissions,
				DmPermission = command.DmPermission/*,
				Nsfw = command.IsNsfw*/
			});
		}
		this.Discord.Logger.LogDebug(DiscordJson.SerializeObject(pld));

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Creates a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="command">The command.</param>
	internal async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong applicationId, ulong guildId, DiscordApplicationCommand command)
	{
		var pld = new RestApplicationCommandCreatePayload
		{
			Type = command.Type,
			Name = command.Name,
			Description = command.Description,
			Options = command.Options,
			NameLocalizations = command.NameLocalizations.GetKeyValuePairs(),
			DescriptionLocalizations = command.DescriptionLocalizations.GetKeyValuePairs(),
			DefaultMemberPermission = command.DefaultMemberPermissions,
			DmPermission = command.DmPermission/*,
			Nsfw = command.IsNsfw*/
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Gets a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Edits a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="options">The options.</param>
	/// <param name="nameLocalization">The localizations of the name.</param>
	/// <param name="descriptionLocalization">The localizations of the description.</param>
	/// <param name="defaultMemberPermission">The default member permissions.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	internal async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId,
		Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options,
		Optional<DiscordApplicationCommandLocalization> nameLocalization, Optional<DiscordApplicationCommandLocalization> descriptionLocalization,
		Optional<Permissions> defaultMemberPermission, Optional<bool> dmPermission, Optional<bool> isNsfw)
	{
		var pld = new RestApplicationCommandEditPayload
		{
			Name = name,
			Description = description,
			Options = options,
			DefaultMemberPermission = defaultMemberPermission,
			DmPermission = dmPermission,
			NameLocalizations = nameLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault(),
			DescriptionLocalizations = descriptionLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault()/*,
			Nsfw = isNsfw*/
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task DeleteGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}


	/// <summary>
	/// Gets the guild application command permissions.
	/// </summary>
	/// <param name="applicationId">The target application id.</param>
	/// <param name="guildId">The target guild id.</param>
	internal async Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> GetGuildApplicationCommandPermissionsAsync(ulong applicationId, ulong guildId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}{Endpoints.PERMISSIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermission>>(res.Response);

		foreach (var app in ret)
			app.Discord = this.Discord;

		return ret.ToList();
	}

	/// <summary>
	/// Gets a guild application command permission.
	/// </summary>
	/// <param name="applicationId">The target application id.</param>
	/// <param name="guildId">The target guild id.</param>
	/// <param name="commandId">The target command id.</param>
	internal async Task<DiscordGuildApplicationCommandPermission> GetGuildApplicationCommandPermissionAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id{Endpoints.PERMISSIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermission>(res.Response);

		ret.Discord = this.Discord;

		return ret;
	}
}
