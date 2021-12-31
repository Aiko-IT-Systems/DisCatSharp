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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.CommandsNext.Entities;
using DisCatSharp.CommandsNext.Exceptions;

namespace DisCatSharp.CommandsNext.Builders
{
    /// <summary>
    /// Represents an interface to build a command.
    /// </summary>
    public class CommandBuilder
    {
        /// <summary>
        /// Gets the name set for this command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the aliases set for this command.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; }
        /// <summary>
        /// Gets the alias list.
        /// </summary>
        private List<string> AliasList { get; }

        /// <summary>
        /// Gets the description set for this command.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets whether this command will be hidden or not.
        /// </summary>
        public bool IsHidden { get; private set; }

        /// <summary>
        /// Gets the execution checks defined for this command.
        /// </summary>
        public IReadOnlyList<CheckBaseAttribute> ExecutionChecks { get; }
        /// <summary>
        /// Gets the execution check list.
        /// </summary>
        private List<CheckBaseAttribute> ExecutionCheckList { get; }

        /// <summary>
        /// Gets the collection of this command's overloads.
        /// </summary>
        public IReadOnlyList<CommandOverloadBuilder> Overloads { get; }
        /// <summary>
        /// Gets the overload list.
        /// </summary>
        private List<CommandOverloadBuilder> OverloadList { get; }
        /// <summary>
        /// Gets the overload argument sets.
        /// </summary>
        private HashSet<string> OverloadArgumentSets { get; }

        /// <summary>
        /// Gets the module on which this command is to be defined.
        /// </summary>
        public ICommandModule Module { get; }

        /// <summary>
        /// Gets custom attributes defined on this command.
        /// </summary>
        public IReadOnlyList<Attribute> CustomAttributes { get; }
        /// <summary>
        /// Gets the custom attribute list.
        /// </summary>
        private List<Attribute> CustomAttributeList { get; }

        /// <summary>
        /// Creates a new module-less command builder.
        /// </summary>
        public CommandBuilder()
            : this(null)
        { }

        /// <summary>
        /// Creates a new command builder.
        /// </summary>
        /// <param name="Module">Module on which this command is to be defined.</param>
        public CommandBuilder(ICommandModule Module)
        {
            this.AliasList = new List<string>();
            this.Aliases = new ReadOnlyCollection<string>(this.AliasList);

            this.ExecutionCheckList = new List<CheckBaseAttribute>();
            this.ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(this.ExecutionCheckList);

            this.OverloadArgumentSets = new HashSet<string>();
            this.OverloadList = new List<CommandOverloadBuilder>();
            this.Overloads = new ReadOnlyCollection<CommandOverloadBuilder>(this.OverloadList);

            this.Module = Module;

            this.CustomAttributeList = new List<Attribute>();
            this.CustomAttributes = new ReadOnlyCollection<Attribute>(this.CustomAttributeList);
        }

        /// <summary>
        /// Sets the name for this command.
        /// </summary>
        /// <param name="Name">Name for this command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithName(string Name)
        {
            if (Name == null || Name.ToCharArray().Any(Xc => char.IsWhiteSpace(Xc)))
                throw new ArgumentException("Command name cannot be null or contain any whitespace characters.", nameof(Name));

            if (this.Name != null)
                throw new InvalidOperationException("This command already has a name.");

            if (this.AliasList.Contains(Name))
                throw new ArgumentException("Command name cannot be one of its aliases.", nameof(Name));

            this.Name = Name;
            return this;
        }

        /// <summary>
        /// Adds aliases to this command.
        /// </summary>
        /// <param name="Aliases">Aliases to add to the command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithAliases(params string[] Aliases)
        {
            if (Aliases == null || !Aliases.Any())
                throw new ArgumentException("You need to pass at least one alias.", nameof(Aliases));

            foreach (var alias in Aliases)
                this.WithAlias(alias);

            return this;
        }

