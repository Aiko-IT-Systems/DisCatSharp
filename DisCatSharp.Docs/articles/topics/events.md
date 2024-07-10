---
uid: topics_events
title: DisCatSharp Events
author: DisCatSharp Team
---

# Events In DisCatSharp

## Consuming Events

DisCatSharp makes use of _asynchronous events_ which will execute each handler asynchronously in sequential order.
This event system will require event handlers have a `Task` return type and take two parameters.

The first parameter will contain an instance of the object which fired the event.<br/>
The second parameter will contain an arguments object for the specific event you're handling.

Below is a snippet demonstrating this with a lambda expression.

```cs
private async Task MainAsync()
{
    var discord = new DiscordClient();

    discord.MessageCreated += async (s, e) =>
    {
        if (e.Message.Content.ToLower().Contains("spiderman"))
            await e.Message.RespondAsync("I want pictures of Spiderman!");
    };

	discord.GuildMemberAdded += (s, e) =>
    {
        // Non asynchronous code here.
        return Task.CompletedTask;
    };
}
```

Alternatively, you can create a new method to consume an event.

```cs
private async Task MainAsync()
{
    var discord = new DiscordClient();

    discord.MessageCreated += MessageCreatedHandler;
	discord.GuildMemberAdded += MemberAddedHandler;
}

private async Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
{
    if (e.Guild?.Id == 379378609942560770 && e.Author.Id == 168548441939509248)
        await e.Message.DeleteAsync();
}

private Task MemberAddedHandler(DiscordClient s, GuildMemberAddEventArgs e)
{
    // Non asynchronous code here.
    return Task.CompletedTask;
}
```

## Using automatic event registration

Instead of having to manually register each event, the attributes `Event` and `EventHandler` can be utilized to semi-automatically register events in a multitude of ways.

By attributing all classes that constitute event handlers with `EventHandler` and all methods within those classes that are intended to handle events with `Event` you can register all of those events with a single call to `DiscordClient.RegisterEventHandlers(Assembly)`

```cs
[EventHandler]
public class MyEventHandler
{
    [Event]
    private async Task MessageCreated(DiscordClient s, MessageCreateEventArgs e) { /* ... */ }

    [Event(DiscordEvent.MessageCreated)] // You can specify the event name in the attribute, instead of via the method name!
    public static async Task MySecondaryHandler(DiscordClient s, MessageCreateEventArgs e) { /* ... */ }
}
```

```cs
private async Task MainAsync()
{
    var discord = new DiscordClient();
    discord.RegisterEventHandlers(Assembly.GetExecutingAssembly());
}
```

If an event handler class is not `abstract` (which also means not `static`) it will be instantiated, optionally with the help of an `IServiceProvider`, if one has been provided to the `DiscordConfig`. The inability to instantiate an event handler type constitutes an error.

```cs
public class SomeClass { }

[EventHandler]
public class MyOtherEventHandler
{
    private SomeClass some;

    public MyOtherEventHandler(SomeClass some) { this.some = some; }

    [Event]
    public async Task MessageCreated(DiscordClient s, MessageCreateEventArgs e) { /* do something with some */ }
}
```

In the above example an instance of `SomeClass` will need to be provided to the `DiscordContext`'s `IServiceProvider`, read the next chapter to see how to accomplish that!

You can also register individual types, and even individual objects as event handlers using the overloaded method `DiscordClient.RegisterEventHandler`. In both of those cases the attribute `EventHandler` is not required.

When registering an object as a handler, by default the type's static methods will _not_ be considered as event handling methods.
This allows for registering multiple instances of the same type without registering their static event handling methods multiple times.
To register the static methods exclusively, use `DiscordClient.RegisterStaticEventHandler` with the type in question.

## Dependency Injection

Often, you need a way to get data in and out of your event handlers.
Although you _could_ use `static` fields to accomplish this, the preferred solution would be _dependency injection_.

First, you need to register the services that you can use in the event handlers in the future.

You can do this in DiscordConfiguration:

```cs
var config = new DiscordConfiguration()
{
    Token = "Token here",
    TokenType = TokenType.Bot,
    ServiceProvider = new ServiceCollection()
        .AddScoped<YourService>()
        .AddSingleton<YourSecondService>()
        .BuildServiceProvider()
};
```

In this case, we have registered two services: `YourService` as Scoped and` YourSecondService` as Singleton.

Now you can use them in your event handlers.

```cs
private async Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
{
    var service = e.ServiceProvider.GetRequiredService<YourService>();
    var secondService = e.ServiceProvider.GetRequiredService<YourSecondService>();
}
```

### Services

| Lifespan  | Instantiated                           |
| :-------: | :------------------------------------- |
| Singleton | One time when added to the collection. |
|  Scoped   | Once for each event handler.           |
| Transient | Every request to the ServiceProvider.  |

## Avoiding Deadlocks

Despite the fact that your event handlers are executed asynchronously, they are also executed one at a time on the gateway thread for consistency.
This means that each handler must complete its execution before others can be dispatched.

Because of this, executing code in your event handlers that runs for an extended period of time may inadvertently
create brief unresponsiveness or, even worse, cause a [deadlock](https://en.wikipedia.org/wiki/Deadlock).
To prevent such issues, any event handler that has the potential to take more than 2 seconds to execute should have its logic offloaded to a `Task.Run`.

```cs
discord.MessageCreated += (s, e) =>
{
    _ = Task.Run(async () =>
    {
        // Pretend this takes many, many seconds to execute.
        var response = await QuerySlowWebServiceAsync(e.Message.Content);

        if (response.Status == HttpStatusCode.OK)
        {
            await e.Guild?.BanMemberAsync((DiscordMember)e.Author);
        }
    });

	return Task.CompletedTask;
};
```

Doing this will allow the handler to complete its execution quicker, which will in turn allow other handlers to be executed and prevent the gateway thread from being blocked.
