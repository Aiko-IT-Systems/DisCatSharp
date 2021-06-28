using System;

namespace DSharpPlusNextGen.SlashCommands
{
    /// <summary>
    /// Marks this method as a slash command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SlashCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this command
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this command
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the default permission of this command
        /// </summary>
        public bool DefaultPermission { get; set; }

        /// <summary>
        /// Marks this method as a slash command
        /// </summary>
        /// <param name="name">The name of this slash command</param>
        /// <param name="description">The description of this slash command</param>
        /// <param name="default_permission">Whether everyone can execute this command.</param>
        public SlashCommandAttribute(string name, string description, bool default_permission = true)
        {
            this.Name = name.ToLower();
            this.Description = description;
            this.DefaultPermission = default_permission;
        }
    }
}
