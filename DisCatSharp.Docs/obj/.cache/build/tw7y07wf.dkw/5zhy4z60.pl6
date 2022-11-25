<!DOCTYPE html>
<!--[if IE]><![endif]-->
<html>

    <head>
      <meta charset="utf-8">
      <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
      <title>Version 9.9.0 | DisCatSharp Docs </title>
      <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=no">
      <meta name="title" content="Version 9.9.0 | DisCatSharp Docs ">
      <meta name="og:title" content="Version 9.9.0 | DisCatSharp Docs ">
      <meta name="generator" content="docfx 2.60.0.0">
  
    <meta name="og:type" content="website">
    <meta name="og:image" content="https://cdn.aitsys.dev/file/data/kmjpa6f64me66dsm7dz3/PHID-FILE-degpfzd7nbw2q5yko5j7/logobig.png">
    <meta name="og:image:alt" content="DisCatSharp Logo">
    <meta name="og:image:type" content="image/png">
    <meta name="og:site_name" content="DisCatSharp Documentation">
    <link rel="apple-touch-icon" sizes="57x57" href="../../apple-icon-57x57.png">
    <link rel="apple-touch-icon" sizes="60x60" href="../../apple-icon-60x60.png">
    <link rel="apple-touch-icon" sizes="72x72" href="../../apple-icon-72x72.png">
    <link rel="apple-touch-icon" sizes="76x76" href="../../apple-icon-76x76.png">
    <link rel="apple-touch-icon" sizes="114x114" href="../../apple-icon-114x114.png">
    <link rel="apple-touch-icon" sizes="120x120" href="../../apple-icon-120x120.png">
    <link rel="apple-touch-icon" sizes="144x144" href="../../apple-icon-144x144.png">
    <link rel="apple-touch-icon" sizes="152x152" href="../../apple-icon-152x152.png">
    <link rel="apple-touch-icon" sizes="180x180" href="../../apple-icon-180x180.png">
    <link rel="icon" type="image/png" sizes="192x192" href="../../android-icon-192x192.png">
    <link rel="icon" type="image/png" sizes="32x32" href="../../favicon-32x32.png">
    <link rel="icon" type="image/png" sizes="96x96" href="../../favicon-96x96.png">
    <link rel="icon" type="image/png" sizes="16x16" href="../../favicon-16x16.png">
    <link rel="manifest" href="/manifest.json">
    <meta name="msapplication-TileColor" content="#ffffff">
    <meta name="msapplication-TileImage" content="../../ms-icon-144x144.png">
    <meta name="theme-color" content="#ffffff">
      <link rel="shortcut icon" href="../../favicon.ico">
      <script defer="" src='https://static.cloudflareinsights.com/beacon.min.js' data-cf-beacon='{"token": "de6c22ce0b3e4c17bb78c8c31b4e695b"}'></script>
      <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap" rel="stylesheet">
      <link rel="stylesheet" href="//cdnjs.cloudflare.com/ajax/libs/highlight.js/11.7.0/styles/night-owl.min.css">
      <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.7.2/font/bootstrap-icons.css" integrity="sha384-EvBWSlnoFgZlXJvpzS+MAUEjvN7+gcCwH+qh7GRFOGgZO0PuwOFro7qPOJnLfe7l" crossorigin="anonymous">
      <link rel="stylesheet" href="../../src/styles/config.css">
      <link rel="stylesheet" href="../../src/styles/discord.css">
      <link rel="stylesheet" href="../../src/styles/dcs.css">
      <link rel="stylesheet" href="../../src/styles/main.css">
      <link rel="stylesheet" href="../../src/styles/colors.css">
      <link rel="stylesheet" href="../../src/styles/highlight/github-dark.min.css">
      <meta property="docfx:navrel" content="../../toc.html">
      <meta property="docfx:tocrel" content="../toc.html">
  
  <meta property="docfx:rel" content="../../">
  <meta property="docfx:newtab" content="true">
    </head>

    <body>
        <div class="top-navbar">
            <a class="burger-icon" onclick="toggleMenu()">
                <svg name="Hamburger" style="vertical-align: middle;" width="34" height="34" viewbox="0 0 24 24"><path fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" d="M20 6H4V9H20V6ZM4 10.999H20V13.999H4V10.999ZM4 15.999H20V18.999H4V15.999Z"></path></svg>
            </a>


            <a class="navbar-brand" href="../../index.html">
              <img id="logo" class="svg" src="../../logo.png" alt="DisCatSharp">
            </a>
        </div>

        <div class="body-content">
            <div id="blackout" class="blackout" onclick="toggleMenu()"></div>

            <nav id="sidebar" role="navigation">
                <div class="sidebar">

                    <div>
                      <div class="mobile-hide">

                        <a class="navbar-brand" href="../../index.html">
                          <img id="logo" class="svg" src="../../logo.png" alt="DisCatSharp">
                        </a>
                      </div>

                      <div class="sidesearch">
                        <form id="search" role="search" class="search">
                            <i class="bi bi-search search-icon"></i>
                            <input type="text" id="search-query" placeholder="Search" autocomplete="off">
                        </form>
                      </div>

                      <div id="navbar">
                      </div>
                    </div>
                    <div class="sidebar-item-separator"></div>

                        <div id="sidetoggle">
                          <div id="sidetoc"></div>
                        </div>
                </div>
                <div class="footer">
  <strong>Made with ♥ by AITSYS</strong>
  
                </div>
            </nav>

            <main class="main-panel">

                <div id="search-results" style="display: none;">
                  <h1 class="search-list">Search Results for <span></span></h1>
                  <div class="sr-items">
                    <p class="lsearch"><i class="bi bi-hourglass-split index-loading"></i></p>
                  </div>
                  <ul id="pagination" data-first="First" data-prev="Previous" data-next="Next" data-last="Last"></ul>
                </div>

                <div role="main" class="hide-when-search">

                        <div class="subnav navbar navbar-default">
                          <div class="container hide-when-search" id="breadcrumb">
                            <ul class="breadcrumb">
                              <li></li>
                            </ul>
                          </div>
                        </div>

      <div id="sidetoggle">
        <div id="sidetoc"></div>
      </div>
						<div class="article row grid-right">

                    <article class="content wrap" id="_content" data-uid="changelogs_v9_9_9_0">