        /// <summary>
        /// Adds an alias to this command.
        /// </summary>
        /// <param name="Alias">Alias to add to the command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithAlias(string Alias)
        {
            if (Alias.ToCharArray().Any(Xc => char.IsWhiteSpace(Xc)))
                throw new ArgumentException("Aliases cannot contain whitespace characters or null strings.", nameof(Alias));

            if (this.Name == Alias || this.AliasList.Contains(Alias))
                throw new ArgumentException("Aliases cannot contain the command name, and cannot be duplicate.", nameof(Alias));

            this.AliasList.Add(Alias);
            return this;
        }

        /// <summary>
        /// Sets the description for this command.
        /// </summary>
        /// <param name="Description">Description to use for this command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithDescription(string Description)
        {
            this.Description = Description;
            return this;
        }

        /// <summary>
        /// Sets whether this command is to be hidden.
        /// </summary>
        /// <param name="Hidden">Whether the command is to be hidden.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithHiddenStatus(bool Hidden)
        {
            this.IsHidden = Hidden;
            return this;
        }

        /// <summary>
        /// Adds pre-execution checks to this command.
        /// </summary>
        /// <param name="Checks">Pre-execution checks to add to this command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithExecutionChecks(params CheckBaseAttribute[] Checks)
        {
            this.ExecutionCheckList.AddRange(Checks.Except(this.ExecutionCheckList));
            return this;
        }

        /// <summary>
        /// Adds a pre-execution check to this command.
        /// </summary>
        /// <param name="Check">Pre-execution check to add to this command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithExecutionCheck(CheckBaseAttribute Check)
        {
            if (!this.ExecutionCheckList.Contains(Check))
                this.ExecutionCheckList.Add(Check);
            return this;
        }

        /// <summary>
        /// Adds overloads to this command. An executable command needs to have at least one overload.
        /// </summary>
        /// <param name="Overloads">Overloads to add to this command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithOverloads(params CommandOverloadBuilder[] Overloads)
        {
            foreach (var overload in Overloads)
                this.WithOverload(overload);

            return this;
        }

        /// <summary>
        /// Adds an overload to this command. An executable command needs to have at least one overload.
        /// </summary>
        /// <param name="Overload">Overload to add to this command.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithOverload(CommandOverloadBuilder Overload)
        {
            if (this.OverloadArgumentSets.Contains(Overload.ArgumentSet))
                throw new DuplicateOverloadException(this.Name, Overload.Arguments.Select(X => X.Type).ToList(), Overload.ArgumentSet);

            this.OverloadArgumentSets.Add(Overload.ArgumentSet);
            this.OverloadList.Add(Overload);

            return this;
        }

        /// <summary>
        /// Adds a custom attribute to this command. This can be used to indicate various custom information about a command.
        /// </summary>
        /// <param name="Attribute">Attribute to add.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithCustomAttribute(Attribute Attribute)
        {
            this.CustomAttributeList.Add(Attribute);
            return this;
        }

        /// <summary>
        /// Adds multiple custom attributes to this command. This can be used to indicate various custom information about a command.
        /// </summary>
        /// <param name="Attributes">Attributes to add.</param>
        /// <returns>This builder.</returns>
        public CommandBuilder WithCustomAttributes(params Attribute[] Attributes)
        {
            foreach (var attr in Attributes)
                this.WithCustomAttribute(attr);

            return this;
        }

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <param name="Parent">The parent command group.</param>
        internal virtual Command Build(CommandGroup Parent)
        {
            var cmd = new Command
            {
                Name = this.Name,
                Description = this.Description,
                Aliases = this.Aliases,
                ExecutionChecks = this.ExecutionChecks,
                IsHidden = this.IsHidden,
                Parent = Parent,
                Overloads = new ReadOnlyCollection<CommandOverload>(this.Overloads.Select(Xo => Xo.Build()).ToList()),
                Module = this.Module,
                CustomAttributes = this.CustomAttributes
            };

            return cmd;
        }
    }
}
