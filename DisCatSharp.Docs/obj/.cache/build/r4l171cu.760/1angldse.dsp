<!DOCTYPE html>
<!--[if IE]><![endif]-->
<html>

    <head>
      <meta charset="utf-8">
      <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
      <title>Class DiscordGuild
 | DisCatSharp Docs </title>
      <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=no">
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
      <meta property="docfx:tocrel" content="toc.html">
  
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

                    <article class="content wrap" id="_content" data-uid="DisCatSharp.Entities.DiscordGuild">


  <h1 id="DisCatSharp_Entities_DiscordGuild" data-uid="DisCatSharp.Entities.DiscordGuild" class="text-break">Class DiscordGuild
</h1>
  <div class="markdown level0 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Represents a Discord guild.</p>
</div>
  <div class="markdown level0 conceptual"></div>
  <div class="inheritance">
    <h5>Inheritance</h5>
    <div class="level0"><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.object">Object</a></div>
    <div class="level1"><a class="xref" href="DisCatSharp.Entities.SnowflakeObject.html">SnowflakeObject</a></div>
    <div class="level2"><span class="xref">DiscordGuild</span></div>
  </div>
  <div class="implements">
    <h5>Implements</h5>
    <div><span class="xref">IEquatable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>&gt;</div>
  </div>
  <div class="inheritedMembers">
    <h5>Inherited Members</h5>
    <div>
      <a class="xref" href="DisCatSharp.Entities.SnowflakeObject.html#DisCatSharp_Entities_SnowflakeObject_Id">SnowflakeObject.Id</a>
    </div>
    <div>
      <a class="xref" href="DisCatSharp.Entities.SnowflakeObject.html#DisCatSharp_Entities_SnowflakeObject_CreationTimestamp">SnowflakeObject.CreationTimestamp</a>
    </div>
  </div>
  <h6><strong>Namespace</strong>: <a class="xref" href="DisCatSharp.Entities.html">DisCatSharp.Entities</a></h6>
  <h6><strong>Assembly</strong>: DisCatSharp.dll</h6>
  <h5 id="DisCatSharp_Entities_DiscordGuild_syntax">Syntax</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public class DiscordGuild : SnowflakeObject</code></pre>
  </div>
  <h3 id="properties">Properties
