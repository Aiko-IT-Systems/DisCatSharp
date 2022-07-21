# DisCatSharp Source Generation
#### Purpose:
Jumpstart the developer experience by enabling you to define Discord commands
via JSON while allowing `source generation` take care of most of the heavy lifting.

#### Badger 2-3:
I was tired of looking at the attribute soup mess. Now it can be hidden away
in the source-generated content. Now I can work with a much cleaner experience.

----
# NOTE

This feature is under construction. Things are subject to change.

Source-generation is not going to change/impact DCS in any way. All it does is generate

----

# Setup
Create a folder called `commands` in your project. This will be where all of your
slash-command json files will go.

Each json file will need to be defined as `Additional Files` which can be done
in a few different ways.

1. If you're using Visual Studio, click on the file. In the properties window change the build output to `Additional Files`
2. If you're in Rider, right click on file --> properties --> `Additional Files`
3. If you rather auto-designate these files as additional files open your csproj file and add the following:
```xml
 <ItemGroup>
      <None Remove="commands\*.json" />
      <AdditionalFiles Include="commands\*.json" />
</ItemGroup>
```

# Configuration Information
**Command Properties**
```yaml
name: Name of your command
description: Description for your command
parameters: Optional array. Please reference the Parameter JSON config for more info
permissions: Optional array. Discord permissions as strings
```

**Parameter Properties**
```yaml
name: Name of parameter
description: Description of what the parameter is / does
type: The strongly typed representation. i.e string, int, DiscordUser, etc.
ac-provider: The strongly-typed class that will handle auto-completion
```

**Sample Json**
```json
[
	{
		"name": "SampleCommand",
		"description": "Does something simple",
		"permissions": [
			"ManageMessages"
		],
		"parameters": [
			{
				"name": "topic",
				"description": "topic of conversation",
				"type": "string"
			}
		]
	}
]
```

----

# Implementation
After you create the json config files all you have to do is build the project! This will create a `Base` class where all you have to do is implement it along with the abstract
methods!

If I have a json file called `admin-commands` it will create an abstract class called `admincommandsBase`. This class will contain abstract methods that get called when a user interacts
with your command(s).
