// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a discord role, to which users can be assigned.
    /// </summary>
    public class DiscordRole : SnowflakeObject, IEquatable<DiscordRole>
    {
        /// <summary>
        /// Gets the name of this role.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the color of this role.
        /// </summary>
        [JsonIgnore]
        public DiscordColor Color
            => new(this._color);

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        internal int _color;

        /// <summary>
        /// Gets whether this role is hoisted.
        /// </summary>
        [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsHoisted { get; internal set; }

        /// <summary>
        /// Gets the position of this role in the role hierarchy.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; internal set; }

        /// <summary>
        /// Gets the permissions set for this role.
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Permissions { get; internal set; }

        /// <summary>
        /// Gets whether this role is managed by an integration.
        /// </summary>
        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsManaged { get; internal set; }

        /// <summary>
        /// Gets whether this role is mentionable.
        /// </summary>
        [JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsMentionable { get; internal set; }

        /// <summary>
        /// Gets the tags this role has.
        /// </summary>
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordRoleTags Tags { get; internal set; }

        /// <summary>
        /// Gets the role icon's hash.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconHash { get; internal set; }

        /// <summary>
        /// Gets the role icon's url.
        /// </summary>
        [JsonIgnore]
        public string IconUrl
            => !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.RoleIcons}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=64" : null;

        /// <summary>
        /// Gets the role unicode_emoji.
        /// </summary>
        [JsonProperty("unicode_emoji", NullValueHandling = NullValueHandling.Ignore)]
        internal string _unicodeEmojiString;

        /// <summary>
        /// Gets the unicode emoji.
        /// </summary>
        public DiscordEmoji UnicodeEmoji
            => this._unicodeEmojiString != null ? DiscordEmoji.FromName(this.Discord, $":{this._unicodeEmojiString}:", false) : null;

        [JsonIgnore]
        internal ulong _guildId = 0;

        /// <summary>
        /// Gets a mention string for this role. If the role is mentionable, this string will mention all the users that belong to this role.
        /// </summary>
        public string Mention
            => Formatter.Mention(this);

        #region Methods
        /// <summary>
        /// Modifies this role's position.
        /// </summary>
        /// <param name="Position">New position</param>
        /// <param name="Reason">Reason why we moved it</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task ModifyPosition(int Position, string Reason = null)
        {
            var roles = this.Discord.Guilds[this._guildId].Roles.Values.OrderByDescending(Xr => Xr.Position).ToArray();
            var pmds = new RestGuildRoleReorderPayload[roles.Length];
            for (var i = 0; i < roles.Length; i++)
            {
                pmds[i] = new RestGuildRoleReorderPayload { RoleId = roles[i].Id };

                pmds[i].Position = roles[i].Id == this.Id ? Position : roles[i].Position <= Position ? roles[i].Position - 1 : roles[i].Position;
            }

            return this.Discord.ApiClient.ModifyGuildRolePosition(this._guildId, pmds, Reason);
        }

        /// <summary>
        /// Updates this role.
        /// </summary>
        /// <param name="Name">New role name</param>
        /// <param name="Permissions">New role permissions</param>
        /// <param name="Color">New role color</param>
        /// <param name="Hoist">New role hoist</param>
        /// <param name="Mentionable">Whether this role is mentionable</param>
        /// <param name="Reason">Reason why we made this change</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Modify(string Name = null, Permissions? Permissions = null, DiscordColor? Color = null, bool? Hoist = null, bool? Mentionable = null, string Reason = null)
            => this.Discord.ApiClient.ModifyGuildRoleAsync(this._guildId, this.Id, Name, Permissions, Color?.Value, Hoist, Mentionable, null, null, Reason);

        /// <summary>
        /// Updates this role.
        /// </summary>
        /// <param name="Action">The action.</param>
        /// <exception cref = "Exceptions.UnauthorizedException" > Thrown when the client does not have the<see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Modify(Action<RoleEditModel> Action)
        {
            var mdl = new RoleEditModel();
            Action(mdl);

            var iconb64 = Optional.FromNoValue<string>();
            var emoji = Optional.FromNoValue<string>();
            var canContinue = true;
            if (mdl.Icon.HasValue || mdl.UnicodeEmoji.HasValue)
                canContinue = this.Discord.Guilds[this._guildId].Features.CanSetRoleIcons;

            if (mdl.Icon.HasValue && mdl.Icon.Value != null)
                using (var imgtool = new ImageTool(mdl.Icon.Value))
                    iconb64 = imgtool.GetBase64();
            else if (mdl.Icon.HasValue)
                iconb64 = null;

            if (mdl.UnicodeEmoji.HasValue && mdl.UnicodeEmoji.Value != null)
                emoji = mdl.UnicodeEmoji.Value.Id == 0 ? mdl.UnicodeEmoji.Value.Name : throw new ArgumentException("Emoji must be unicode");
            else if (mdl.UnicodeEmoji.HasValue)
                emoji = null;

            return canContinue ? this.Discord.ApiClient.ModifyGuildRoleAsync(this._guildId, this.Id, mdl.Name, mdl.Permissions, mdl.Color?.Value, mdl.Hoist, mdl.Mentionable, iconb64, emoji, mdl.AuditLogReason) : throw new NotSupportedException($"Cannot modify role icon. Guild needs boost tier two.");
        }

        /// <summary>
        /// Deletes this role.
        /// </summary>
        /// <param name="Reason">Reason as to why this role has been deleted.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Delete(string Reason = null) => this.Discord.ApiClient.DeleteRole(this._guildId, this.Id, Reason);
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRole"/> class.
        /// </summary>
        internal DiscordRole() { }

        /// <summary>
        /// Checks whether this role has specific permissions.
        /// </summary>
        /// <param name="Permission">Permissions to check for.</param>
        /// <returns>Whether the permissions are allowed or not.</returns>
        public PermissionLevel CheckPermission(Permissions Permission)
            => (this.Permissions & Permission) != 0 ? PermissionLevel.Allowed : PermissionLevel.Unset;

        /// <summary>
        /// Returns a string representation of this role.
        /// </summary>
        /// <returns>String representation of this role.</returns>
        public override string ToString() => $"Role {this.Id}; {this.Name}";

        /// <summary>
        /// Checks whether this <see cref="DiscordRole"/> is equal to another object.
        /// </summary>
        /// <param name="Obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordRole"/>.</returns>
        public override bool Equals(object Obj) => this.Equals(Obj as DiscordRole);

        /// <summary>
        /// Checks whether this <see cref="DiscordRole"/> is equal to another <see cref="DiscordRole"/>.
        /// </summary>
        /// <param name="E"><see cref="DiscordRole"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordRole"/> is equal to this <see cref="DiscordRole"/>.</returns>
        public bool Equals(DiscordRole E)
            => E switch
            {
                null => false,
                _ => ReferenceEquals(this, E) || this.Id == E.Id
            };

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordRole"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordRole"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordRole"/> objects are equal.
        /// </summary>
        /// <param name="E1">First role to compare.</param>
        /// <param name="E2">Second role to compare.</param>
        /// <returns>Whether the two roles are equal.</returns>
        public static bool operator ==(DiscordRole E1, DiscordRole E2)
            => E1 is null == E2 is null
            && ((E1 is null && E2 is null) || E1.Id == E2.Id);

        /// <summary>
        /// Gets whether the two <see cref="DiscordRole"/> objects are not equal.
        /// </summary>
        /// <param name="E1">First role to compare.</param>
        /// <param name="E2">Second role to compare.</param>
        /// <returns>Whether the two roles are not equal.</returns>
        public static bool operator !=(DiscordRole E1, DiscordRole E2)
            => !(E1 == E2);
    }
}
