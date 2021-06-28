using System;

namespace DSharpPlusNextGen.SlashCommands
{
    /// <summary>
    /// Marks this class a slash command group
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SlashCommandGroupAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this slash command group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the description of this slash command group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the default permission of this slash command group
        /// </summary>
        public bool DefaultPermission { get; set; }

        /// <summary>
        /// Marks this class as a slash command group
        /// </summary>
        /// <param name="name">The name of this slash command group</param>
        /// <param name="description">The description of this slash command group</param>
        /// <param name="default_permission">Whether everyone can execute this command.</param>
        public SlashCommandGroupAttribute(string name, string description, bool default_permission = true)
        {
            this.Name = name.ToLower();
            this.Description = description;
            this.DefaultPermission = default_permission;
        }
    }
}
