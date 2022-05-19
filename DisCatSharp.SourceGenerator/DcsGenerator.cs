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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DisCatSharp.SourceGenerator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Newtonsoft.Json;

namespace DisCatSharp.SourceGenerator;

[Generator]
public class DcsGenerator : ISourceGenerator
{
	private readonly Regex _fileRegex = new("(.)*.json");
	private GeneratorExecutionContext _context;

	public void Initialize(GeneratorInitializationContext context)// => context.RegisterForSyntaxNotifications(()=>new DcsSyntaxReceiver());
	{

	}

	private void Log(string message, DiagnosticSeverity severity = DiagnosticSeverity.Error) =>
		this._context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SI001",
				message,
				message,
				"DisCatSharp-Generator",
				severity,
				true),
			null));

	public void Execute(GeneratorExecutionContext context)
	{
		this._context = context;
		var jsonFiles = context.AdditionalFiles.Where(x => this._fileRegex.IsMatch(x.Path));
		var assemblyName = context.Compilation.AssemblyName ?? "DisCatSharp.SourceGenerator";

		ConcurrentBag<string> commands = new();

		HashSet<string> requiredNamespaces = new()
		{
			assemblyName,
			Constants.DCS_NAMESPACE,
			Constants.DCS_ENTITIES,
			Constants.DCS_APPLICATION_COMMANDS
		};

		foreach(var file in jsonFiles)
		{
			var parsed = JsonConvert.DeserializeObject<List<CommandInfo>>(File.ReadAllText(file.Path));
			this.Log($"Processing: {file.Path}", DiagnosticSeverity.Info);
			if (parsed is null)
				return;

			foreach (var c in parsed)
				try
				{
					commands.Add(CommandBuilder.Build(c, ref requiredNamespaces));
				}
				catch (Exception ex)
				{
					this.Log($"Error occurred while processing {c.Name}:\n\t{ex.Message}\n{ex.StackTrace}");
				}

			var fileInfo = new FileInfo(file.Path);
			var userClass = fileInfo.Name.Split('.').First().Replace("-", string.Empty) + "Base";
			var sourceText = SourceText.From($@"
/*
	A T T E N T I O N

	Do not modify this file.
	It is auto-generated and will be overriden during build.

	A T T E N T I O N
*/

{string.Join("\n", requiredNamespaces.Select(x=>$"using {x};"))}
public abstract class {userClass}
{{
	{string.Join("\n\n", commands)}
}}", Encoding.UTF8);

			context.AddSource($"{userClass}.Generated.cs", sourceText);
		}
	}
}