</h3>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_AfkChannel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.AfkChannel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L143">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_AfkChannel_" data-uid="DisCatSharp.Entities.DiscordGuild.AfkChannel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_AfkChannel" data-uid="DisCatSharp.Entities.DiscordGuild.AfkChannel">AfkChannel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s AFK voice channel.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordChannel AfkChannel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_AfkTimeout.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.AfkTimeout%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L160">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_AfkTimeout_" data-uid="DisCatSharp.Entities.DiscordGuild.AfkTimeout*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_AfkTimeout" data-uid="DisCatSharp.Entities.DiscordGuild.AfkTimeout">AfkTimeout</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s AFK timeout.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int AfkTimeout { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ApplicationId.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ApplicationId%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L260">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ApplicationId_" data-uid="DisCatSharp.Entities.DiscordGuild.ApplicationId*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ApplicationId" data-uid="DisCatSharp.Entities.DiscordGuild.ApplicationId">ApplicationId</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the application id of this guild if it is bot created.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public ulong? ApplicationId { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ApproximateMemberCount.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ApproximateMemberCount%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L350">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ApproximateMemberCount_" data-uid="DisCatSharp.Entities.DiscordGuild.ApproximateMemberCount*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ApproximateMemberCount" data-uid="DisCatSharp.Entities.DiscordGuild.ApproximateMemberCount">ApproximateMemberCount</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the approximate number of members in this guild, when using <a class="xref" href="DisCatSharp.DiscordClient.html#DisCatSharp_DiscordClient_GetGuildAsync_System_UInt64_System_Nullable_System_Boolean__System_Boolean_">GetGuildAsync(UInt64, Nullable&lt;Boolean&gt;, Boolean)</a> and having withCounts set to true.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int? ApproximateMemberCount { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ApproximatePresenceCount.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ApproximatePresenceCount%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L356">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ApproximatePresenceCount_" data-uid="DisCatSharp.Entities.DiscordGuild.ApproximatePresenceCount*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ApproximatePresenceCount" data-uid="DisCatSharp.Entities.DiscordGuild.ApproximatePresenceCount">ApproximatePresenceCount</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the approximate number of presences in this guild, when using <a class="xref" href="DisCatSharp.DiscordClient.html#DisCatSharp_DiscordClient_GetGuildAsync_System_UInt64_System_Nullable_System_Boolean__System_Boolean_">GetGuildAsync(UInt64, Nullable&lt;Boolean&gt;, Boolean)</a> and having withCounts set to true.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int? ApproximatePresenceCount { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_BannerHash.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.BannerHash%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L479">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_BannerHash_" data-uid="DisCatSharp.Entities.DiscordGuild.BannerHash*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_BannerHash" data-uid="DisCatSharp.Entities.DiscordGuild.BannerHash">BannerHash</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s banner hash, when applicable.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string BannerHash { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_BannerUrl.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.BannerUrl%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L485">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_BannerUrl_" data-uid="DisCatSharp.Entities.DiscordGuild.BannerUrl*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_BannerUrl" data-uid="DisCatSharp.Entities.DiscordGuild.BannerUrl">BannerUrl</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s banner in url form.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string BannerUrl { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Channels.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Channels%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L395">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Channels_" data-uid="DisCatSharp.Entities.DiscordGuild.Channels*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Channels" data-uid="DisCatSharp.Entities.DiscordGuild.Channels">Channels</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a dictionary of all the channels associated with this guild. The dictionary&apos;s key is the channel ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordChannel&gt; Channels { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CurrentMember.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CurrentMember%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L437">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CurrentMember_" data-uid="DisCatSharp.Entities.DiscordGuild.CurrentMember*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CurrentMember" data-uid="DisCatSharp.Entities.DiscordGuild.CurrentMember">CurrentMember</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild member for current user.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordMember CurrentMember { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DefaultMessageNotifications.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DefaultMessageNotifications%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L172">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DefaultMessageNotifications_" data-uid="DisCatSharp.Entities.DiscordGuild.DefaultMessageNotifications*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DefaultMessageNotifications" data-uid="DisCatSharp.Entities.DiscordGuild.DefaultMessageNotifications">DefaultMessageNotifications</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s default notification settings.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DefaultMessageNotifications DefaultMessageNotifications { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.DefaultMessageNotifications.html">DefaultMessageNotifications</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Description.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Description%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L473">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Description_" data-uid="DisCatSharp.Entities.DiscordGuild.Description*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Description" data-uid="DisCatSharp.Entities.DiscordGuild.Description">Description</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild description, when applicable.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string Description { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DiscoverySplashHash.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DiscoverySplashHash%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L83">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DiscoverySplashHash_" data-uid="DisCatSharp.Entities.DiscordGuild.DiscoverySplashHash*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DiscoverySplashHash" data-uid="DisCatSharp.Entities.DiscordGuild.DiscoverySplashHash">DiscoverySplashHash</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild discovery splash&apos;s hash.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string DiscoverySplashHash { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DiscoverySplashUrl.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DiscoverySplashUrl%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L89">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DiscoverySplashUrl_" data-uid="DisCatSharp.Entities.DiscordGuild.DiscoverySplashUrl*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DiscoverySplashUrl" data-uid="DisCatSharp.Entities.DiscordGuild.DiscoverySplashUrl">DiscoverySplashUrl</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild discovery splash&apos;s url.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string DiscoverySplashUrl { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Emojis.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Emojis%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L286">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Emojis_" data-uid="DisCatSharp.Entities.DiscordGuild.Emojis*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Emojis" data-uid="DisCatSharp.Entities.DiscordGuild.Emojis">Emojis</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a collection of this guild&apos;s emojis.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordEmoji&gt; Emojis { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordEmoji.html">DiscordEmoji</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_EveryoneRole.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.EveryoneRole%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L447">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_EveryoneRole_" data-uid="DisCatSharp.Entities.DiscordGuild.EveryoneRole*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_EveryoneRole" data-uid="DisCatSharp.Entities.DiscordGuild.EveryoneRole">EveryoneRole</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the @everyone role for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordRole EveryoneRole { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ExplicitContentFilter.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ExplicitContentFilter%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L178">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ExplicitContentFilter_" data-uid="DisCatSharp.Entities.DiscordGuild.ExplicitContentFilter*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ExplicitContentFilter" data-uid="DisCatSharp.Entities.DiscordGuild.ExplicitContentFilter">ExplicitContentFilter</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s explicit content filter settings.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public ExplicitContentFilter ExplicitContentFilter { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.ExplicitContentFilter.html">ExplicitContentFilter</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Features.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Features%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L302">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Features_" data-uid="DisCatSharp.Entities.DiscordGuild.Features*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Features" data-uid="DisCatSharp.Entities.DiscordGuild.Features">Features</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s features.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public GuildFeatures Features { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.GuildFeatures.html">GuildFeatures</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_HasMemberVerificationGate.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.HasMemberVerificationGate%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L504">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_HasMemberVerificationGate_" data-uid="DisCatSharp.Entities.DiscordGuild.HasMemberVerificationGate*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_HasMemberVerificationGate" data-uid="DisCatSharp.Entities.DiscordGuild.HasMemberVerificationGate">HasMemberVerificationGate</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Whether this guild has enabled membership screening.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool HasMemberVerificationGate { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_HasWelcomeScreen.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.HasWelcomeScreen%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L498">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_HasWelcomeScreen_" data-uid="DisCatSharp.Entities.DiscordGuild.HasWelcomeScreen*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_HasWelcomeScreen" data-uid="DisCatSharp.Entities.DiscordGuild.HasWelcomeScreen">HasWelcomeScreen</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Whether this guild has enabled the welcome screen.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool HasWelcomeScreen { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_HubType.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.HubType%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L534">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_HubType_" data-uid="DisCatSharp.Entities.DiscordGuild.HubType*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_HubType" data-uid="DisCatSharp.Entities.DiscordGuild.HubType">HubType</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s hub type, if applicable.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public HubType HubType { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.HubType.html">HubType</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_IconHash.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.IconHash%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L57">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_IconHash_" data-uid="DisCatSharp.Entities.DiscordGuild.IconHash*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_IconHash" data-uid="DisCatSharp.Entities.DiscordGuild.IconHash">IconHash</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild icon&apos;s hash.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string IconHash { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_IconUrl.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.IconUrl%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L63">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_IconUrl_" data-uid="DisCatSharp.Entities.DiscordGuild.IconUrl*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_IconUrl" data-uid="DisCatSharp.Entities.DiscordGuild.IconUrl">IconUrl</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild icon&apos;s url.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string IconUrl { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_IsCommunity.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.IsCommunity%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L492">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_IsCommunity_" data-uid="DisCatSharp.Entities.DiscordGuild.IsCommunity*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_IsCommunity" data-uid="DisCatSharp.Entities.DiscordGuild.IsCommunity">IsCommunity</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Whether this guild has the community feature enabled.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool IsCommunity { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_IsLarge.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.IsLarge%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L320">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_IsLarge_" data-uid="DisCatSharp.Entities.DiscordGuild.IsLarge*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_IsLarge" data-uid="DisCatSharp.Entities.DiscordGuild.IsLarge">IsLarge</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets whether this guild is considered to be a large guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool IsLarge { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_IsNsfw.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.IsNsfw%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L528">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_IsNsfw_" data-uid="DisCatSharp.Entities.DiscordGuild.IsNsfw*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_IsNsfw" data-uid="DisCatSharp.Entities.DiscordGuild.IsNsfw">IsNsfw</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets whether this guild is designated as NSFW.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool IsNsfw { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_IsOwner.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.IsOwner%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L457">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_IsOwner_" data-uid="DisCatSharp.Entities.DiscordGuild.IsOwner*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_IsOwner" data-uid="DisCatSharp.Entities.DiscordGuild.IsOwner">IsOwner</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets whether the current user is the guild&apos;s owner.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool IsOwner { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_IsUnavailable.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.IsUnavailable%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L326">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_IsUnavailable_" data-uid="DisCatSharp.Entities.DiscordGuild.IsUnavailable*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_IsUnavailable" data-uid="DisCatSharp.Entities.DiscordGuild.IsUnavailable">IsUnavailable</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets whether this guild is unavailable.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool IsUnavailable { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_JoinedAt.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.JoinedAt%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L314">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_JoinedAt_" data-uid="DisCatSharp.Entities.DiscordGuild.JoinedAt*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_JoinedAt" data-uid="DisCatSharp.Entities.DiscordGuild.JoinedAt">JoinedAt</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s join date.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DateTimeOffset JoinedAt { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">DateTimeOffset</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_MaxMembers.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.MaxMembers%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L338">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_MaxMembers_" data-uid="DisCatSharp.Entities.DiscordGuild.MaxMembers*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_MaxMembers" data-uid="DisCatSharp.Entities.DiscordGuild.MaxMembers">MaxMembers</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the maximum amount of members allowed for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int? MaxMembers { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_MaxPresences.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.MaxPresences%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L344">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_MaxPresences_" data-uid="DisCatSharp.Entities.DiscordGuild.MaxPresences*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_MaxPresences" data-uid="DisCatSharp.Entities.DiscordGuild.MaxPresences">MaxPresences</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the maximum amount of presences allowed for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int? MaxPresences { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_MaxStageVideoChannelUsers.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.MaxStageVideoChannelUsers%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L368">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_MaxStageVideoChannelUsers_" data-uid="DisCatSharp.Entities.DiscordGuild.MaxStageVideoChannelUsers*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_MaxStageVideoChannelUsers" data-uid="DisCatSharp.Entities.DiscordGuild.MaxStageVideoChannelUsers">MaxStageVideoChannelUsers</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the maximum amount of users allowed per video stage channel.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int? MaxStageVideoChannelUsers { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_MaxVideoChannelUsers.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.MaxVideoChannelUsers%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L362">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_MaxVideoChannelUsers_" data-uid="DisCatSharp.Entities.DiscordGuild.MaxVideoChannelUsers*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_MaxVideoChannelUsers" data-uid="DisCatSharp.Entities.DiscordGuild.MaxVideoChannelUsers">MaxVideoChannelUsers</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the maximum amount of users allowed per video channel.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int? MaxVideoChannelUsers { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_MemberCount.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.MemberCount%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L332">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_MemberCount_" data-uid="DisCatSharp.Entities.DiscordGuild.MemberCount*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_MemberCount" data-uid="DisCatSharp.Entities.DiscordGuild.MemberCount">MemberCount</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the total number of members in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int MemberCount { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Members.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Members%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L385">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Members_" data-uid="DisCatSharp.Entities.DiscordGuild.Members*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Members" data-uid="DisCatSharp.Entities.DiscordGuild.Members">Members</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a dictionary of all the members that belong to this guild. The dictionary&apos;s key is the member ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordMember&gt; Members { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_MfaLevel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.MfaLevel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L308">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_MfaLevel_" data-uid="DisCatSharp.Entities.DiscordGuild.MfaLevel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_MfaLevel" data-uid="DisCatSharp.Entities.DiscordGuild.MfaLevel">MfaLevel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the required multi-factor authentication level for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public MfaLevel MfaLevel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.MfaLevel.html">MfaLevel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Name.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Name%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L51">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Name_" data-uid="DisCatSharp.Entities.DiscordGuild.Name*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Name" data-uid="DisCatSharp.Entities.DiscordGuild.Name">Name</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s name.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string Name { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_NsfwLevel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.NsfwLevel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L184">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_NsfwLevel_" data-uid="DisCatSharp.Entities.DiscordGuild.NsfwLevel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_NsfwLevel" data-uid="DisCatSharp.Entities.DiscordGuild.NsfwLevel">NsfwLevel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s nsfw level.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public NsfwLevel NsfwLevel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.NsfwLevel.html">NsfwLevel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_OrderedChannels.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.OrderedChannels%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L540">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_OrderedChannels_" data-uid="DisCatSharp.Entities.DiscordGuild.OrderedChannels*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_OrderedChannels" data-uid="DisCatSharp.Entities.DiscordGuild.OrderedChannels">OrderedChannels</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a dictionary of all by position ordered channels associated with this guild. The dictionary&apos;s key is the channel ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordChannel&gt; OrderedChannels { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Owner.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Owner%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L109">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Owner_" data-uid="DisCatSharp.Entities.DiscordGuild.Owner*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Owner" data-uid="DisCatSharp.Entities.DiscordGuild.Owner">Owner</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s owner.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordMember Owner { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_OwnerId.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.OwnerId%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L103">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_OwnerId_" data-uid="DisCatSharp.Entities.DiscordGuild.OwnerId*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_OwnerId" data-uid="DisCatSharp.Entities.DiscordGuild.OwnerId">OwnerId</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the ID of the guild&apos;s owner.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public ulong OwnerId { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Permissions.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Permissions%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L118">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Permissions_" data-uid="DisCatSharp.Entities.DiscordGuild.Permissions*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Permissions" data-uid="DisCatSharp.Entities.DiscordGuild.Permissions">Permissions</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets permissions for the user in the guild (does not include channel overrides)</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Permissions? Permissions { get; set; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.Permissions.html">Permissions</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_PreferredLocale.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.PreferredLocale%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L97">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_PreferredLocale_" data-uid="DisCatSharp.Entities.DiscordGuild.PreferredLocale*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_PreferredLocale" data-uid="DisCatSharp.Entities.DiscordGuild.PreferredLocale">PreferredLocale</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Gets the preferred locale of this guild.
<p>This is used for server discovery, interactions and notices from Discord. Defaults to en-US.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string PreferredLocale { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_PremiumProgressBarEnabled.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.PremiumProgressBarEnabled%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L522">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_PremiumProgressBarEnabled_" data-uid="DisCatSharp.Entities.DiscordGuild.PremiumProgressBarEnabled*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_PremiumProgressBarEnabled" data-uid="DisCatSharp.Entities.DiscordGuild.PremiumProgressBarEnabled">PremiumProgressBarEnabled</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Whether the premium progress bar is enabled.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool PremiumProgressBarEnabled { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_PremiumSubscriptionCount.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.PremiumSubscriptionCount%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L516">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_PremiumSubscriptionCount_" data-uid="DisCatSharp.Entities.DiscordGuild.PremiumSubscriptionCount*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_PremiumSubscriptionCount" data-uid="DisCatSharp.Entities.DiscordGuild.PremiumSubscriptionCount">PremiumSubscriptionCount</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the amount of members that boosted this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public int? PremiumSubscriptionCount { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_PremiumTier.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.PremiumTier%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L510">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_PremiumTier_" data-uid="DisCatSharp.Entities.DiscordGuild.PremiumTier*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_PremiumTier" data-uid="DisCatSharp.Entities.DiscordGuild.PremiumTier">PremiumTier</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s premium tier (Nitro boosting).</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public PremiumTier PremiumTier { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.PremiumTier.html">PremiumTier</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_PublicUpdatesChannel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.PublicUpdatesChannel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L252">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_PublicUpdatesChannel_" data-uid="DisCatSharp.Entities.DiscordGuild.PublicUpdatesChannel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_PublicUpdatesChannel" data-uid="DisCatSharp.Entities.DiscordGuild.PublicUpdatesChannel">PublicUpdatesChannel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Gets the public updates channel (where admins and moderators receive messages from Discord) for this guild.
<p>This is only available if the guild is considered &quot;discoverable&quot;.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordChannel PublicUpdatesChannel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_RawFeatures.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.RawFeatures%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L296">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_RawFeatures_" data-uid="DisCatSharp.Entities.DiscordGuild.RawFeatures*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_RawFeatures" data-uid="DisCatSharp.Entities.DiscordGuild.RawFeatures">RawFeatures</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a collection of this guild&apos;s features.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyList&lt;string&gt; RawFeatures { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_RegisteredApplicationCommands.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.RegisteredApplicationCommands%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L151">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_RegisteredApplicationCommands_" data-uid="DisCatSharp.Entities.DiscordGuild.RegisteredApplicationCommands*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_RegisteredApplicationCommands" data-uid="DisCatSharp.Entities.DiscordGuild.RegisteredApplicationCommands">RegisteredApplicationCommands</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">List of <a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a>.
Null if DisCatSharp.ApplicationCommands is not used or no guild commands are registered.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public ReadOnlyCollection&lt;DiscordApplicationCommand&gt; RegisteredApplicationCommands { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">ReadOnlyCollection</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Roles.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Roles%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L266">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Roles_" data-uid="DisCatSharp.Entities.DiscordGuild.Roles*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Roles" data-uid="DisCatSharp.Entities.DiscordGuild.Roles">Roles</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a collection of this guild&apos;s roles.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordRole&gt; Roles { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_RulesChannel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.RulesChannel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L237">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_RulesChannel_" data-uid="DisCatSharp.Entities.DiscordGuild.RulesChannel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_RulesChannel" data-uid="DisCatSharp.Entities.DiscordGuild.RulesChannel">RulesChannel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Gets the rules channel for this guild.
<p>This is only available if the guild is considered &quot;discoverable&quot;.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordChannel RulesChannel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ScheduledEvents.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ScheduledEvents%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L427">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ScheduledEvents_" data-uid="DisCatSharp.Entities.DiscordGuild.ScheduledEvents*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ScheduledEvents" data-uid="DisCatSharp.Entities.DiscordGuild.ScheduledEvents">ScheduledEvents</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a dictionary of all scheduled events.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordScheduledEvent&gt; ScheduledEvents { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_SplashHash.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.SplashHash%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L70">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_SplashHash_" data-uid="DisCatSharp.Entities.DiscordGuild.SplashHash*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_SplashHash" data-uid="DisCatSharp.Entities.DiscordGuild.SplashHash">SplashHash</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild splash&apos;s hash.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string SplashHash { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_SplashUrl.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.SplashUrl%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L76">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_SplashUrl_" data-uid="DisCatSharp.Entities.DiscordGuild.SplashUrl*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_SplashUrl" data-uid="DisCatSharp.Entities.DiscordGuild.SplashUrl">SplashUrl</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild splash&apos;s url.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string SplashUrl { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_StageInstances.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.StageInstances%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L417">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_StageInstances_" data-uid="DisCatSharp.Entities.DiscordGuild.StageInstances*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_StageInstances" data-uid="DisCatSharp.Entities.DiscordGuild.StageInstances">StageInstances</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a dictionary of all active stage instances. The dictionary&apos;s key is the stage ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordStageInstance&gt; StageInstances { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordStageInstance.html">DiscordStageInstance</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Stickers.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Stickers%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L276">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Stickers_" data-uid="DisCatSharp.Entities.DiscordGuild.Stickers*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Stickers" data-uid="DisCatSharp.Entities.DiscordGuild.Stickers">Stickers</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a collection of this guild&apos;s stickers.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordSticker&gt; Stickers { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_SystemChannel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.SystemChannel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L196">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_SystemChannel_" data-uid="DisCatSharp.Entities.DiscordGuild.SystemChannel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_SystemChannel" data-uid="DisCatSharp.Entities.DiscordGuild.SystemChannel">SystemChannel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the channel where system messages (such as boost and welcome messages) are sent.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordChannel SystemChannel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_SystemChannelFlags.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.SystemChannelFlags%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L204">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_SystemChannelFlags_" data-uid="DisCatSharp.Entities.DiscordGuild.SystemChannelFlags*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_SystemChannelFlags" data-uid="DisCatSharp.Entities.DiscordGuild.SystemChannelFlags">SystemChannelFlags</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the settings for this guild&apos;s system channel.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public SystemChannelFlags SystemChannelFlags { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.SystemChannelFlags.html">SystemChannelFlags</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Threads.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Threads%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L407">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Threads_" data-uid="DisCatSharp.Entities.DiscordGuild.Threads*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Threads" data-uid="DisCatSharp.Entities.DiscordGuild.Threads">Threads</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a dictionary of all the active threads associated with this guild the user has permission to view. The dictionary&apos;s key is the channel ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordThreadChannel&gt; Threads { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordThreadChannel.html">DiscordThreadChannel</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_VanityUrlCode.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.VanityUrlCode%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L467">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_VanityUrlCode_" data-uid="DisCatSharp.Entities.DiscordGuild.VanityUrlCode*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_VanityUrlCode" data-uid="DisCatSharp.Entities.DiscordGuild.VanityUrlCode">VanityUrlCode</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the vanity URL code for this guild, when applicable.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string VanityUrlCode { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_VerificationLevel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.VerificationLevel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L166">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_VerificationLevel_" data-uid="DisCatSharp.Entities.DiscordGuild.VerificationLevel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_VerificationLevel" data-uid="DisCatSharp.Entities.DiscordGuild.VerificationLevel">VerificationLevel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s verification level.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public VerificationLevel VerificationLevel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.VerificationLevel.html">VerificationLevel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_VoiceRegion.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.VoiceRegion%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L130">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_VoiceRegion_" data-uid="DisCatSharp.Entities.DiscordGuild.VoiceRegion*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_VoiceRegion" data-uid="DisCatSharp.Entities.DiscordGuild.VoiceRegion">VoiceRegion</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s voice region.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordVoiceRegion VoiceRegion { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordVoiceRegion.html">DiscordVoiceRegion</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_VoiceStates.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.VoiceStates%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L375">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_VoiceStates_" data-uid="DisCatSharp.Entities.DiscordGuild.VoiceStates*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_VoiceStates" data-uid="DisCatSharp.Entities.DiscordGuild.VoiceStates">VoiceStates</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Gets a dictionary of all the voice states for this guilds. The key for this dictionary is the ID of the user
the voice state corresponds to.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public IReadOnlyDictionary&lt;ulong, DiscordVoiceState&gt; VoiceStates { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordVoiceState.html">DiscordVoiceState</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_WidgetChannel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.WidgetChannel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L222">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_WidgetChannel_" data-uid="DisCatSharp.Entities.DiscordGuild.WidgetChannel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_WidgetChannel" data-uid="DisCatSharp.Entities.DiscordGuild.WidgetChannel">WidgetChannel</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the widget channel for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordChannel WidgetChannel { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_WidgetEnabled.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.WidgetEnabled%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L210">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_WidgetEnabled_" data-uid="DisCatSharp.Entities.DiscordGuild.WidgetEnabled*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_WidgetEnabled" data-uid="DisCatSharp.Entities.DiscordGuild.WidgetEnabled">WidgetEnabled</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets whether this guild&apos;s widget is enabled.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool? WidgetEnabled { get; }</code></pre>
  </div>
  <h5 class="propertyValue">Property Value</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h3 id="methods">Methods
</h3>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_AddMemberAsync_DisCatSharp_Entities_DiscordUser_System_String_System_String_IEnumerable_DisCatSharp_Entities_DiscordRole__System_Boolean_System_Boolean_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.AddMemberAsync(DisCatSharp.Entities.DiscordUser%2CSystem.String%2CSystem.String%2CIEnumerable%7BDisCatSharp.Entities.DiscordRole%7D%2CSystem.Boolean%2CSystem.Boolean)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L686">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_AddMemberAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.AddMemberAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_AddMemberAsync_DisCatSharp_Entities_DiscordUser_System_String_System_String_IEnumerable_DisCatSharp_Entities_DiscordRole__System_Boolean_System_Boolean_" data-uid="DisCatSharp.Entities.DiscordGuild.AddMemberAsync(DisCatSharp.Entities.DiscordUser,System.String,System.String,IEnumerable{DisCatSharp.Entities.DiscordRole},System.Boolean,System.Boolean)">AddMemberAsync(DiscordUser, String, String, IEnumerable&lt;DiscordRole&gt;, Boolean, Boolean)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Adds a new member to this guild</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task AddMemberAsync(DiscordUser user, string accessToken, string nickname = null, IEnumerable&lt;DiscordRole&gt; roles = null, bool muted = false, bool deaf = false)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordUser.html">DiscordUser</a></td>
        <td><span class="parametername">user</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">User to add</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">accessToken</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">User&apos;s access token (OAuth2)</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">nickname</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">new nickname</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a>&gt;</td>
        <td><span class="parametername">roles</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">new roles</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><span class="parametername">muted</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">whether this user has to be muted</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><span class="parametername">deaf</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">whether this user has to be deafened</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_CreateInstantInvite">CreateInstantInvite</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the <code data-dev-comment-type="paramref" class="paramref">user</code> or <code data-dev-comment-type="paramref" class="paramref">accessToken</code> is not found.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_AttachUserIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.AttachUserIntegrationAsync(DisCatSharp.Entities.DiscordIntegration)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1417">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_AttachUserIntegrationAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.AttachUserIntegrationAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_AttachUserIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_" data-uid="DisCatSharp.Entities.DiscordGuild.AttachUserIntegrationAsync(DisCatSharp.Entities.DiscordIntegration)">AttachUserIntegrationAsync(DiscordIntegration)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Attaches an integration from current user to this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordIntegration&gt; AttachUserIntegrationAsync(DiscordIntegration integration)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordIntegration.html">DiscordIntegration</a></td>
        <td><span class="parametername">integration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Integration to attach.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordIntegration.html">DiscordIntegration</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The integration after being attached to the guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_BanMemberAsync_DisCatSharp_Entities_DiscordMember_System_Int32_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.BanMemberAsync(DisCatSharp.Entities.DiscordMember%2CSystem.Int32%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L910">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_BanMemberAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.BanMemberAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_BanMemberAsync_DisCatSharp_Entities_DiscordMember_System_Int32_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.BanMemberAsync(DisCatSharp.Entities.DiscordMember,System.Int32,System.String)">BanMemberAsync(DiscordMember, Int32, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Bans a specified member from this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task BanMemberAsync(DiscordMember member, int deleteMessageDays = 0, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a></td>
        <td><span class="parametername">member</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Member to ban.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><span class="parametername">deleteMessageDays</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">How many days to remove messages from.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_BanMembers">BanMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the member does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_BanMemberAsync_System_UInt64_System_Int32_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.BanMemberAsync(System.UInt64%2CSystem.Int32%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L923">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_BanMemberAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.BanMemberAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_BanMemberAsync_System_UInt64_System_Int32_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.BanMemberAsync(System.UInt64,System.Int32,System.String)">BanMemberAsync(UInt64, Int32, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Bans a specified user by ID. This doesn&apos;t require the user to be in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task BanMemberAsync(ulong userId, int deleteMessageDays = 0, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">userId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">ID of the user to ban.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><span class="parametername">deleteMessageDays</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">How many days to remove messages from.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_BanMembers">BanMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the member does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_BulkOverwriteApplicationCommandsAsync_IEnumerable_DisCatSharp_Entities_DiscordApplicationCommand__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.BulkOverwriteApplicationCommandsAsync(IEnumerable%7BDisCatSharp.Entities.DiscordApplicationCommand%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2053">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_BulkOverwriteApplicationCommandsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.BulkOverwriteApplicationCommandsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_BulkOverwriteApplicationCommandsAsync_IEnumerable_DisCatSharp_Entities_DiscordApplicationCommand__" data-uid="DisCatSharp.Entities.DiscordGuild.BulkOverwriteApplicationCommandsAsync(IEnumerable{DisCatSharp.Entities.DiscordApplicationCommand})">BulkOverwriteApplicationCommandsAsync(IEnumerable&lt;DiscordApplicationCommand&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Overwrites the existing application commands in this guild. New commands are automatically created and missing commands are automatically delete</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordApplicationCommand&gt;&gt; BulkOverwriteApplicationCommandsAsync(IEnumerable&lt;DiscordApplicationCommand&gt; commands)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a>&gt;</td>
        <td><span class="parametername">commands</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The list of commands to overwrite with.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The list of guild commands</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateApplicationCommandAsync_DisCatSharp_Entities_DiscordApplicationCommand_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateApplicationCommandAsync(DisCatSharp.Entities.DiscordApplicationCommand)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2061">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateApplicationCommandAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateApplicationCommandAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateApplicationCommandAsync_DisCatSharp_Entities_DiscordApplicationCommand_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateApplicationCommandAsync(DisCatSharp.Entities.DiscordApplicationCommand)">CreateApplicationCommandAsync(DiscordApplicationCommand)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates or overwrites a application command in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordApplicationCommand&gt; CreateApplicationCommandAsync(DiscordApplicationCommand command)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a></td>
        <td><span class="parametername">command</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The command to create.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The created command.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateAutomodRuleAsync_System_String_DisCatSharp_Enums_AutomodEventType_DisCatSharp_Enums_AutomodTriggerType_IEnumerable_DisCatSharp_Entities_AutomodAction__DisCatSharp_Entities_AutomodTriggerMetadata_System_Boolean_IEnumerable_System_UInt64__IEnumerable_System_UInt64__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateAutomodRuleAsync(System.String%2CDisCatSharp.Enums.AutomodEventType%2CDisCatSharp.Enums.AutomodTriggerType%2CIEnumerable%7BDisCatSharp.Entities.AutomodAction%7D%2CDisCatSharp.Entities.AutomodTriggerMetadata%2CSystem.Boolean%2CIEnumerable%7BSystem.UInt64%7D%2CIEnumerable%7BSystem.UInt64%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1057">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateAutomodRuleAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateAutomodRuleAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateAutomodRuleAsync_System_String_DisCatSharp_Enums_AutomodEventType_DisCatSharp_Enums_AutomodTriggerType_IEnumerable_DisCatSharp_Entities_AutomodAction__DisCatSharp_Entities_AutomodTriggerMetadata_System_Boolean_IEnumerable_System_UInt64__IEnumerable_System_UInt64__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateAutomodRuleAsync(System.String,DisCatSharp.Enums.AutomodEventType,DisCatSharp.Enums.AutomodTriggerType,IEnumerable{DisCatSharp.Entities.AutomodAction},DisCatSharp.Entities.AutomodTriggerMetadata,System.Boolean,IEnumerable{System.UInt64},IEnumerable{System.UInt64},System.String)">CreateAutomodRuleAsync(String, AutomodEventType, AutomodTriggerType, IEnumerable&lt;AutomodAction&gt;, AutomodTriggerMetadata, Boolean, IEnumerable&lt;UInt64&gt;, IEnumerable&lt;UInt64&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new auto mod rule in a guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;AutomodRule&gt; CreateAutomodRuleAsync(string name, AutomodEventType eventType, AutomodTriggerType triggerType, IEnumerable&lt;AutomodAction&gt; actions, AutomodTriggerMetadata triggerMetadata = null, bool enabled = false, IEnumerable&lt;ulong&gt; exemptRoles = null, IEnumerable&lt;ulong&gt; exemptChannels = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The name of the rule.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.AutomodEventType.html">AutomodEventType</a></td>
        <td><span class="parametername">eventType</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The event type of the rule.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.AutomodTriggerType.html">AutomodTriggerType</a></td>
        <td><span class="parametername">triggerType</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The trigger type of the rule.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.AutomodAction.html">AutomodAction</a>&gt;</td>
        <td><span class="parametername">actions</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The actions of the rule.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.AutomodTriggerMetadata.html">AutomodTriggerMetadata</a></td>
        <td><span class="parametername">triggerMetadata</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The meta data of the rule.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><span class="parametername">enabled</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether this rule is enabled.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>&gt;</td>
        <td><span class="parametername">exemptRoles</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The exempt roles of the rule.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>&gt;</td>
        <td><span class="parametername">exemptChannels</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The exempt channels of the rule.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The reason for this addition</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.AutomodRule.html">AutomodRule</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The created rule.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateChannelAsync_System_String_DisCatSharp_Enums_ChannelType_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_Optional_System_String__System_Nullable_System_Int32__System_Nullable_System_Int32__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_System_Boolean__DisCatSharp_Entities_Optional_System_Nullable_System_Int32___System_Nullable_DisCatSharp_Enums_VideoQualityMode__System_Nullable_DisCatSharp_Enums_ThreadAutoArchiveDuration__DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateChannelAsync(System.String%2CDisCatSharp.Enums.ChannelType%2CDisCatSharp.Entities.DiscordChannel%2CDisCatSharp.Entities.Optional%7BSystem.String%7D%2CSystem.Nullable%7BSystem.Int32%7D%2CSystem.Nullable%7BSystem.Int32%7D%2CIEnumerable%7BDisCatSharp.Entities.DiscordOverwriteBuilder%7D%2CSystem.Nullable%7BSystem.Boolean%7D%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BSystem.Int32%7D%7D%2CSystem.Nullable%7BDisCatSharp.Enums.VideoQualityMode%7D%2CSystem.Nullable%7BDisCatSharp.Enums.ThreadAutoArchiveDuration%7D%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BDisCatSharp.Enums.ChannelFlags%7D%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1316">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateChannelAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateChannelAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateChannelAsync_System_String_DisCatSharp_Enums_ChannelType_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_Optional_System_String__System_Nullable_System_Int32__System_Nullable_System_Int32__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_System_Boolean__DisCatSharp_Entities_Optional_System_Nullable_System_Int32___System_Nullable_DisCatSharp_Enums_VideoQualityMode__System_Nullable_DisCatSharp_Enums_ThreadAutoArchiveDuration__DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateChannelAsync(System.String,DisCatSharp.Enums.ChannelType,DisCatSharp.Entities.DiscordChannel,DisCatSharp.Entities.Optional{System.String},System.Nullable{System.Int32},System.Nullable{System.Int32},IEnumerable{DisCatSharp.Entities.DiscordOverwriteBuilder},System.Nullable{System.Boolean},DisCatSharp.Entities.Optional{System.Nullable{System.Int32}},System.Nullable{DisCatSharp.Enums.VideoQualityMode},System.Nullable{DisCatSharp.Enums.ThreadAutoArchiveDuration},DisCatSharp.Entities.Optional{System.Nullable{DisCatSharp.Enums.ChannelFlags}},System.String)">CreateChannelAsync(String, ChannelType, DiscordChannel, Optional&lt;String&gt;, Nullable&lt;Int32&gt;, Nullable&lt;Int32&gt;, IEnumerable&lt;DiscordOverwriteBuilder&gt;, Nullable&lt;Boolean&gt;, Optional&lt;Nullable&lt;Int32&gt;&gt;, Nullable&lt;VideoQualityMode&gt;, Nullable&lt;ThreadAutoArchiveDuration&gt;, Optional&lt;Nullable&lt;ChannelFlags&gt;&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new channel in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordChannel&gt; CreateChannelAsync(string name, ChannelType type, DiscordChannel parent = null, Optional&lt;string&gt; topic = default(Optional&lt;string&gt;), int? bitrate = null, int? userLimit = null, IEnumerable&lt;DiscordOverwriteBuilder&gt; overwrites = null, bool? nsfw = null, Optional&lt;int?&gt; perUserRateLimit = default(Optional&lt;int?&gt;), VideoQualityMode? qualityMode = null, ThreadAutoArchiveDuration? defaultAutoArchiveDuration = null, Optional&lt;ChannelFlags?&gt; flags = default(Optional&lt;ChannelFlags?&gt;), string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.ChannelType.html">ChannelType</a></td>
        <td><span class="parametername">type</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Type of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">parent</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Category to put this channel in.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td><span class="parametername">topic</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Topic of the channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><span class="parametername">bitrate</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Bitrate of the channel. Applies to voice only.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><span class="parametername">userLimit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Maximum number of users in the channel. Applies to voice only.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordOverwriteBuilder.html">DiscordOverwriteBuilder</a>&gt;</td>
        <td><span class="parametername">overwrites</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permission overwrites for this channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">nsfw</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the channel is to be flagged as not safe for work. Applies to text only.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;&gt;</td>
        <td><span class="parametername">perUserRateLimit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Slow mode timeout for users.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.VideoQualityMode.html">VideoQualityMode</a>&gt;</td>
        <td><span class="parametername">qualityMode</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Video quality mode of the channel. Applies to voice only.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.ThreadAutoArchiveDuration.html">ThreadAutoArchiveDuration</a>&gt;</td>
        <td><span class="parametername">defaultAutoArchiveDuration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The default auto archive duration for new threads.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.ChannelFlags.html">ChannelFlags</a>&gt;&gt;</td>
        <td><span class="parametername">flags</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The flags of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateChannelCategoryAsync_System_String_IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateChannelCategoryAsync(System.String%2CIEnumerable%7BDisCatSharp.Entities.DiscordOverwriteBuilder%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1241">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateChannelCategoryAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateChannelCategoryAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateChannelCategoryAsync_System_String_IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateChannelCategoryAsync(System.String,IEnumerable{DisCatSharp.Entities.DiscordOverwriteBuilder},System.String)">CreateChannelCategoryAsync(String, IEnumerable&lt;DiscordOverwriteBuilder&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new channel category in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordChannel&gt; CreateChannelCategoryAsync(string name, IEnumerable&lt;DiscordOverwriteBuilder&gt; overwrites = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new category.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordOverwriteBuilder.html">DiscordOverwriteBuilder</a>&gt;</td>
        <td><span class="parametername">overwrites</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permission overwrites for this category.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created channel category.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateEmojiAsync_System_String_Stream_IEnumerable_DisCatSharp_Entities_DiscordRole__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateEmojiAsync(System.String%2CStream%2CIEnumerable%7BDisCatSharp.Entities.DiscordRole%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1718">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateEmojiAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateEmojiAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateEmojiAsync_System_String_Stream_IEnumerable_DisCatSharp_Entities_DiscordRole__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateEmojiAsync(System.String,Stream,IEnumerable{DisCatSharp.Entities.DiscordRole},System.String)">CreateEmojiAsync(String, Stream, IEnumerable&lt;DiscordRole&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new custom emoji for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildEmoji&gt; CreateEmojiAsync(string name, Stream image, IEnumerable&lt;DiscordRole&gt; roles = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new emoji.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">Stream</span></td>
        <td><span class="parametername">image</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Image to use as the emoji.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a>&gt;</td>
        <td><span class="parametername">roles</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit log.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildEmoji.html">DiscordGuildEmoji</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created emoji.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateExternalScheduledEventAsync_System_String_DateTimeOffset_DateTimeOffset_System_String_System_String_DisCatSharp_Entities_Optional_Stream__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateExternalScheduledEventAsync(System.String%2CDateTimeOffset%2CDateTimeOffset%2CSystem.String%2CSystem.String%2CDisCatSharp.Entities.Optional%7BStream%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1099">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateExternalScheduledEventAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateExternalScheduledEventAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateExternalScheduledEventAsync_System_String_DateTimeOffset_DateTimeOffset_System_String_System_String_DisCatSharp_Entities_Optional_Stream__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateExternalScheduledEventAsync(System.String,DateTimeOffset,DateTimeOffset,System.String,System.String,DisCatSharp.Entities.Optional{Stream},System.String)">CreateExternalScheduledEventAsync(String, DateTimeOffset, DateTimeOffset, String, String, Optional&lt;Stream&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a scheduled event with type <a class="xref" href="DisCatSharp.Enums.ScheduledEventEntityType.html#DisCatSharp_Enums_ScheduledEventEntityType_External">External</a>.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordScheduledEvent&gt; CreateExternalScheduledEventAsync(string name, DateTimeOffset scheduledStartTime, DateTimeOffset scheduledEndTime, string location, string description = null, Optional&lt;Stream&gt; coverImage = default(Optional&lt;Stream&gt;), string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The name.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">DateTimeOffset</span></td>
        <td><span class="parametername">scheduledStartTime</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The scheduled start time.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">DateTimeOffset</span></td>
        <td><span class="parametername">scheduledEndTime</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The scheduled end time.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">location</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The location of the external event.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The description.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<span class="xref">Stream</span>&gt;</td>
        <td><span class="parametername">coverImage</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The cover image.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The reason.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A scheduled event.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateForumChannelAsync_System_String_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_Optional_System_String__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_System_Boolean__DisCatSharp_Entities_Optional_DisCatSharp_Entities_ForumReactionEmoji__DisCatSharp_Entities_Optional_System_Nullable_System_Int32___DisCatSharp_Entities_Optional_System_Nullable_System_Int32___DisCatSharp_Enums_ThreadAutoArchiveDuration_DisCatSharp_Entities_Optional_DisCatSharp_Enums_ForumPostSortOrder__DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateForumChannelAsync(System.String%2CDisCatSharp.Entities.DiscordChannel%2CDisCatSharp.Entities.Optional%7BSystem.String%7D%2CIEnumerable%7BDisCatSharp.Entities.DiscordOverwriteBuilder%7D%2CSystem.Nullable%7BSystem.Boolean%7D%2CDisCatSharp.Entities.Optional%7BDisCatSharp.Entities.ForumReactionEmoji%7D%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BSystem.Int32%7D%7D%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BSystem.Int32%7D%7D%2CDisCatSharp.Enums.ThreadAutoArchiveDuration%2CDisCatSharp.Entities.Optional%7BDisCatSharp.Enums.ForumPostSortOrder%7D%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BDisCatSharp.Enums.ChannelFlags%7D%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1227">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateForumChannelAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateForumChannelAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateForumChannelAsync_System_String_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_Optional_System_String__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_System_Boolean__DisCatSharp_Entities_Optional_DisCatSharp_Entities_ForumReactionEmoji__DisCatSharp_Entities_Optional_System_Nullable_System_Int32___DisCatSharp_Entities_Optional_System_Nullable_System_Int32___DisCatSharp_Enums_ThreadAutoArchiveDuration_DisCatSharp_Entities_Optional_DisCatSharp_Enums_ForumPostSortOrder__DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateForumChannelAsync(System.String,DisCatSharp.Entities.DiscordChannel,DisCatSharp.Entities.Optional{System.String},IEnumerable{DisCatSharp.Entities.DiscordOverwriteBuilder},System.Nullable{System.Boolean},DisCatSharp.Entities.Optional{DisCatSharp.Entities.ForumReactionEmoji},DisCatSharp.Entities.Optional{System.Nullable{System.Int32}},DisCatSharp.Entities.Optional{System.Nullable{System.Int32}},DisCatSharp.Enums.ThreadAutoArchiveDuration,DisCatSharp.Entities.Optional{DisCatSharp.Enums.ForumPostSortOrder},DisCatSharp.Entities.Optional{System.Nullable{DisCatSharp.Enums.ChannelFlags}},System.String)">CreateForumChannelAsync(String, DiscordChannel, Optional&lt;String&gt;, IEnumerable&lt;DiscordOverwriteBuilder&gt;, Nullable&lt;Boolean&gt;, Optional&lt;ForumReactionEmoji&gt;, Optional&lt;Nullable&lt;Int32&gt;&gt;, Optional&lt;Nullable&lt;Int32&gt;&gt;, ThreadAutoArchiveDuration, Optional&lt;ForumPostSortOrder&gt;, Optional&lt;Nullable&lt;ChannelFlags&gt;&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Creates a new forum channel in this guild.
<div class="NOTE"><h5>note</h5><p>The field template is not yet released, so it won&apos;t applied.</p></div>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordChannel&gt; CreateForumChannelAsync(string name, DiscordChannel parent = null, Optional&lt;string&gt; topic = default(Optional&lt;string&gt;), IEnumerable&lt;DiscordOverwriteBuilder&gt; overwrites = null, bool? nsfw = null, Optional&lt;ForumReactionEmoji&gt; defaultReactionEmoji = default(Optional&lt;ForumReactionEmoji&gt;), Optional&lt;int?&gt; perUserRateLimit = default(Optional&lt;int?&gt;), Optional&lt;int?&gt; postCreateUserRateLimit = default(Optional&lt;int?&gt;), ThreadAutoArchiveDuration defaultAutoArchiveDuration = default(ThreadAutoArchiveDuration), Optional&lt;ForumPostSortOrder&gt; defaultSortOrder = default(Optional&lt;ForumPostSortOrder&gt;), Optional&lt;ChannelFlags?&gt; flags = default(Optional&lt;ChannelFlags?&gt;), string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">parent</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Category to put this channel in.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td><span class="parametername">topic</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Topic of the channel.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordOverwriteBuilder.html">DiscordOverwriteBuilder</a>&gt;</td>
        <td><span class="parametername">overwrites</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permission overwrites for this channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">nsfw</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the channel is to be flagged as not safe for work.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="DisCatSharp.Entities.ForumReactionEmoji.html">ForumReactionEmoji</a>&gt;</td>
        <td><span class="parametername">defaultReactionEmoji</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The default reaction emoji for posts.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;&gt;</td>
        <td><span class="parametername">perUserRateLimit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Slow mode timeout for users.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;&gt;</td>
        <td><span class="parametername">postCreateUserRateLimit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Slow mode timeout for user post creations.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.ThreadAutoArchiveDuration.html">ThreadAutoArchiveDuration</a></td>
        <td><span class="parametername">defaultAutoArchiveDuration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The default auto archive duration for new threads.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="DisCatSharp.Enums.ForumPostSortOrder.html">ForumPostSortOrder</a>&gt;</td>
        <td><span class="parametername">defaultSortOrder</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The default sort order for posts in the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.ChannelFlags.html">ChannelFlags</a>&gt;&gt;</td>
        <td><span class="parametername">flags</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The flags of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a> permission or the guild does not have the forum channel feature.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateNewsChannelAsync_System_String_IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_String_DisCatSharp_Enums_ThreadAutoArchiveDuration_DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateNewsChannelAsync(System.String%2CIEnumerable%7BDisCatSharp.Entities.DiscordOverwriteBuilder%7D%2CSystem.String%2CDisCatSharp.Enums.ThreadAutoArchiveDuration%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BDisCatSharp.Enums.ChannelFlags%7D%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1273">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateNewsChannelAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateNewsChannelAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateNewsChannelAsync_System_String_IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_String_DisCatSharp_Enums_ThreadAutoArchiveDuration_DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___" data-uid="DisCatSharp.Entities.DiscordGuild.CreateNewsChannelAsync(System.String,IEnumerable{DisCatSharp.Entities.DiscordOverwriteBuilder},System.String,DisCatSharp.Enums.ThreadAutoArchiveDuration,DisCatSharp.Entities.Optional{System.Nullable{DisCatSharp.Enums.ChannelFlags}})">CreateNewsChannelAsync(String, IEnumerable&lt;DiscordOverwriteBuilder&gt;, String, ThreadAutoArchiveDuration, Optional&lt;Nullable&lt;ChannelFlags&gt;&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new news channel in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordChannel&gt; CreateNewsChannelAsync(string name, IEnumerable&lt;DiscordOverwriteBuilder&gt; overwrites = null, string reason = null, ThreadAutoArchiveDuration defaultAutoArchiveDuration = default(ThreadAutoArchiveDuration), Optional&lt;ChannelFlags?&gt; flags = default(Optional&lt;ChannelFlags?&gt;))</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new news channel.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordOverwriteBuilder.html">DiscordOverwriteBuilder</a>&gt;</td>
        <td><span class="parametername">overwrites</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permission overwrites for this news channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.ThreadAutoArchiveDuration.html">ThreadAutoArchiveDuration</a></td>
        <td><span class="parametername">defaultAutoArchiveDuration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The default auto archive duration for new threads.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.ChannelFlags.html">ChannelFlags</a>&gt;&gt;</td>
        <td><span class="parametername">flags</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The flags of the new channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created news channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a>.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateRoleAsync_System_String_System_Nullable_DisCatSharp_Enums_Permissions__System_Nullable_DisCatSharp_Entities_DiscordColor__System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateRoleAsync(System.String%2CSystem.Nullable%7BDisCatSharp.Enums.Permissions%7D%2CSystem.Nullable%7BDisCatSharp.Entities.DiscordColor%7D%2CSystem.Nullable%7BSystem.Boolean%7D%2CSystem.Nullable%7BSystem.Boolean%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1661">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateRoleAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateRoleAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateRoleAsync_System_String_System_Nullable_DisCatSharp_Enums_Permissions__System_Nullable_DisCatSharp_Entities_DiscordColor__System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateRoleAsync(System.String,System.Nullable{DisCatSharp.Enums.Permissions},System.Nullable{DisCatSharp.Entities.DiscordColor},System.Nullable{System.Boolean},System.Nullable{System.Boolean},System.String)">CreateRoleAsync(String, Nullable&lt;Permissions&gt;, Nullable&lt;DiscordColor&gt;, Nullable&lt;Boolean&gt;, Nullable&lt;Boolean&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new role in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordRole&gt; CreateRoleAsync(string name = null, Permissions? permissions = null, DiscordColor? color = null, bool? hoist = null, bool? mentionable = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the role.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.Permissions.html">Permissions</a>&gt;</td>
        <td><span class="parametername">permissions</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permissions for the role.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordColor.html">DiscordColor</a>&gt;</td>
        <td><span class="parametername">color</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Color for the role.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">hoist</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the role is to be hoisted.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">mentionable</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the role is to be mentionable.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created role.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageRoles">ManageRoles</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateScheduledEventAsync_System_String_DateTimeOffset_System_Nullable_DateTimeOffset__DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_DiscordScheduledEventEntityMetadata_System_String_DisCatSharp_Enums_ScheduledEventEntityType_DisCatSharp_Entities_Optional_Stream__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateScheduledEventAsync(System.String%2CDateTimeOffset%2CSystem.Nullable%7BDateTimeOffset%7D%2CDisCatSharp.Entities.DiscordChannel%2CDisCatSharp.Entities.DiscordScheduledEventEntityMetadata%2CSystem.String%2CDisCatSharp.Enums.ScheduledEventEntityType%2CDisCatSharp.Entities.Optional%7BStream%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1079">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateScheduledEventAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateScheduledEventAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateScheduledEventAsync_System_String_DateTimeOffset_System_Nullable_DateTimeOffset__DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_DiscordScheduledEventEntityMetadata_System_String_DisCatSharp_Enums_ScheduledEventEntityType_DisCatSharp_Entities_Optional_Stream__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateScheduledEventAsync(System.String,DateTimeOffset,System.Nullable{DateTimeOffset},DisCatSharp.Entities.DiscordChannel,DisCatSharp.Entities.DiscordScheduledEventEntityMetadata,System.String,DisCatSharp.Enums.ScheduledEventEntityType,DisCatSharp.Entities.Optional{Stream},System.String)">CreateScheduledEventAsync(String, DateTimeOffset, Nullable&lt;DateTimeOffset&gt;, DiscordChannel, DiscordScheduledEventEntityMetadata, String, ScheduledEventEntityType, Optional&lt;Stream&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a scheduled event.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordScheduledEvent&gt; CreateScheduledEventAsync(string name, DateTimeOffset scheduledStartTime, DateTimeOffset? scheduledEndTime = null, DiscordChannel channel = null, DiscordScheduledEventEntityMetadata metadata = null, string description = null, ScheduledEventEntityType type = default(ScheduledEventEntityType), Optional&lt;Stream&gt; coverImage = default(Optional&lt;Stream&gt;), string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The name.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">DateTimeOffset</span></td>
        <td><span class="parametername">scheduledStartTime</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The scheduled start time.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<span class="xref">DateTimeOffset</span>&gt;</td>
        <td><span class="parametername">scheduledEndTime</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The scheduled end time.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">channel</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordScheduledEventEntityMetadata.html">DiscordScheduledEventEntityMetadata</a></td>
        <td><span class="parametername">metadata</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The metadata.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The description.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.ScheduledEventEntityType.html">ScheduledEventEntityType</a></td>
        <td><span class="parametername">type</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The type.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<span class="xref">Stream</span>&gt;</td>
        <td><span class="parametername">coverImage</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The cover image.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The reason.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A scheduled event.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateStageChannelAsync_System_String_IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateStageChannelAsync(System.String%2CIEnumerable%7BDisCatSharp.Entities.DiscordOverwriteBuilder%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1256">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateStageChannelAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateStageChannelAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateStageChannelAsync_System_String_IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateStageChannelAsync(System.String,IEnumerable{DisCatSharp.Entities.DiscordOverwriteBuilder},System.String)">CreateStageChannelAsync(String, IEnumerable&lt;DiscordOverwriteBuilder&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new stage channel in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordChannel&gt; CreateStageChannelAsync(string name, IEnumerable&lt;DiscordOverwriteBuilder&gt; overwrites = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new stage channel.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordOverwriteBuilder.html">DiscordOverwriteBuilder</a>&gt;</td>
        <td><span class="parametername">overwrites</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permission overwrites for this stage channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created stage channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a>.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateStickerAsync_System_String_System_String_DisCatSharp_Entities_DiscordEmoji_Stream_DisCatSharp_Entities_StickerFormat_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateStickerAsync(System.String%2CSystem.String%2CDisCatSharp.Entities.DiscordEmoji%2CStream%2CDisCatSharp.Entities.StickerFormat%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1820">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateStickerAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateStickerAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateStickerAsync_System_String_System_String_DisCatSharp_Entities_DiscordEmoji_Stream_DisCatSharp_Entities_StickerFormat_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateStickerAsync(System.String,System.String,DisCatSharp.Entities.DiscordEmoji,Stream,DisCatSharp.Entities.StickerFormat,System.String)">CreateStickerAsync(String, String, DiscordEmoji, Stream, StickerFormat, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a sticker</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordSticker&gt; CreateStickerAsync(string name, string description, DiscordEmoji emoji, Stream file, StickerFormat format, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The name of the sticker.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The optional description of the sticker.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordEmoji.html">DiscordEmoji</a></td>
        <td><span class="parametername">emoji</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The emoji to associate the sticker with.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">Stream</span></td>
        <td><span class="parametername">file</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The sticker.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.StickerFormat.html">StickerFormat</a></td>
        <td><span class="parametername">format</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The file format the sticker is written in.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Audit log reason</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateTemplateAsync_System_String_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateTemplateAsync(System.String%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1981">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateTemplateAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateTemplateAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateTemplateAsync_System_String_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateTemplateAsync(System.String,System.String)">CreateTemplateAsync(String, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a guild template.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildTemplate&gt; CreateTemplateAsync(string name, string description = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the template.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Description of the template.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildTemplate.html">DiscordGuildTemplate</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The template created.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when a template already exists for the guild or a null parameter is provided for the name.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateTextChannelAsync_System_String_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_Optional_System_String__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_System_Boolean__DisCatSharp_Entities_Optional_System_Nullable_System_Int32___DisCatSharp_Enums_ThreadAutoArchiveDuration_DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateTextChannelAsync(System.String%2CDisCatSharp.Entities.DiscordChannel%2CDisCatSharp.Entities.Optional%7BSystem.String%7D%2CIEnumerable%7BDisCatSharp.Entities.DiscordOverwriteBuilder%7D%2CSystem.Nullable%7BSystem.Boolean%7D%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BSystem.Int32%7D%7D%2CDisCatSharp.Enums.ThreadAutoArchiveDuration%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BDisCatSharp.Enums.ChannelFlags%7D%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1203">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateTextChannelAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateTextChannelAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateTextChannelAsync_System_String_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_Optional_System_String__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_System_Boolean__DisCatSharp_Entities_Optional_System_Nullable_System_Int32___DisCatSharp_Enums_ThreadAutoArchiveDuration_DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateTextChannelAsync(System.String,DisCatSharp.Entities.DiscordChannel,DisCatSharp.Entities.Optional{System.String},IEnumerable{DisCatSharp.Entities.DiscordOverwriteBuilder},System.Nullable{System.Boolean},DisCatSharp.Entities.Optional{System.Nullable{System.Int32}},DisCatSharp.Enums.ThreadAutoArchiveDuration,DisCatSharp.Entities.Optional{System.Nullable{DisCatSharp.Enums.ChannelFlags}},System.String)">CreateTextChannelAsync(String, DiscordChannel, Optional&lt;String&gt;, IEnumerable&lt;DiscordOverwriteBuilder&gt;, Nullable&lt;Boolean&gt;, Optional&lt;Nullable&lt;Int32&gt;&gt;, ThreadAutoArchiveDuration, Optional&lt;Nullable&lt;ChannelFlags&gt;&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new text channel in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordChannel&gt; CreateTextChannelAsync(string name, DiscordChannel parent = null, Optional&lt;string&gt; topic = default(Optional&lt;string&gt;), IEnumerable&lt;DiscordOverwriteBuilder&gt; overwrites = null, bool? nsfw = null, Optional&lt;int?&gt; perUserRateLimit = default(Optional&lt;int?&gt;), ThreadAutoArchiveDuration defaultAutoArchiveDuration = default(ThreadAutoArchiveDuration), Optional&lt;ChannelFlags?&gt; flags = default(Optional&lt;ChannelFlags?&gt;), string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">parent</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Category to put this channel in.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td><span class="parametername">topic</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Topic of the channel.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordOverwriteBuilder.html">DiscordOverwriteBuilder</a>&gt;</td>
        <td><span class="parametername">overwrites</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permission overwrites for this channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">nsfw</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the channel is to be flagged as not safe for work.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;&gt;</td>
        <td><span class="parametername">perUserRateLimit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Slow mode timeout for users.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.ThreadAutoArchiveDuration.html">ThreadAutoArchiveDuration</a></td>
        <td><span class="parametername">defaultAutoArchiveDuration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The default auto archive duration for new threads.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.ChannelFlags.html">ChannelFlags</a>&gt;&gt;</td>
        <td><span class="parametername">flags</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The flags of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_CreateVoiceChannelAsync_System_String_DisCatSharp_Entities_DiscordChannel_System_Nullable_System_Int32__System_Nullable_System_Int32__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_DisCatSharp_Enums_VideoQualityMode__DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.CreateVoiceChannelAsync(System.String%2CDisCatSharp.Entities.DiscordChannel%2CSystem.Nullable%7BSystem.Int32%7D%2CSystem.Nullable%7BSystem.Int32%7D%2CIEnumerable%7BDisCatSharp.Entities.DiscordOverwriteBuilder%7D%2CSystem.Nullable%7BDisCatSharp.Enums.VideoQualityMode%7D%2CDisCatSharp.Entities.Optional%7BSystem.Nullable%7BDisCatSharp.Enums.ChannelFlags%7D%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1292">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_CreateVoiceChannelAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateVoiceChannelAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_CreateVoiceChannelAsync_System_String_DisCatSharp_Entities_DiscordChannel_System_Nullable_System_Int32__System_Nullable_System_Int32__IEnumerable_DisCatSharp_Entities_DiscordOverwriteBuilder__System_Nullable_DisCatSharp_Enums_VideoQualityMode__DisCatSharp_Entities_Optional_System_Nullable_DisCatSharp_Enums_ChannelFlags___System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.CreateVoiceChannelAsync(System.String,DisCatSharp.Entities.DiscordChannel,System.Nullable{System.Int32},System.Nullable{System.Int32},IEnumerable{DisCatSharp.Entities.DiscordOverwriteBuilder},System.Nullable{DisCatSharp.Enums.VideoQualityMode},DisCatSharp.Entities.Optional{System.Nullable{DisCatSharp.Enums.ChannelFlags}},System.String)">CreateVoiceChannelAsync(String, DiscordChannel, Nullable&lt;Int32&gt;, Nullable&lt;Int32&gt;, IEnumerable&lt;DiscordOverwriteBuilder&gt;, Nullable&lt;VideoQualityMode&gt;, Optional&lt;Nullable&lt;ChannelFlags&gt;&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Creates a new voice channel in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordChannel&gt; CreateVoiceChannelAsync(string name, DiscordChannel parent = null, int? bitrate = null, int? userLimit = null, IEnumerable&lt;DiscordOverwriteBuilder&gt; overwrites = null, VideoQualityMode? qualityMode = null, Optional&lt;ChannelFlags?&gt; flags = default(Optional&lt;ChannelFlags?&gt;), string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">parent</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Category to put this channel in.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><span class="parametername">bitrate</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Bitrate of the channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><span class="parametername">userLimit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Maximum number of users in the channel.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordOverwriteBuilder.html">DiscordOverwriteBuilder</a>&gt;</td>
        <td><span class="parametername">overwrites</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Permission overwrites for this channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.VideoQualityMode.html">VideoQualityMode</a>&gt;</td>
        <td><span class="parametername">qualityMode</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Video quality mode of the channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.ChannelFlags.html">ChannelFlags</a>&gt;&gt;</td>
        <td><span class="parametername">flags</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The flags of the new channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly-created channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DeleteAllChannelsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DeleteAllChannelsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1339">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DeleteAllChannelsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteAllChannelsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DeleteAllChannelsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteAllChannelsAsync">DeleteAllChannelsAsync()</h4>
  <div class="markdown level1 summary"><p>Deletes all channels in this guild.</p>
<p>Note that this is irreversible. Use carefully!</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task DeleteAllChannelsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DeleteAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DeleteAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L695">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DeleteAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DeleteAsync" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteAsync">DeleteAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Deletes this guild. Requires the caller to be the owner of the guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task DeleteAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client is not the owner of the guild.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DeleteEmojiAsync_DisCatSharp_Entities_DiscordGuildEmoji_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DeleteEmojiAsync(DisCatSharp.Entities.DiscordGuildEmoji%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1769">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DeleteEmojiAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteEmojiAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DeleteEmojiAsync_DisCatSharp_Entities_DiscordGuildEmoji_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteEmojiAsync(DisCatSharp.Entities.DiscordGuildEmoji,System.String)">DeleteEmojiAsync(DiscordGuildEmoji, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Deletes this guild&apos;s custom emoji.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task DeleteEmojiAsync(DiscordGuildEmoji emoji, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordGuildEmoji.html">DiscordGuildEmoji</a></td>
        <td><span class="parametername">emoji</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Emoji to delete.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit log.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DeleteIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DeleteIntegrationAsync(DisCatSharp.Entities.DiscordIntegration)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1443">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DeleteIntegrationAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteIntegrationAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DeleteIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteIntegrationAsync(DisCatSharp.Entities.DiscordIntegration)">DeleteIntegrationAsync(DiscordIntegration)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Removes an integration from this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task DeleteIntegrationAsync(DiscordIntegration integration)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordIntegration.html">DiscordIntegration</a></td>
        <td><span class="parametername">integration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Integration to remove.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DeleteStickerAsync_DisCatSharp_Entities_DiscordSticker_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DeleteStickerAsync(DisCatSharp.Entities.DiscordSticker%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1925">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DeleteStickerAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteStickerAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DeleteStickerAsync_DisCatSharp_Entities_DiscordSticker_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteStickerAsync(DisCatSharp.Entities.DiscordSticker,System.String)">DeleteStickerAsync(DiscordSticker, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Deletes a sticker</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task DeleteStickerAsync(DiscordSticker sticker, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a></td>
        <td><span class="parametername">sticker</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Sticker to delete</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Audit log reason</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the sticker could not be found.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DeleteStickerAsync_System_UInt64_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DeleteStickerAsync(System.UInt64%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1909">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DeleteStickerAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteStickerAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DeleteStickerAsync_System_UInt64_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteStickerAsync(System.UInt64,System.String)">DeleteStickerAsync(UInt64, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Deletes a sticker</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task DeleteStickerAsync(ulong sticker, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">sticker</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Id of sticker to delete</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Audit log reason</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the sticker could not be found.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DeleteTemplateAsync_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DeleteTemplateAsync(System.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2016">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DeleteTemplateAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteTemplateAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DeleteTemplateAsync_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.DeleteTemplateAsync(System.String)">DeleteTemplateAsync(String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Deletes the template.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildTemplate&gt; DeleteTemplateAsync(string code)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">code</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The code of the template to delete.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildTemplate.html">DiscordGuildTemplate</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The deleted template.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the template for the code cannot be found</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DisableInvitesAsync_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DisableInvitesAsync(System.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L836">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DisableInvitesAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DisableInvitesAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DisableInvitesAsync_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.DisableInvitesAsync(System.String)">DisableInvitesAsync(String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Disables invites for the guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordGuild&gt; DisableInvitesAsync(string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_DisableMfaAsync_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.DisableMfaAsync(System.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L716">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_DisableMfaAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.DisableMfaAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_DisableMfaAsync_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.DisableMfaAsync(System.String)">DisableMfaAsync(String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Disables the mfa requirement for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task DisableMfaAsync(string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The audit log reason.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the current user is not the guilds owner.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_EditApplicationCommandAsync_System_UInt64_Action_DisCatSharp_Net_Models_ApplicationCommandEditModel__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.EditApplicationCommandAsync(System.UInt64%2CAction%7BDisCatSharp.Net.Models.ApplicationCommandEditModel%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2070">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_EditApplicationCommandAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.EditApplicationCommandAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_EditApplicationCommandAsync_System_UInt64_Action_DisCatSharp_Net_Models_ApplicationCommandEditModel__" data-uid="DisCatSharp.Entities.DiscordGuild.EditApplicationCommandAsync(System.UInt64,Action{DisCatSharp.Net.Models.ApplicationCommandEditModel})">EditApplicationCommandAsync(UInt64, Action&lt;ApplicationCommandEditModel&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Edits a application command in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordApplicationCommand&gt; EditApplicationCommandAsync(ulong commandId, Action&lt;ApplicationCommandEditModel&gt; action)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">commandId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The id of the command to edit.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">Action</span>&lt;<a class="xref" href="DisCatSharp.Net.Models.ApplicationCommandEditModel.html">ApplicationCommandEditModel</a>&gt;</td>
        <td><span class="parametername">action</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Action to perform.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The edit command.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_EnableInvitesAsync_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.EnableInvitesAsync(System.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L816">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_EnableInvitesAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.EnableInvitesAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_EnableInvitesAsync_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.EnableInvitesAsync(System.String)">EnableInvitesAsync(String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Enables invites for the guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordGuild&gt; EnableInvitesAsync(string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The audit log reason.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_EnableMfaAsync_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.EnableMfaAsync(System.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L705">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_EnableMfaAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.EnableMfaAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_EnableMfaAsync_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.EnableMfaAsync(System.String)">EnableMfaAsync(String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Enables the mfa requirement for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task EnableMfaAsync(string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The audit log reason.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the current user is not the guilds owner.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Equals_DisCatSharp_Entities_DiscordGuild_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Equals(DisCatSharp.Entities.DiscordGuild)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2120">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Equals_" data-uid="DisCatSharp.Entities.DiscordGuild.Equals*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Equals_DisCatSharp_Entities_DiscordGuild_" data-uid="DisCatSharp.Entities.DiscordGuild.Equals(DisCatSharp.Entities.DiscordGuild)">Equals(DiscordGuild)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Checks whether this <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a> is equal to another <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public bool Equals(DiscordGuild e)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a></td>
        <td><span class="parametername">e</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1"><a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a> to compare to.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a> is equal to this <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_Equals_System_Object_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.Equals(System.Object)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2112">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_Equals_" data-uid="DisCatSharp.Entities.DiscordGuild.Equals*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_Equals_System_Object_" data-uid="DisCatSharp.Entities.DiscordGuild.Equals(System.Object)">Equals(Object)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Checks whether this <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a> is equal to another object.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public override bool Equals(object obj)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.object">Object</a></td>
        <td><span class="parametername">obj</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Object to compare to.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the object is equal to this <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetActiveThreadsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetActiveThreadsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1331">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetActiveThreadsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetActiveThreadsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetActiveThreadsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetActiveThreadsAsync">GetActiveThreadsAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Gets active threads. Can contain more threads.
If the result&apos;s value &apos;HasMore&apos; is true, you need to recall this function to get older threads.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordThreadResult&gt; GetActiveThreadsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordThreadResult.html">DiscordThreadResult</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the thread does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetAllMembersAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetAllMembersAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1567">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetAllMembersAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetAllMembersAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetAllMembersAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetAllMembersAsync">GetAllMembersAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Retrieves a full list of members from Discord. This method will bypass cache.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;IReadOnlyCollection&lt;DiscordMember&gt;&gt; GetAllMembersAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyCollection</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A collection of all members in this guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetApplicationCommandsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetApplicationCommandsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2045">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetApplicationCommandsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetApplicationCommandsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetApplicationCommandsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetApplicationCommandsAsync">GetApplicationCommandsAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all the application commands in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordApplicationCommand&gt;&gt; GetApplicationCommandsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordApplicationCommand.html">DiscordApplicationCommand</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A list of application commands in this guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetAuditLogsAsync_System_Nullable_System_Int32__DisCatSharp_Entities_DiscordMember_System_Nullable_DisCatSharp_Enums_AuditLogActionType__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetAuditLogsAsync(System.Nullable%7BSystem.Int32%7D%2CDisCatSharp.Entities.DiscordMember%2CSystem.Nullable%7BDisCatSharp.Enums.AuditLogActionType%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.AuditLog.cs/#L55">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetAuditLogsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetAuditLogsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetAuditLogsAsync_System_Nullable_System_Int32__DisCatSharp_Entities_DiscordMember_System_Nullable_DisCatSharp_Enums_AuditLogActionType__" data-uid="DisCatSharp.Entities.DiscordGuild.GetAuditLogsAsync(System.Nullable{System.Int32},DisCatSharp.Entities.DiscordMember,System.Nullable{DisCatSharp.Enums.AuditLogActionType})">GetAuditLogsAsync(Nullable&lt;Int32&gt;, DiscordMember, Nullable&lt;AuditLogActionType&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets audit log entries for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;IReadOnlyList&lt;DiscordAuditLogEntry&gt;&gt; GetAuditLogsAsync(int? limit = null, DiscordMember byMember = null, AuditLogActionType? actionType = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><span class="parametername">limit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Maximum number of entries to fetch.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a></td>
        <td><span class="parametername">byMember</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Filter by member responsible.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="DisCatSharp.Enums.AuditLogActionType.html">AuditLogActionType</a>&gt;</td>
        <td><span class="parametername">actionType</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Filter by action type.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordAuditLogEntry.html">DiscordAuditLogEntry</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A collection of requested audit log entries.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetAutomodRuleAsync_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetAutomodRuleAsync(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1038">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetAutomodRuleAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetAutomodRuleAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetAutomodRuleAsync_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.GetAutomodRuleAsync(System.UInt64)">GetAutomodRuleAsync(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a specific auto mod rule.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;AutomodRule&gt; GetAutomodRuleAsync(ulong ruleId)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">ruleId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The rule id to get.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.AutomodRule.html">AutomodRule</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The auto mod rule.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetAutomodRulesAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetAutomodRulesAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1030">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetAutomodRulesAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetAutomodRulesAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetAutomodRulesAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetAutomodRulesAsync">GetAutomodRulesAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all auto mod rules for a guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;ReadOnlyCollection&lt;AutomodRule&gt;&gt; GetAutomodRulesAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">ReadOnlyCollection</span>&lt;<a class="xref" href="DisCatSharp.Entities.AutomodRule.html">AutomodRule</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A collection of all rules in the guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetBanAsync_DisCatSharp_Entities_DiscordUser_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetBanAsync(DisCatSharp.Entities.DiscordUser)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1004">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetBanAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetBanAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetBanAsync_DisCatSharp_Entities_DiscordUser_" data-uid="DisCatSharp.Entities.DiscordGuild.GetBanAsync(DisCatSharp.Entities.DiscordUser)">GetBanAsync(DiscordUser)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a ban for a specific user.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordBan&gt; GetBanAsync(DiscordUser user)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordUser.html">DiscordUser</a></td>
        <td><span class="parametername">user</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The user to get the ban for.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordBan.html">DiscordBan</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The requested ban object.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the specified user is not banned.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetBanAsync_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetBanAsync(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L975">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetBanAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetBanAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetBanAsync_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.GetBanAsync(System.UInt64)">GetBanAsync(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a ban for a specific user.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordBan&gt; GetBanAsync(ulong userId)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">userId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The Id of the user to get the ban for.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordBan.html">DiscordBan</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The requested ban object.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the specified user is not banned.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetBansAsync_System_Nullable_System_Int32__System_Nullable_System_UInt64__System_Nullable_System_UInt64__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetBansAsync(System.Nullable%7BSystem.Int32%7D%2CSystem.Nullable%7BSystem.UInt64%7D%2CSystem.Nullable%7BSystem.UInt64%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L966">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetBansAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetBansAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetBansAsync_System_Nullable_System_Int32__System_Nullable_System_UInt64__System_Nullable_System_UInt64__" data-uid="DisCatSharp.Entities.DiscordGuild.GetBansAsync(System.Nullable{System.Int32},System.Nullable{System.UInt64},System.Nullable{System.UInt64})">GetBansAsync(Nullable&lt;Int32&gt;, Nullable&lt;UInt64&gt;, Nullable&lt;UInt64&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the bans for this guild, allowing for pagination.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordBan&gt;&gt; GetBansAsync(int? limit = null, ulong? before = null, ulong? after = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><span class="parametername">limit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Maximum number of bans to fetch. Max 1000. Defaults to 1000.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>&gt;</td>
        <td><span class="parametername">before</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The Id of the user before which to fetch the bans. Overrides <code data-dev-comment-type="paramref" class="paramref">after</code> if both are present.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>&gt;</td>
        <td><span class="parametername">after</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The Id of the user after which to fetch the bans.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordBan.html">DiscordBan</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Collection of bans in this guild in ascending order by user id.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_BanMembers">BanMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetChannel_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetChannel(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1679">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetChannel_" data-uid="DisCatSharp.Entities.DiscordGuild.GetChannel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetChannel_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.GetChannel(System.UInt64)">GetChannel(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a channel from this guild by its ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordChannel GetChannel(ulong id)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">id</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">ID of the channel to get.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Requested channel.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetChannelsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetChannelsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1646">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetChannelsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetChannelsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetChannelsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetChannelsAsync">GetChannelsAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all the channels this guild has.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordChannel&gt;&gt; GetChannelsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A collection of this guild&apos;s channels.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetDefaultChannel.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetDefaultChannel%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1934">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetDefaultChannel_" data-uid="DisCatSharp.Entities.DiscordGuild.GetDefaultChannel*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetDefaultChannel" data-uid="DisCatSharp.Entities.DiscordGuild.GetDefaultChannel">GetDefaultChannel()</h4>
  <div class="markdown level1 summary"><p>Gets the default channel for this guild.</p>
<p>Default channel is the first channel current member can see.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordChannel GetDefaultChannel()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">This member&apos;s default guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetEmojiAsync_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetEmojiAsync(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1705">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetEmojiAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetEmojiAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetEmojiAsync_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.GetEmojiAsync(System.UInt64)">GetEmojiAsync(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s specified custom emoji.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildEmoji&gt; GetEmojiAsync(ulong id)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">id</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">ID of the emoji to get.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildEmoji.html">DiscordGuildEmoji</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The requested custom emoji.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetEmojisAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetEmojisAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1696">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetEmojisAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetEmojisAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetEmojisAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetEmojisAsync">GetEmojisAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all of this guild&apos;s custom emojis.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordGuildEmoji&gt;&gt; GetEmojisAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildEmoji.html">DiscordGuildEmoji</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">All of this guild&apos;s custom emojis.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetHashCode.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetHashCode%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2127">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetHashCode_" data-uid="DisCatSharp.Entities.DiscordGuild.GetHashCode*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetHashCode" data-uid="DisCatSharp.Entities.DiscordGuild.GetHashCode">GetHashCode()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the hash code for this <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public override int GetHashCode()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The hash code for this <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetIntegrationsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetIntegrationsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1405">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetIntegrationsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetIntegrationsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetIntegrationsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetIntegrationsAsync">GetIntegrationsAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets integrations attached to this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordIntegration&gt;&gt; GetIntegrationsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordIntegration.html">DiscordIntegration</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Collection of integrations attached to this guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetInvite_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetInvite(System.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1476">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetInvite_" data-uid="DisCatSharp.Entities.DiscordGuild.GetInvite*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetInvite_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.GetInvite(System.String)">GetInvite(String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets an invite from this guild from an invite code.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordInvite GetInvite(string code)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">code</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The invite code</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordInvite.html">DiscordInvite</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">An invite, or null if not in cache.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetInvitesAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetInvitesAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1484">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetInvitesAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetInvitesAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetInvitesAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetInvitesAsync">GetInvitesAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all the invites created for all the channels in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;IReadOnlyList&lt;DiscordInvite&gt;&gt; GetInvitesAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordInvite.html">DiscordInvite</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A collection of invites.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetMemberAsync_System_UInt64_System_Boolean_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetMemberAsync(System.UInt64%2CSystem.Boolean)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1542">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetMemberAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetMemberAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetMemberAsync_System_UInt64_System_Boolean_" data-uid="DisCatSharp.Entities.DiscordGuild.GetMemberAsync(System.UInt64,System.Boolean)">GetMemberAsync(UInt64, Boolean)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a member of this guild by their user ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordMember&gt; GetMemberAsync(ulong userId, bool fetch = false)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">userId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">ID of the member to get.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><span class="parametername">fetch</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to fetch the member from the api prior to cache.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The requested member.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetMembershipScreeningFormAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetMembershipScreeningFormAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2024">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetMembershipScreeningFormAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetMembershipScreeningFormAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetMembershipScreeningFormAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetMembershipScreeningFormAsync">GetMembershipScreeningFormAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s membership screening form.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildMembershipScreening&gt; GetMembershipScreeningFormAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildMembershipScreening.html">DiscordGuildMembershipScreening</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">This guild&apos;s membership screening form.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetOrderedChannels.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetOrderedChannels%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L567">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetOrderedChannels_" data-uid="DisCatSharp.Entities.DiscordGuild.GetOrderedChannels*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetOrderedChannels" data-uid="DisCatSharp.Entities.DiscordGuild.GetOrderedChannels">GetOrderedChannels()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="5">Gets an ordered <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a> list out of the channel cache.
Returns a Dictionary where the key is an ulong and can be mapped to <a class="xref" href="DisCatSharp.Enums.ChannelType.html#DisCatSharp_Enums_ChannelType_Category">Category</a> <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>s.
Ignore the 0 key here, because that indicates that this is the &quot;has no category&quot; list.
Each value contains a ordered list of text/news and voice/stage channels as <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Dictionary&lt;ulong, List&lt;DiscordChannel&gt;&gt; GetOrderedChannels()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Dictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <span class="xref">List</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A ordered list of categories with its channels</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetOrderedChannelsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetOrderedChannelsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L609">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetOrderedChannelsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetOrderedChannelsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetOrderedChannelsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetOrderedChannelsAsync">GetOrderedChannelsAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="5">Gets an ordered <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a> list.
Returns a Dictionary where the key is an ulong and can be mapped to <a class="xref" href="DisCatSharp.Enums.ChannelType.html#DisCatSharp_Enums_ChannelType_Category">Category</a> <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>s.
Ignore the 0 key here, because that indicates that this is the &quot;has no category&quot; list.
Each value contains a ordered list of text/news and voice/stage channels as <a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;Dictionary&lt;ulong, List&lt;DiscordChannel&gt;&gt;&gt; GetOrderedChannelsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">Dictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <span class="xref">List</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a>&gt;&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A ordered list of categories with its channels</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetPruneCountAsync_System_Int32_IEnumerable_DisCatSharp_Entities_DiscordRole__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetPruneCountAsync(System.Int32%2CIEnumerable%7BDisCatSharp.Entities.DiscordRole%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1355">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetPruneCountAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetPruneCountAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetPruneCountAsync_System_Int32_IEnumerable_DisCatSharp_Entities_DiscordRole__" data-uid="DisCatSharp.Entities.DiscordGuild.GetPruneCountAsync(System.Int32,IEnumerable{DisCatSharp.Entities.DiscordRole})">GetPruneCountAsync(Int32, IEnumerable&lt;DiscordRole&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Estimates the number of users to be pruned.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;int&gt; GetPruneCountAsync(int days = 7, IEnumerable&lt;DiscordRole&gt; includedRoles = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><span class="parametername">days</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a>&gt;</td>
        <td><span class="parametername">includedRoles</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The roles to be included in the prune.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Number of users that will be pruned.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_KickMembers">KickMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetRole_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetRole(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1670">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetRole_" data-uid="DisCatSharp.Entities.DiscordGuild.GetRole*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetRole_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.GetRole(System.UInt64)">GetRole(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a role from this guild by its ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordRole GetRole(ulong id)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">id</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">ID of the role to get.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Requested role.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetScheduledEventAsync_DisCatSharp_Entities_DiscordScheduledEvent_System_Nullable_System_Boolean__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetScheduledEventAsync(DisCatSharp.Entities.DiscordScheduledEvent%2CSystem.Nullable%7BSystem.Boolean%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1149">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetScheduledEventAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetScheduledEventAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetScheduledEventAsync_DisCatSharp_Entities_DiscordScheduledEvent_System_Nullable_System_Boolean__" data-uid="DisCatSharp.Entities.DiscordGuild.GetScheduledEventAsync(DisCatSharp.Entities.DiscordScheduledEvent,System.Nullable{System.Boolean})">GetScheduledEventAsync(DiscordScheduledEvent, Nullable&lt;Boolean&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a specific scheduled events.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordScheduledEvent&gt; GetScheduledEventAsync(DiscordScheduledEvent scheduledEvent, bool? withUserCount = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a></td>
        <td><span class="parametername">scheduledEvent</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The event to get.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">withUserCount</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to include user count.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A scheduled event.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetScheduledEventAsync_System_UInt64_System_Nullable_System_Boolean__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetScheduledEventAsync(System.UInt64%2CSystem.Nullable%7BSystem.Boolean%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1115">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetScheduledEventAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetScheduledEventAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetScheduledEventAsync_System_UInt64_System_Nullable_System_Boolean__" data-uid="DisCatSharp.Entities.DiscordGuild.GetScheduledEventAsync(System.UInt64,System.Nullable{System.Boolean})">GetScheduledEventAsync(UInt64, Nullable&lt;Boolean&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a specific scheduled events.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordScheduledEvent&gt; GetScheduledEventAsync(ulong scheduledEventId, bool? withUserCount = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">scheduledEventId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The Id of the event to get.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">withUserCount</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to include user count.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A scheduled event.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetScheduledEventsAsync_System_Nullable_System_Boolean__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetScheduledEventsAsync(System.Nullable%7BSystem.Boolean%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1182">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetScheduledEventsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetScheduledEventsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetScheduledEventsAsync_System_Nullable_System_Boolean__" data-uid="DisCatSharp.Entities.DiscordGuild.GetScheduledEventsAsync(System.Nullable{System.Boolean})">GetScheduledEventsAsync(Nullable&lt;Boolean&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guilds scheduled events.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;IReadOnlyDictionary&lt;ulong, DiscordScheduledEvent&gt;&gt; GetScheduledEventsAsync(bool? withUserCount = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">withUserCount</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to include user count.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyDictionary</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>, <a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A list of the guilds scheduled events.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetStickerAsync_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetStickerAsync(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1806">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetStickerAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetStickerAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetStickerAsync_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.GetStickerAsync(System.UInt64)">GetStickerAsync(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a sticker</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordSticker&gt; GetStickerAsync(ulong stickerId)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">stickerId</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the sticker could not be found.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetStickersAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetStickersAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1781">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetStickersAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetStickersAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetStickersAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetStickersAsync">GetStickersAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all of this guild&apos;s custom stickers.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;IReadOnlyList&lt;DiscordSticker&gt;&gt; GetStickersAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">All of this guild&apos;s custom stickers.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetTemplatesAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetTemplatesAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1969">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetTemplatesAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetTemplatesAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetTemplatesAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetTemplatesAsync">GetTemplatesAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all of this guild&apos;s templates.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordGuildTemplate&gt;&gt; GetTemplatesAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildTemplate.html">DiscordGuildTemplate</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">All of the guild&apos;s templates.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetThread_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetThread(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1688">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetThread_" data-uid="DisCatSharp.Entities.DiscordGuild.GetThread*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetThread_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.GetThread(System.UInt64)">GetThread(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets a thread from this guild by its ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public DiscordThreadChannel GetThread(ulong id)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">id</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">ID of the thread to get.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordThreadChannel.html">DiscordThreadChannel</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Requested thread.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetVanityInviteAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetVanityInviteAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1505">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetVanityInviteAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetVanityInviteAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetVanityInviteAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetVanityInviteAsync">GetVanityInviteAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the vanity invite for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordInvite&gt; GetVanityInviteAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordInvite.html">DiscordInvite</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A partial vanity invite.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetWebhooksAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetWebhooksAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1514">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetWebhooksAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetWebhooksAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetWebhooksAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetWebhooksAsync">GetWebhooksAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets all the webhooks created for all the channels in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordWebhook&gt;&gt; GetWebhooksAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordWebhook.html">DiscordWebhook</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A collection of webhooks this guild has.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageWebhooks">ManageWebhooks</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetWelcomeScreenAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetWelcomeScreenAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2082">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetWelcomeScreenAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetWelcomeScreenAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetWelcomeScreenAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetWelcomeScreenAsync">GetWelcomeScreenAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s welcome screen.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildWelcomeScreen&gt; GetWelcomeScreenAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildWelcomeScreen.html">DiscordGuildWelcomeScreen</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">This guild&apos;s welcome screen object.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetWidgetAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetWidgetAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1943">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetWidgetAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetWidgetAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetWidgetAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetWidgetAsync">GetWidgetAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s widget</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordWidget&gt; GetWidgetAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordWidget.html">DiscordWidget</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The guild&apos;s widget</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetWidgetImage_DisCatSharp_Enums_WidgetType_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetWidgetImage(DisCatSharp.Enums.WidgetType)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1522">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetWidgetImage_" data-uid="DisCatSharp.Entities.DiscordGuild.GetWidgetImage*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetWidgetImage_DisCatSharp_Enums_WidgetType_" data-uid="DisCatSharp.Entities.DiscordGuild.GetWidgetImage(DisCatSharp.Enums.WidgetType)">GetWidgetImage(WidgetType)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets this guild&apos;s widget image.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public string GetWidgetImage(WidgetType bannerType = default(WidgetType))</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.WidgetType.html">WidgetType</a></td>
        <td><span class="parametername">bannerType</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The format of the widget.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The URL of the widget image.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_GetWidgetSettingsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.GetWidgetSettingsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1950">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_GetWidgetSettingsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.GetWidgetSettingsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_GetWidgetSettingsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.GetWidgetSettingsAsync">GetWidgetSettingsAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the guild&apos;s widget settings</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordWidgetSettings&gt; GetWidgetSettingsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordWidgetSettings.html">DiscordWidgetSettings</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The guild&apos;s widget settings</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_LeaveAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.LeaveAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L954">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_LeaveAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.LeaveAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_LeaveAsync" data-uid="DisCatSharp.Entities.DiscordGuild.LeaveAsync">LeaveAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Leaves this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task LeaveAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ListVoiceRegionsAsync.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ListVoiceRegionsAsync%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1462">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ListVoiceRegionsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ListVoiceRegionsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ListVoiceRegionsAsync" data-uid="DisCatSharp.Entities.DiscordGuild.ListVoiceRegionsAsync">ListVoiceRegionsAsync()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets the voice regions for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;IReadOnlyList&lt;DiscordVoiceRegion&gt;&gt; ListVoiceRegionsAsync()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordVoiceRegion.html">DiscordVoiceRegion</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Voice regions available for this guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyAsync_Action_DisCatSharp_Net_Models_GuildEditModel__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyAsync(Action%7BDisCatSharp.Net.Models.GuildEditModel%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L728">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyAsync_Action_DisCatSharp_Net_Models_GuildEditModel__" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyAsync(Action{DisCatSharp.Net.Models.GuildEditModel})">ModifyAsync(Action&lt;GuildEditModel&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordGuild&gt; ModifyAsync(Action&lt;GuildEditModel&gt; action)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Action</span>&lt;<a class="xref" href="DisCatSharp.Net.Models.GuildEditModel.html">GuildEditModel</a>&gt;</td>
        <td><span class="parametername">action</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Action to perform on this guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The modified guild object.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyCommunitySettingsAsync_System_Boolean_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_DiscordChannel_System_String_System_String_DisCatSharp_Enums_DefaultMessageNotifications_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyCommunitySettingsAsync(System.Boolean%2CDisCatSharp.Entities.DiscordChannel%2CDisCatSharp.Entities.DiscordChannel%2CSystem.String%2CSystem.String%2CDisCatSharp.Enums.DefaultMessageNotifications%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L774">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyCommunitySettingsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyCommunitySettingsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyCommunitySettingsAsync_System_Boolean_DisCatSharp_Entities_DiscordChannel_DisCatSharp_Entities_DiscordChannel_System_String_System_String_DisCatSharp_Enums_DefaultMessageNotifications_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyCommunitySettingsAsync(System.Boolean,DisCatSharp.Entities.DiscordChannel,DisCatSharp.Entities.DiscordChannel,System.String,System.String,DisCatSharp.Enums.DefaultMessageNotifications,System.String)">ModifyCommunitySettingsAsync(Boolean, DiscordChannel, DiscordChannel, String, String, DefaultMessageNotifications, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Modifies the community settings async.
This sets <a class="xref" href="DisCatSharp.Enums.VerificationLevel.html#DisCatSharp_Enums_VerificationLevel_High">High</a> if not highest and <a class="xref" href="DisCatSharp.Enums.ExplicitContentFilter.html#DisCatSharp_Enums_ExplicitContentFilter_AllMembers">AllMembers</a>.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordGuild&gt; ModifyCommunitySettingsAsync(bool enabled, DiscordChannel rulesChannel, DiscordChannel publicUpdatesChannel, string preferredLocale = &quot;en-US&quot;, string description = null, DefaultMessageNotifications defaultMessageNotifications = default(DefaultMessageNotifications), string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><span class="parametername">enabled</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">If true, enables <a class="xref" href="DisCatSharp.Entities.GuildFeaturesEnum.html#DisCatSharp_Entities_GuildFeaturesEnum_HasCommunityEnabled">HasCommunityEnabled</a>.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">rulesChannel</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The rules channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">publicUpdatesChannel</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The public updates channel.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">preferredLocale</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The preferred locale. Defaults to en-US.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The description.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Enums.DefaultMessageNotifications.html">DefaultMessageNotifications</a></td>
        <td><span class="parametername">defaultMessageNotifications</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The default message notifications. Defaults to <a class="xref" href="DisCatSharp.Enums.DefaultMessageNotifications.html#DisCatSharp_Enums_DefaultMessageNotifications_MentionsOnly">MentionsOnly</a></p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The audit log reason.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a>&gt;</td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_Administrator">Administrator</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyEmojiAsync_DisCatSharp_Entities_DiscordGuildEmoji_System_String_IEnumerable_DisCatSharp_Entities_DiscordRole__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyEmojiAsync(DisCatSharp.Entities.DiscordGuildEmoji%2CSystem.String%2CIEnumerable%7BDisCatSharp.Entities.DiscordRole%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1745">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyEmojiAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyEmojiAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyEmojiAsync_DisCatSharp_Entities_DiscordGuildEmoji_System_String_IEnumerable_DisCatSharp_Entities_DiscordRole__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyEmojiAsync(DisCatSharp.Entities.DiscordGuildEmoji,System.String,IEnumerable{DisCatSharp.Entities.DiscordRole},System.String)">ModifyEmojiAsync(DiscordGuildEmoji, String, IEnumerable&lt;DiscordRole&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies a this guild&apos;s custom emoji.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildEmoji&gt; ModifyEmojiAsync(DiscordGuildEmoji emoji, string name, IEnumerable&lt;DiscordRole&gt; roles = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordGuildEmoji.html">DiscordGuildEmoji</a></td>
        <td><span class="parametername">emoji</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Emoji to modify.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">New name for the emoji.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a>&gt;</td>
        <td><span class="parametername">roles</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit log.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildEmoji.html">DiscordGuildEmoji</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The modified emoji.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_System_Int32_System_Int32_System_Boolean_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyIntegrationAsync(DisCatSharp.Entities.DiscordIntegration%2CSystem.Int32%2CSystem.Int32%2CSystem.Boolean)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1432">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyIntegrationAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyIntegrationAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_System_Int32_System_Int32_System_Boolean_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyIntegrationAsync(DisCatSharp.Entities.DiscordIntegration,System.Int32,System.Int32,System.Boolean)">ModifyIntegrationAsync(DiscordIntegration, Int32, Int32, Boolean)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies an integration in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordIntegration&gt; ModifyIntegrationAsync(DiscordIntegration integration, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordIntegration.html">DiscordIntegration</a></td>
        <td><span class="parametername">integration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Integration to modify.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><span class="parametername">expireBehaviour</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Number of days after which the integration expires.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><span class="parametername">expireGracePeriod</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Length of grace period which allows for renewing the integration.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><span class="parametername">enableEmoticons</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether emotes should be synced from this integration.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordIntegration.html">DiscordIntegration</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The modified integration.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyMembershipScreeningFormAsync_Action_DisCatSharp_Net_Models_MembershipScreeningEditModel__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyMembershipScreeningFormAsync(Action%7BDisCatSharp.Net.Models.MembershipScreeningEditModel%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2034">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyMembershipScreeningFormAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyMembershipScreeningFormAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyMembershipScreeningFormAsync_Action_DisCatSharp_Net_Models_MembershipScreeningEditModel__" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyMembershipScreeningFormAsync(Action{DisCatSharp.Net.Models.MembershipScreeningEditModel})">ModifyMembershipScreeningFormAsync(Action&lt;MembershipScreeningEditModel&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies this guild&apos;s membership screening form.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordGuildMembershipScreening&gt; ModifyMembershipScreeningFormAsync(Action&lt;MembershipScreeningEditModel&gt; action)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Action</span>&lt;<a class="xref" href="DisCatSharp.Net.Models.MembershipScreeningEditModel.html">MembershipScreeningEditModel</a>&gt;</td>
        <td><span class="parametername">action</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Action to perform</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildMembershipScreening.html">DiscordGuildMembershipScreening</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The modified screening form.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client doesn&apos;t have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission, or community is not enabled on this guild.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyStickerAsync_DisCatSharp_Entities_DiscordSticker_DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_DisCatSharp_Entities_DiscordEmoji__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyStickerAsync(DisCatSharp.Entities.DiscordSticker%2CDisCatSharp.Entities.Optional%7BSystem.String%7D%2CDisCatSharp.Entities.Optional%7BSystem.String%7D%2CDisCatSharp.Entities.Optional%7BDisCatSharp.Entities.DiscordEmoji%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1897">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyStickerAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyStickerAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyStickerAsync_DisCatSharp_Entities_DiscordSticker_DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_DisCatSharp_Entities_DiscordEmoji__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyStickerAsync(DisCatSharp.Entities.DiscordSticker,DisCatSharp.Entities.Optional{System.String},DisCatSharp.Entities.Optional{System.String},DisCatSharp.Entities.Optional{DisCatSharp.Entities.DiscordEmoji},System.String)">ModifyStickerAsync(DiscordSticker, Optional&lt;String&gt;, Optional&lt;String&gt;, Optional&lt;DiscordEmoji&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies a sticker</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordSticker&gt; ModifyStickerAsync(DiscordSticker sticker, Optional&lt;string&gt; name, Optional&lt;string&gt; description, Optional&lt;DiscordEmoji&gt; emoji, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a></td>
        <td><span class="parametername">sticker</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The sticker to modify</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The name of the sticker</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The description of the sticker</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordEmoji.html">DiscordEmoji</a>&gt;</td>
        <td><span class="parametername">emoji</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The emoji to associate with this sticker.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Audit log reason</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A sticker object</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the sticker could not be found.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyStickerAsync_System_UInt64_DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_DisCatSharp_Entities_DiscordEmoji__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyStickerAsync(System.UInt64%2CDisCatSharp.Entities.Optional%7BSystem.String%7D%2CDisCatSharp.Entities.Optional%7BSystem.String%7D%2CDisCatSharp.Entities.Optional%7BDisCatSharp.Entities.DiscordEmoji%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1860">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyStickerAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyStickerAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyStickerAsync_System_UInt64_DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_System_String__DisCatSharp_Entities_Optional_DisCatSharp_Entities_DiscordEmoji__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyStickerAsync(System.UInt64,DisCatSharp.Entities.Optional{System.String},DisCatSharp.Entities.Optional{System.String},DisCatSharp.Entities.Optional{DisCatSharp.Entities.DiscordEmoji},System.String)">ModifyStickerAsync(UInt64, Optional&lt;String&gt;, Optional&lt;String&gt;, Optional&lt;DiscordEmoji&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies a sticker</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordSticker&gt; ModifyStickerAsync(ulong sticker, Optional&lt;string&gt; name, Optional&lt;string&gt; description, Optional&lt;DiscordEmoji&gt; emoji, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">sticker</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The id of the sticker to modify</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The name of the sticker</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a>&gt;</td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The description of the sticker</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.Optional-1.html">Optional</a>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordEmoji.html">DiscordEmoji</a>&gt;</td>
        <td><span class="parametername">emoji</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The emoji to associate with this sticker.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Audit log reason</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordSticker.html">DiscordSticker</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A sticker object</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the sticker could not be found.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageEmojisAndStickers">ManageEmojisAndStickers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyTemplateAsync_System_String_System_String_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyTemplateAsync(System.String%2CSystem.String%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2005">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyTemplateAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyTemplateAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyTemplateAsync_System_String_System_String_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyTemplateAsync(System.String,System.String,System.String)">ModifyTemplateAsync(String, String, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies the template&apos;s metadata.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildTemplate&gt; ModifyTemplateAsync(string code, string name = null, string description = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">code</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The template&apos;s code.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Name of the template.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">description</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Description of the template.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildTemplate.html">DiscordGuildTemplate</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The template modified.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the template for the code cannot be found</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyWelcomeScreenAsync_Action_DisCatSharp_Net_Models_WelcomeScreenEditModel__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyWelcomeScreenAsync(Action%7BDisCatSharp.Net.Models.WelcomeScreenEditModel%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2092">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyWelcomeScreenAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyWelcomeScreenAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyWelcomeScreenAsync_Action_DisCatSharp_Net_Models_WelcomeScreenEditModel__" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyWelcomeScreenAsync(Action{DisCatSharp.Net.Models.WelcomeScreenEditModel})">ModifyWelcomeScreenAsync(Action&lt;WelcomeScreenEditModel&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies this guild&apos;s welcome screen.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordGuildWelcomeScreen&gt; ModifyWelcomeScreenAsync(Action&lt;WelcomeScreenEditModel&gt; action)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Action</span>&lt;<a class="xref" href="DisCatSharp.Net.Models.WelcomeScreenEditModel.html">WelcomeScreenEditModel</a>&gt;</td>
        <td><span class="parametername">action</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Action to perform.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildWelcomeScreen.html">DiscordGuildWelcomeScreen</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The modified welcome screen.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client doesn&apos;t have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission, or community is not enabled on this guild.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ModifyWidgetSettingsAsync_System_Nullable_System_Boolean__DisCatSharp_Entities_DiscordChannel_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ModifyWidgetSettingsAsync(System.Nullable%7BSystem.Boolean%7D%2CDisCatSharp.Entities.DiscordChannel%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1960">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ModifyWidgetSettingsAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyWidgetSettingsAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ModifyWidgetSettingsAsync_System_Nullable_System_Boolean__DisCatSharp_Entities_DiscordChannel_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.ModifyWidgetSettingsAsync(System.Nullable{System.Boolean},DisCatSharp.Entities.DiscordChannel,System.String)">ModifyWidgetSettingsAsync(Nullable&lt;Boolean&gt;, DiscordChannel, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Modifies the guild&apos;s widget settings</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordWidgetSettings&gt; ModifyWidgetSettingsAsync(bool? isEnabled = null, DiscordChannel channel = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">isEnabled</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">If the widget is enabled or not</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordChannel.html">DiscordChannel</a></td>
        <td><span class="parametername">channel</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Widget channel</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason the widget settings were modified</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordWidgetSettings.html">DiscordWidgetSettings</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The newly modified widget settings</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_PruneAsync_System_Int32_System_Boolean_IEnumerable_DisCatSharp_Entities_DiscordRole__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.PruneAsync(System.Int32%2CSystem.Boolean%2CIEnumerable%7BDisCatSharp.Entities.DiscordRole%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1382">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_PruneAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.PruneAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_PruneAsync_System_Int32_System_Boolean_IEnumerable_DisCatSharp_Entities_DiscordRole__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.PruneAsync(System.Int32,System.Boolean,IEnumerable{DisCatSharp.Entities.DiscordRole},System.String)">PruneAsync(Int32, Boolean, IEnumerable&lt;DiscordRole&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Prunes inactive users from this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;int?&gt; PruneAsync(int days = 7, bool computePruneCount = true, IEnumerable&lt;DiscordRole&gt; includedRoles = null, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><span class="parametername">days</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><span class="parametername">computePruneCount</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordRole.html">DiscordRole</a>&gt;</td>
        <td><span class="parametername">includedRoles</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The roles to be included in the prune.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Number of users pruned.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageChannels">ManageChannels</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_RemoveTimeoutAsync_System_UInt64_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.RemoveTimeoutAsync(System.UInt64%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L897">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_RemoveTimeoutAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.RemoveTimeoutAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_RemoveTimeoutAsync_System_UInt64_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.RemoveTimeoutAsync(System.UInt64,System.String)">RemoveTimeoutAsync(UInt64, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Removes the timeout from a specified member in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task RemoveTimeoutAsync(ulong memberId, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">memberId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Member to remove the timeout from.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ModerateMembers">ModerateMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the member does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_RequestMembersAsync_System_String_System_Int32_System_Nullable_System_Boolean__IEnumerable_System_UInt64__System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.RequestMembersAsync(System.String%2CSystem.Int32%2CSystem.Nullable%7BSystem.Boolean%7D%2CIEnumerable%7BSystem.UInt64%7D%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1611">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_RequestMembersAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.RequestMembersAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_RequestMembersAsync_System_String_System_Int32_System_Nullable_System_Boolean__IEnumerable_System_UInt64__System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.RequestMembersAsync(System.String,System.Int32,System.Nullable{System.Boolean},IEnumerable{System.UInt64},System.String)">RequestMembersAsync(String, Int32, Nullable&lt;Boolean&gt;, IEnumerable&lt;UInt64&gt;, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="3">Requests that Discord send a list of guild members based on the specified arguments. This method will fire the <a class="xref" href="DisCatSharp.DiscordClient.html#DisCatSharp_DiscordClient_GuildMembersChunked">GuildMembersChunked</a> event.
<p>If no arguments aside from <code data-dev-comment-type="paramref" class="paramref">presences</code> and <code data-dev-comment-type="paramref" class="paramref">nonce</code> are specified, this will request all guild members.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task RequestMembersAsync(string query = &quot;&quot;, int limit = 0, bool? presences = null, IEnumerable&lt;ulong&gt; userIds = null, string nonce = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">query</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="2">Filters the returned members based on what the username starts with. Either this or <code data-dev-comment-type="paramref" class="paramref">userIds</code> must not be null.
The <code data-dev-comment-type="paramref" class="paramref">limit</code> must also be greater than 0 if this is specified.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a></td>
        <td><span class="parametername">limit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Total number of members to request. This must be greater than 0 if <code data-dev-comment-type="paramref" class="paramref">query</code> is specified.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">presences</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to include the <a class="xref" href="DisCatSharp.EventArgs.GuildMembersChunkEventArgs.html#DisCatSharp_EventArgs_GuildMembersChunkEventArgs_Presences">Presences</a> associated with the fetched members.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">IEnumerable</span>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a>&gt;</td>
        <td><span class="parametername">userIds</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to limit the request to the specified user ids. Either this or <code data-dev-comment-type="paramref" class="paramref">query</code> must not be null.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">nonce</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The unique string to identify the response.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_SearchMembersAsync_System_String_System_Nullable_System_Int32__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.SearchMembersAsync(System.String%2CSystem.Nullable%7BSystem.Int32%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L670">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_SearchMembersAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.SearchMembersAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_SearchMembersAsync_System_String_System_Nullable_System_Int32__" data-uid="DisCatSharp.Entities.DiscordGuild.SearchMembersAsync(System.String,System.Nullable{System.Int32})">SearchMembersAsync(String, Nullable&lt;Int32&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Searches the current guild for members who&apos;s display name start with the specified name.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;IReadOnlyList&lt;DiscordMember&gt;&gt; SearchMembersAsync(string name, int? limit)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">name</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The name to search for.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.int32">Int32</a>&gt;</td>
        <td><span class="parametername">limit</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The maximum amount of members to return. Max 1000. Defaults to 1.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<span class="xref">IReadOnlyList</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordMember.html">DiscordMember</a>&gt;&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The members found, if any.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_SyncIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.SyncIntegrationAsync(DisCatSharp.Entities.DiscordIntegration)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1454">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_SyncIntegrationAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.SyncIntegrationAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_SyncIntegrationAsync_DisCatSharp_Entities_DiscordIntegration_" data-uid="DisCatSharp.Entities.DiscordGuild.SyncIntegrationAsync(DisCatSharp.Entities.DiscordIntegration)">SyncIntegrationAsync(DiscordIntegration)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Forces re-synchronization of an integration for this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task SyncIntegrationAsync(DiscordIntegration integration)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordIntegration.html">DiscordIntegration</a></td>
        <td><span class="parametername">integration</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Integration to synchronize.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the guild does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_SyncTemplateAsync_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.SyncTemplateAsync(System.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1992">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_SyncTemplateAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.SyncTemplateAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_SyncTemplateAsync_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.SyncTemplateAsync(System.String)">SyncTemplateAsync(String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Syncs the template to the current guild&apos;s state.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task&lt;DiscordGuildTemplate&gt; SyncTemplateAsync(string code)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">code</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The code of the template to sync.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordGuildTemplate.html">DiscordGuildTemplate</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The template synced.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the template for the code cannot be found</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Throws when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ManageGuild">ManageGuild</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_TimeoutAsync_System_UInt64_DateTime_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.TimeoutAsync(System.UInt64%2CDateTime%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L885">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_TimeoutAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.TimeoutAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_TimeoutAsync_System_UInt64_DateTime_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.TimeoutAsync(System.UInt64,DateTime,System.String)">TimeoutAsync(UInt64, DateTime, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Timeout a specified member in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task TimeoutAsync(ulong memberId, DateTime until, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">memberId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Member to timeout.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">DateTime</span></td>
        <td><span class="parametername">until</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The datetime to time out the user. Up to 28 days.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ModerateMembers">ModerateMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the member does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_TimeoutAsync_System_UInt64_DateTimeOffset_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.TimeoutAsync(System.UInt64%2CDateTimeOffset%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L857">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_TimeoutAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.TimeoutAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_TimeoutAsync_System_UInt64_DateTimeOffset_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.TimeoutAsync(System.UInt64,DateTimeOffset,System.String)">TimeoutAsync(UInt64, DateTimeOffset, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Timeout a specified member in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task TimeoutAsync(ulong memberId, DateTimeOffset until, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">memberId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Member to timeout.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">DateTimeOffset</span></td>
        <td><span class="parametername">until</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The datetime offset to time out the user. Up to 28 days.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ModerateMembers">ModerateMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the member does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_TimeoutAsync_System_UInt64_TimeSpan_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.TimeoutAsync(System.UInt64%2CTimeSpan%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L872">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_TimeoutAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.TimeoutAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_TimeoutAsync_System_UInt64_TimeSpan_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.TimeoutAsync(System.UInt64,TimeSpan,System.String)">TimeoutAsync(UInt64, TimeSpan, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Timeout a specified member in this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task TimeoutAsync(ulong memberId, TimeSpan until, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">memberId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Member to timeout.</p>
</td>
      </tr>
      <tr>
        <td><span class="xref">TimeSpan</span></td>
        <td><span class="parametername">until</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The timespan to time out the user. Up to 28 days.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_ModerateMembers">ModerateMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the member does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_ToString.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.ToString%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2104">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_ToString_" data-uid="DisCatSharp.Entities.DiscordGuild.ToString*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_ToString" data-uid="DisCatSharp.Entities.DiscordGuild.ToString">ToString()</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Returns a string representation of this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public override string ToString()</code></pre>
  </div>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">String representation of this guild.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_TryGetBanAsync_DisCatSharp_Entities_DiscordUser_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.TryGetBanAsync(DisCatSharp.Entities.DiscordUser)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1013">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_TryGetBanAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetBanAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_TryGetBanAsync_DisCatSharp_Entities_DiscordUser_" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetBanAsync(DisCatSharp.Entities.DiscordUser)">TryGetBanAsync(DiscordUser)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Tries to get a ban for a specific user.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordBan&gt; TryGetBanAsync(DiscordUser user)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordUser.html">DiscordUser</a></td>
        <td><span class="parametername">user</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The user to get the ban for.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordBan.html">DiscordBan</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The requested ban object or null if not found.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_TryGetBanAsync_System_UInt64_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.TryGetBanAsync(System.UInt64)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L984">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_TryGetBanAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetBanAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_TryGetBanAsync_System_UInt64_" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetBanAsync(System.UInt64)">TryGetBanAsync(UInt64)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Tries to get a ban for a specific user.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordBan&gt; TryGetBanAsync(ulong userId)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">userId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The Id of the user to get the ban for.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordBan.html">DiscordBan</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The requested ban object or null if not found.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_TryGetScheduledEventAsync_DisCatSharp_Entities_DiscordScheduledEvent_System_Nullable_System_Boolean__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.TryGetScheduledEventAsync(DisCatSharp.Entities.DiscordScheduledEvent%2CSystem.Nullable%7BSystem.Boolean%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1161">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_TryGetScheduledEventAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetScheduledEventAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_TryGetScheduledEventAsync_DisCatSharp_Entities_DiscordScheduledEvent_System_Nullable_System_Boolean__" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetScheduledEventAsync(DisCatSharp.Entities.DiscordScheduledEvent,System.Nullable{System.Boolean})">TryGetScheduledEventAsync(DiscordScheduledEvent, Nullable&lt;Boolean&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Tries to get a specific scheduled events.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordScheduledEvent&gt; TryGetScheduledEventAsync(DiscordScheduledEvent scheduledEvent, bool? withUserCount = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a></td>
        <td><span class="parametername">scheduledEvent</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The event to get.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">withUserCount</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to include user count.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A scheduled event or null if not found.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_TryGetScheduledEventAsync_System_UInt64_System_Nullable_System_Boolean__.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.TryGetScheduledEventAsync(System.UInt64%2CSystem.Nullable%7BSystem.Boolean%7D)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L1127">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_TryGetScheduledEventAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetScheduledEventAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_TryGetScheduledEventAsync_System_UInt64_System_Nullable_System_Boolean__" data-uid="DisCatSharp.Entities.DiscordGuild.TryGetScheduledEventAsync(System.UInt64,System.Nullable{System.Boolean})">TryGetScheduledEventAsync(UInt64, Nullable&lt;Boolean&gt;)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Tries to get a specific scheduled events.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public async Task&lt;DiscordScheduledEvent&gt; TryGetScheduledEventAsync(ulong scheduledEventId, bool? withUserCount = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">scheduledEventId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">The Id of the event to get.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.nullable-1">Nullable</a>&lt;<a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a>&gt;</td>
        <td><span class="parametername">withUserCount</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether to include user count.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span>&lt;<a class="xref" href="DisCatSharp.Entities.DiscordScheduledEvent.html">DiscordScheduledEvent</a>&gt;</td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">A scheduled event or null if not found.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_UnbanMemberAsync_DisCatSharp_Entities_DiscordUser_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.UnbanMemberAsync(DisCatSharp.Entities.DiscordUser%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L935">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_UnbanMemberAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.UnbanMemberAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_UnbanMemberAsync_DisCatSharp_Entities_DiscordUser_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.UnbanMemberAsync(DisCatSharp.Entities.DiscordUser,System.String)">UnbanMemberAsync(DiscordUser, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Unbans a user from this guild.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task UnbanMemberAsync(DiscordUser user, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordUser.html">DiscordUser</a></td>
        <td><span class="parametername">user</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">User to unban.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_BanMembers">BanMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the user does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_UnbanMemberAsync_System_UInt64_System_String_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.UnbanMemberAsync(System.UInt64%2CSystem.String)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L947">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_UnbanMemberAsync_" data-uid="DisCatSharp.Entities.DiscordGuild.UnbanMemberAsync*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_UnbanMemberAsync_System_UInt64_System_String_" data-uid="DisCatSharp.Entities.DiscordGuild.UnbanMemberAsync(System.UInt64,System.String)">UnbanMemberAsync(UInt64, String)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Unbans a user by ID.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public Task UnbanMemberAsync(ulong userId, string reason = null)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.uint64">UInt64</a></td>
        <td><span class="parametername">userId</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">ID of the user to unban.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.string">String</a></td>
        <td><span class="parametername">reason</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Reason for audit logs.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">Task</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="exceptions">Exceptions</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Condition</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.UnauthorizedException.html">UnauthorizedException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the client does not have the <a class="xref" href="DisCatSharp.Enums.Permissions.html#DisCatSharp_Enums_Permissions_BanMembers">BanMembers</a> permission.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.NotFoundException.html">NotFoundException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when the user does not exist.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.BadRequestException.html">BadRequestException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when an invalid parameter was provided.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Exceptions.ServerErrorException.html">ServerErrorException</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Thrown when Discord is unable to process the request.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h3 id="operators">Operators
</h3>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_op_Equality_DisCatSharp_Entities_DiscordGuild_DisCatSharp_Entities_DiscordGuild_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.op_Equality(DisCatSharp.Entities.DiscordGuild%2CDisCatSharp.Entities.DiscordGuild)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2136">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_op_Equality_" data-uid="DisCatSharp.Entities.DiscordGuild.op_Equality*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_op_Equality_DisCatSharp_Entities_DiscordGuild_DisCatSharp_Entities_DiscordGuild_" data-uid="DisCatSharp.Entities.DiscordGuild.op_Equality(DisCatSharp.Entities.DiscordGuild,DisCatSharp.Entities.DiscordGuild)">Equality(DiscordGuild, DiscordGuild)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets whether the two <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a> objects are equal.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static bool operator ==(DiscordGuild e1, DiscordGuild e2)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a></td>
        <td><span class="parametername">e1</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">First guild to compare.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a></td>
        <td><span class="parametername">e2</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Second guild to compare.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the two guilds are equal.</p>
</td>
      </tr>
    </tbody>
  </table>
  <span class="small pull-right mobile-hide">
    <span class="divider">|</span>
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/new/main/apiSpec/new?filename=DisCatSharp_Entities_DiscordGuild_op_Inequality_DisCatSharp_Entities_DiscordGuild_DisCatSharp_Entities_DiscordGuild_.md&amp;value=---%0Auid%3A%20DisCatSharp.Entities.DiscordGuild.op_Inequality(DisCatSharp.Entities.DiscordGuild%2CDisCatSharp.Entities.DiscordGuild)%0Asummary%3A%20&#39;*You%20can%20override%20summary%20for%20the%20API%20here%20using%20*MARKDOWN*%20syntax&#39;%0A---%0A%0A*Please%20type%20below%20more%20information%20about%20this%20API%3A*%0A%0A">Improve this Doc</a>
  </span>
  <span class="small pull-right mobile-hide">
    <a href="https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/DisCatSharp/Entities/Guild/DiscordGuild.cs/#L2150">View Source</a>
  </span>
  <a id="DisCatSharp_Entities_DiscordGuild_op_Inequality_" data-uid="DisCatSharp.Entities.DiscordGuild.op_Inequality*"></a>
  <h4 id="DisCatSharp_Entities_DiscordGuild_op_Inequality_DisCatSharp_Entities_DiscordGuild_DisCatSharp_Entities_DiscordGuild_" data-uid="DisCatSharp.Entities.DiscordGuild.op_Inequality(DisCatSharp.Entities.DiscordGuild,DisCatSharp.Entities.DiscordGuild)">Inequality(DiscordGuild, DiscordGuild)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="2" sourceendlinenumber="2">Gets whether the two <a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a> objects are not equal.</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="decalaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static bool operator !=(DiscordGuild e1, DiscordGuild e2)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a></td>
        <td><span class="parametername">e1</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">First guild to compare.</p>
</td>
      </tr>
      <tr>
        <td><a class="xref" href="DisCatSharp.Entities.DiscordGuild.html">DiscordGuild</a></td>
        <td><span class="parametername">e2</span></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Second guild to compare.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><a class="xref" href="https://learn.microsoft.com/dotnet/api/system.boolean">Boolean</a></td>
        <td><p sourcefile="api/DisCatSharp/DisCatSharp.Entities.DiscordGuild.yml" sourcestartlinenumber="1" sourceendlinenumber="1">Whether the two guilds are not equal.</p>
</td>
      </tr>
    </tbody>
  </table>
  <h3 id="implements">Implements</h3>
  <div>
      <span class="xref">IEquatable&lt;&gt;</span>
  </div>

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
