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

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>", Scope = "member", Target = "~F:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.s_registeredCommands")]
[assembly: SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.RegistrationWorker.BuildGuildCreateList(System.UInt64,System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand})~System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand}")]
[assembly: SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.RegistrationWorker.BuildGuildDeleteList(System.UInt64,System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand})~System.Collections.Generic.List{System.UInt64}")]
[assembly: SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.RegistrationWorker.BuildGuildOverwriteList(System.UInt64,System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand})~System.ValueTuple{System.Collections.Generic.Dictionary{System.UInt64,DisCatSharp.Entities.DiscordApplicationCommand},System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand}}")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.CheckRegistrationStartup(System.Boolean)")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.RegisterCommands(System.Collections.Generic.IEnumerable{DisCatSharp.ApplicationCommands.ApplicationCommandsModuleConfiguration},System.Nullable{System.UInt64})~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.UpdateAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1842:Do not use 'WhenAll' with a single task", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.DefaultHelpModule.DefaultHelpAutoCompleteLevelOneProvider.Provider(DisCatSharp.ApplicationCommands.AutocompleteContext)~System.Threading.Tasks.Task{System.Collections.Generic.IEnumerable{DisCatSharp.Entities.DiscordApplicationCommandAutocompleteChoice}}")]
[assembly: SuppressMessage("Performance", "CA1842:Do not use 'WhenAll' with a single task", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.DefaultHelpModule.DefaultHelpAutoCompleteLevelTwoProvider.Provider(DisCatSharp.ApplicationCommands.AutocompleteContext)~System.Threading.Tasks.Task{System.Collections.Generic.IEnumerable{DisCatSharp.Entities.DiscordApplicationCommandAutocompleteChoice}}")]
[assembly: SuppressMessage("Performance", "CA1842:Do not use 'WhenAll' with a single task", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.DefaultHelpModule.DefaultHelpAutoCompleteProvider.Provider(DisCatSharp.ApplicationCommands.AutocompleteContext)~System.Threading.Tasks.Task{System.Collections.Generic.IEnumerable{DisCatSharp.Entities.DiscordApplicationCommandAutocompleteChoice}}")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.ApplicationCommandEqualityChecks.IsEqualTo(DisCatSharp.Entities.DiscordApplicationCommand,DisCatSharp.Entities.DiscordApplicationCommand)~System.Boolean")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.RegistrationWorker.BuildGlobalOverwriteList(System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand})~System.ValueTuple{System.Collections.Generic.Dictionary{System.UInt64,DisCatSharp.Entities.DiscordApplicationCommand},System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand}}")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.RegistrationWorker.BuildGuildOverwriteList(System.UInt64,System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand})~System.ValueTuple{System.Collections.Generic.Dictionary{System.UInt64,DisCatSharp.Entities.DiscordApplicationCommand},System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand}}")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.RegistrationWorker.RegisterGlobalCommandsAsync(System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand})~System.Threading.Tasks.Task{System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand}}")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.RegistrationWorker.RegisterGuildCommandsAsync(System.UInt64,System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand})~System.Threading.Tasks.Task{System.Collections.Generic.List{DisCatSharp.Entities.DiscordApplicationCommand}}")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.DefaultHelpModule.DefaultHelpAsync(DisCatSharp.ApplicationCommands.InteractionContext,System.String,System.String,System.String)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.RunPreexecutionChecksAsync(System.Reflection.MethodInfo,DisCatSharp.ApplicationCommands.BaseContext)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~P:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.GlobalCommands")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~P:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.GuildCommands")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~P:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.RegisteredCommands")]
