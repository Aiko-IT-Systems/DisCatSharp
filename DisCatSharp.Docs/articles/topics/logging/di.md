---
uid: topics_logging_di
title: Dependency Injection Logging
author: DisCatSharp Team
---

# Default Logger

By default, when using DI, you will be using Microsoft's implementation of `ILogger` and `ILoggerFactory`. This requires no additional setup.

# Third Party Logger

If you wish to use a different logging service/implementation you simply install the appropriate nuget package, or create your own which have implementations for `ILoggerFactory` and `ILogger`.

You need to register either this third party implementation or your own via the `IServiceCollection` which becomes `IServiceProvider` at runtime.

If we are using the DisCatSharp ProjectTemplates there will be a project with a `.Web`. Inside is a `Program.cs` file.

###WebHost: Serilog

-   Serilog.AspNetCore
-   Serilog.Extensions.Hosting -- Gives us the `UseSerilog` extension
-   Serilog.Sinks.Console -- Sinks are used to direct where logs go. In this case we need it for outputting to console

```cs
using Serilog;

// unrelated code not shown for brevity
builder.Host.UseSerilog( (context, lc) => lc.WriteTo.Console()); // Our logs are directed to the console

var app = builder.Build();
app.Run();
```