<h1 id="upgrade-from-986-to-990" sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="6" sourceendlinenumber="6">Upgrade from <strong>9.8.6</strong> to <strong>9.9.0</strong></h1>

<h2 id="what-is-new-in-discatsharp" sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="8" sourceendlinenumber="8">What is new in DisCatSharp?</h2>
<ul sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="9" sourceendlinenumber="10">
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="9" sourceendlinenumber="9">Slash Attachments</li>
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="10" sourceendlinenumber="10"><a class="xref" href="../../api/DisCatSharp.Common/DisCatSharp.Common.RegularExpressions.html" data-raw-source="[DisCatSharp.Common.RegularExpressions](xref:DisCatSharp.Common.RegularExpressions)" sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="10" sourceendlinenumber="10">DisCatSharp.Common.RegularExpressions</a></li>
</ul>
<h2 id="what-changed" sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="12" sourceendlinenumber="12">What changed?</h2>
<p sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="14" sourceendlinenumber="14">This will be a quick one:</p>
<ul sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="16" sourceendlinenumber="22">
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="16" sourceendlinenumber="16"><a class="xref" href="../../api/DisCatSharp/DisCatSharp.Entities.DiscordInvite.html" data-raw-source="[DiscordInvite](xref:DisCatSharp.Entities.DiscordInvite)" sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="16" sourceendlinenumber="16">DiscordInvite</a> has the property <code>InviteTarget</code> to support user invites.</li>
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="17" sourceendlinenumber="17">A few NRE&#39;s where fixed</li>
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="18" sourceendlinenumber="18">Guild scheduled events support uploading a cover image</li>
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="19" sourceendlinenumber="19"><a class="xref" href="../../api/DisCatSharp/DisCatSharp.Entities.DiscordThreadChannel.html" data-raw-source="[DiscordThreadChannel](xref:DisCatSharp.Entities.DiscordThreadChannel)" sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="19" sourceendlinenumber="19">DiscordThreadChannel</a> has the new property <code>CreateTimestamp</code></li>
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="20" sourceendlinenumber="20">The AsEphemeral functions defaulting to <code>true</code> now</li>
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="21" sourceendlinenumber="21">Slash Attachments fully works</li>
<li sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="22" sourceendlinenumber="22">You can let the <a class="xref" href="../../api/DisCatSharp.ApplicationCommands/DisCatSharp.ApplicationCommands.ApplicationCommandsModule.html" data-raw-source="[ApplicationCommandsModule](xref:DisCatSharp.ApplicationCommands.ApplicationCommandsModule)" sourcefile="changelogs/v9/9_9_0.md" sourcestartlinenumber="22" sourceendlinenumber="22">ApplicationCommandsModule</a> auto defer all responses</li>
</ul>
</article>
                </div>

                <div class="copyright-footer">
                    <span>&#169; Aiko IT Systems. All rights reserved.</span>
                </div>
            </div></main>
        </div>


        <script src="https://code.jquery.com/jquery-3.5.1.min.js" integrity="sha256-9/aliU8dGd2tb6OSsuzixeV4y/faTqgFtohetphbbj0=" crossorigin="anonymous"></script>
        <script type="text/javascript" src="../../src/scripts/docfx.vendor.js"></script>
        <script type="text/javascript" src="../../src/scripts/docfx.js"></script>
        <script type="text/javascript" src="../../src/scripts/url.min.js"></script>
        <script type="text/javascript" src="../../src/scripts/highlight/highlight.min.js"></script>
        <script>hljs.highlightAll();</script>
        <script src="https://cdn.jsdelivr.net/npm/anchor-js/anchor.min.js"></script>
        <script type="text/javascript" src="../../src/scripts/jquery.twbsPagination.js"></script>
        <script type="text/javascript" src="../../src/scripts/dcs.js"></script>
        <script type="text/javascript" src="../../src/scripts/lunr.js"></script>
    </body>
</html>
