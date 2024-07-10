---
uid: getting_started_templates
title: Project Templates
author: DisCatSharp Team
---

# Prerequisites

Install the following packages:

-   DisCatSharp.ProjectTemplates

To Install the latest:

```powershell
dotnet new --install DisCatSharp.ProjectTemplates
```

To install a specific version (example uses 10.0.0):

```powershell
dotnet new --install DisCatSharp.ProjectTemplates::10.0.0
```

# Steps

## ![Install Setup](/images/pt_nuget_install.png)

If you're using Visual Studio, the templates will show up when creating a new project/solution
![Install Setup](/images/pt_project_new.png)

To easily find the DCS templates, you can search for either `Bot` or `Discord`. These tags are generic so if anyone else creates their own
discord or bot template our DCS templates will still be discoverable. We shall be using the solution template for our example.
![Classification](/images/pt_project_new_classification.png)

For example sake, the project name is DCSTest
![Project Name](/images/pt_project_new_name.png)

Input your Discord Token which can be retrieved via Discord's Developer Portal. The checkboxes represent the various modules in the DCS library. Checking it,
will include it in your project. If it's an extension, it automatically gets configured/included.
![Parameters](/images/pt_project_new_options.png)

You should see something similar to the following image. It's worth noting that you need to set the Web project as the `Startup` project. Due to the web being
list last, the `Bot` project is considered the startup. You would think that a class-library which doesn't have an exe could be considered a startup project....
![Project Structure](/images/pt_scaffolded.png)

At this point in time the template is ready to run!

---

# Templates

## Bot Template

This is a class library in which you place bot related code. It contains its own json file where you can
configure your bot accordingly!

An extension class provides easy to call methods for adding the Bot's services/configuration into the dependency injection (DI) pipeline.

## Web Template

This is a very minimal project. By itself it only has a default endpoint which displays "Hello World".

## Solution Template

Combines the bot and web templates. Includes the appropriate references/calls to get your bot up and running with minimal
effort.
