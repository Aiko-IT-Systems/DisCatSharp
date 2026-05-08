---
uid: home
title: DisCatSharp
author: DisCatSharp Team
hasDiscordComponents: true
_disableAffix: true
_disableBreadcrumb: true
_disableNextArticle: true
---

<section class="catpunk-hero">
	<div>
		<div class="catpunk-eyebrow">catpunk discord engineering</div>
		<h1 class="catpunk-hero-title"><span class="catpunk-gradient-text">Build bots that keep up.</span></h1>
		<p class="catpunk-hero-copy">
			DisCatSharp is a modern .NET wrapper for the Discord API, focused on fast access to new Discord features,
			practical bot ergonomics, and docs that do not make you want to bite your keyboard.
		</p>
		<div class="catpunk-actions">
			<a class="catpunk-button" href="~/articles/getting_started/first_bot.md">Start building</a>
			<a class="catpunk-button secondary" href="~/api/index.md">Browse API</a>
			<a class="catpunk-button secondary" href="https://discord.gg/RXA6u3jxdU">Join Discord</a>
		</div>
	</div>
	<div class="catpunk-discord-preview">
		<discord-messages>
			<discord-header guild="DisCatSharp" channel="general" icon="https://i.imgur.com/sHdXUPx.png">catpunk docs preview</discord-header>
			<discord-message profile="user">I need a C# Discord library that is not stuck in the past.</discord-message>
			<discord-message profile="dcs" highlight>
				<discord-reply slot="reply" profile="user" mentions>I need a C# Discord library...</discord-reply>
				Use <discord-bold>DisCatSharp</discord-bold>. Slash commands, components, modals, web ingress, voice, Lavalink, and practical docs are all here.
				<discord-reactions slot="reactions">
					<discord-reaction interactive="true" name="catjam" emoji="https://cdn.discordapp.com/emojis/1059823127271575612.gif" count="42"></discord-reaction>
				</discord-reactions>
			</discord-message>
		</discord-messages>
	</div>
</section>

<section class="catpunk-card-grid" aria-label="DisCatSharp highlights">
	<div class="catpunk-card">
		<h3>API-first, feature-fast</h3>
		<p>Access the newest Discord API surfaces without waiting forever for wrappers to catch up.</p>
	</div>
	<div class="catpunk-card">
		<h3>Modules that scale</h3>
		<p>Application Commands, Interactivity, Lavalink, Voice, Hosting, Web Ingress, and more.</p>
	</div>
	<div class="catpunk-card">
		<h3>Practical docs</h3>
		<p>Guides, examples, generated API docs, and Discord-style previews for real bot workflows.</p>
	</div>
</section>

<section class="catpunk-panel">
	<h2>Install the stable package</h2>
	<p>Start from the core package, then add modules as your bot grows.</p>
	<div class="catpunk-terminal">
		<div class="catpunk-terminal-bar"><span class="catpunk-dot" aria-hidden="true"></span></div>
		<pre><code class="lang-powershell">dotnet add package DisCatSharp</code></pre>
	</div>
	<div class="catpunk-actions">
		<img alt="Stable NuGet version" src="https://img.shields.io/nuget/v/DisCatSharp?color=1F8B4C&label=Stable&style=flat-square&logo=Nuget">
		<img alt="Nightly NuGet version" src="https://img.shields.io/nuget/vpre/DisCatSharp?color=AD1457&label=Nightly&style=flat-square&logo=Nuget">
	</div>
</section>

<section class="catpunk-link-grid" aria-label="Quick links">
	<a class="catpunk-link-card" href="~/articles/getting_started/first_bot.md">
		<strong>Getting Started</strong>
		<span>Create your first bot and connect it to Discord.</span>
	</a>
	<a class="catpunk-link-card" href="~/articles/toc.yml">
		<strong>Articles</strong>
		<span>Guides for modules, hosting, topics, and workflows.</span>
	</a>
	<a class="catpunk-link-card" href="~/api/index.md">
		<strong>API Reference</strong>
		<span>Generated types, members, overloads, and XML docs.</span>
	</a>
	<a class="catpunk-link-card" href="https://github.com/Aiko-IT-Systems/DisCatSharp">
		<strong>GitHub</strong>
		<span>Source, issues, discussions, and contributions.</span>
	</a>
</section>

<section class="catpunk-card-grid" aria-label="Fresh paths">
	<div class="catpunk-card">
		<h3>Interactivity</h3>
		<p>Messages, reactions, polls, buttons, selects, modals, and modern pagination helpers.</p>
		<p><a href="~/articles/modules/interactivity/interactivity.md">Read the interactivity guides</a></p>
	</div>
	<div class="catpunk-card">
		<h3>Web Ingress</h3>
		<p>Handle Discord interactions, webhooks, OAuth callbacks, and linked roles from ASP.NET Core.</p>
		<p><a href="~/articles/modules/web_ingress/overview.md">Open Web Ingress</a></p>
	</div>
	<div class="catpunk-card">
		<h3>Changelogs</h3>
		<p>Track what changed and decide when to update your bot.</p>
		<p><a href="~/changelogs/toc.yml">View changelogs</a></p>
	</div>
</section>

<section class="catpunk-panel">
	<h2>Thanks and sponsors</h2>
	<p>
		DisCatSharp is MIT licensed and community-supported. Thanks to everyone filing issues, writing docs,
		testing new Discord features, and helping other developers in the server.
	</p>
	<p>
		Special thanks to Nagisa for artwork and to <a href="https://sentry.io">Sentry</a> for sponsoring error tracking.
	</p>
</section>
