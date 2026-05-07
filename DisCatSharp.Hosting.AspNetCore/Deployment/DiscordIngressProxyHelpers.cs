using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Routing;

namespace DisCatSharp.Hosting.AspNetCore.Deployment;

/// <summary>
///     Generates starter reverse-proxy configuration snippets for the DisCatSharp ASP.NET Core ingress surface.
/// </summary>
/// <remarks>
///     The generated snippets intentionally preserve the package's default routing behavior by forwarding the
///     entire ingress surface and the <c>X-Forwarded-Prefix</c> header expected by the URL helpers.
/// </remarks>
public static class DiscordIngressProxyHelpers
{
	/// <summary>
	///     Creates an NGINX configuration snippet that forwards the public ingress surface to the ASP.NET Core app.
	/// </summary>
	/// <param name="publicBaseUrl">The externally visible base URL.</param>
	/// <param name="upstreamBaseUrl">The base URL where the ASP.NET Core app listens internally.</param>
	/// <param name="options">Optional ingress route options. Defaults to package defaults.</param>
	/// <returns>An NGINX location snippet.</returns>
	public static string CreateNginxConfig(Uri publicBaseUrl, Uri upstreamBaseUrl, DiscordAspNetCoreIngressOptions? options = null)
	{
		var publicUrls = DiscordIngressPublicUrls.Create(publicBaseUrl, options);
		var upstreamUrls = DiscordIngressPublicUrls.Create(upstreamBaseUrl, options);

		return $$"""
		# DisCatSharp ASP.NET Core ingress behind NGINX
		location = {{publicUrls.IngressRootPath}} {
		    proxy_pass {{upstreamUrls.IngressRootUrl.AbsoluteUri.TrimEnd('/')}};
		    proxy_http_version 1.1;
		    proxy_set_header Host $host;
		    proxy_set_header X-Forwarded-Host $host;
		    proxy_set_header X-Forwarded-Proto $scheme;
		    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
		    proxy_set_header X-Forwarded-Prefix {{publicUrls.IngressRootPath}};
		}

		location {{EnsureTrailingSlash(publicUrls.IngressRootPath)}} {
		    proxy_pass {{EnsureTrailingSlash(upstreamUrls.IngressRootUrl.AbsoluteUri)}};
		    proxy_http_version 1.1;
		    proxy_set_header Host $host;
		    proxy_set_header X-Forwarded-Host $host;
		    proxy_set_header X-Forwarded-Proto $scheme;
		    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
		    proxy_set_header X-Forwarded-Prefix {{publicUrls.IngressRootPath}};
		}
		""";
	}

	/// <summary>
	///     Creates an Apache httpd configuration snippet that forwards the public ingress surface to the ASP.NET Core app.
	/// </summary>
	/// <param name="publicBaseUrl">The externally visible base URL.</param>
	/// <param name="upstreamBaseUrl">The base URL where the ASP.NET Core app listens internally.</param>
	/// <param name="options">Optional ingress route options. Defaults to package defaults.</param>
	/// <returns>An Apache reverse-proxy snippet.</returns>
	public static string CreateApacheConfig(Uri publicBaseUrl, Uri upstreamBaseUrl, DiscordAspNetCoreIngressOptions? options = null)
	{
		var publicUrls = DiscordIngressPublicUrls.Create(publicBaseUrl, options);
		var upstreamUrls = DiscordIngressPublicUrls.Create(upstreamBaseUrl, options);

		return $$"""
		# DisCatSharp ASP.NET Core ingress behind Apache httpd
		ProxyPreserveHost On
		RequestHeader set X-Forwarded-Proto expr=%{REQUEST_SCHEME}
		RequestHeader set X-Forwarded-Host expr=%{HTTP_HOST}
		RequestHeader append X-Forwarded-For %{REMOTE_ADDR}s
		RequestHeader set X-Forwarded-Prefix "{{publicUrls.IngressRootPath}}"

		ProxyPass "{{publicUrls.IngressRootPath}}" "{{upstreamUrls.IngressRootUrl.AbsoluteUri.TrimEnd('/')}}"
		ProxyPassReverse "{{publicUrls.IngressRootPath}}" "{{upstreamUrls.IngressRootUrl.AbsoluteUri.TrimEnd('/')}}"
		ProxyPass "{{EnsureTrailingSlash(publicUrls.IngressRootPath)}}" "{{EnsureTrailingSlash(upstreamUrls.IngressRootUrl.AbsoluteUri)}}"
		ProxyPassReverse "{{EnsureTrailingSlash(publicUrls.IngressRootPath)}}" "{{EnsureTrailingSlash(upstreamUrls.IngressRootUrl.AbsoluteUri)}}"
		""";
	}

	/// <summary>
	///     Creates an NGINX configuration snippet tailored for a Docker network where the ASP.NET Core app is addressed by service name.
	/// </summary>
	/// <param name="publicBaseUrl">The externally visible base URL.</param>
	/// <param name="upstreamServiceName">The Docker service name for the ASP.NET Core container.</param>
	/// <param name="upstreamPort">The ASP.NET Core container port.</param>
	/// <param name="options">Optional ingress route options. Defaults to package defaults.</param>
	/// <returns>An NGINX location snippet.</returns>
	public static string CreateDockerNginxConfig(
		Uri publicBaseUrl,
		string upstreamServiceName,
		int upstreamPort,
		DiscordAspNetCoreIngressOptions? options = null)
		=> CreateNginxConfig(publicBaseUrl, CreateDockerUpstreamBaseUrl(upstreamServiceName, upstreamPort), options);

	/// <summary>
	///     Creates Traefik Docker labels that route the public ingress surface to the ASP.NET Core container.
	/// </summary>
	/// <param name="publicBaseUrl">The externally visible base URL.</param>
	/// <param name="routerName">The Traefik router and service name prefix.</param>
	/// <param name="containerPort">The container port exposed by the ASP.NET Core app.</param>
	/// <param name="options">Optional ingress route options. Defaults to package defaults.</param>
	/// <param name="upstreamBaseUrl">
	///     Optional upstream base URL used to compute path rewrites. Defaults to <c>http://internal/</c>.
	/// </param>
	/// <returns>The Traefik labels keyed by label name.</returns>
	public static IReadOnlyDictionary<string, string> CreateTraefikDockerLabels(
		Uri publicBaseUrl,
		string routerName,
		int containerPort,
		DiscordAspNetCoreIngressOptions? options = null,
		Uri? upstreamBaseUrl = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(routerName);
		ValidatePort(containerPort, nameof(containerPort));

		var publicUrls = DiscordIngressPublicUrls.Create(publicBaseUrl, options);
		var upstreamUrls = DiscordIngressPublicUrls.Create(upstreamBaseUrl ?? new Uri("http://internal/"), options);
		var middlewareName = $"{routerName}-headers";
		var rewriteName = $"{routerName}-rewrite";
		var rule = CreateTraefikRule(publicBaseUrl, publicUrls.IngressRootPath);
		var pathRegex = $"^{Regex.Escape(publicUrls.IngressRootPath)}(/.*)?$";
		var replacement = $"{upstreamUrls.IngressRootPath}$1";

		Dictionary<string, string> labels = new(StringComparer.Ordinal)
		{
			["traefik.enable"] = "true",
			[$"traefik.http.routers.{routerName}.rule"] = rule,
			[$"traefik.http.routers.{routerName}.service"] = routerName,
			[$"traefik.http.routers.{routerName}.middlewares"] = $"{rewriteName},{middlewareName}",
			[$"traefik.http.services.{routerName}.loadbalancer.server.port"] = containerPort.ToString(),
			[$"traefik.http.middlewares.{middlewareName}.headers.customrequestheaders.X-Forwarded-Prefix"] = publicUrls.IngressRootPath,
			[$"traefik.http.middlewares.{rewriteName}.replacepathregex.regex"] = pathRegex,
			[$"traefik.http.middlewares.{rewriteName}.replacepathregex.replacement"] = replacement
		};

		labels[$"traefik.http.routers.{routerName}.entrypoints"] = publicBaseUrl.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
			? "websecure"
			: "web";

		if (publicBaseUrl.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
			labels[$"traefik.http.routers.{routerName}.tls"] = "true";

		return labels;
	}

	/// <summary>
	///     Creates a Caddy configuration snippet tailored for a Docker network where the ASP.NET Core app is addressed by service name.
	/// </summary>
	/// <param name="publicBaseUrl">The externally visible base URL.</param>
	/// <param name="upstreamServiceName">The Docker service name for the ASP.NET Core container.</param>
	/// <param name="upstreamPort">The ASP.NET Core container port.</param>
	/// <param name="options">Optional ingress route options. Defaults to package defaults.</param>
	/// <param name="upstreamBaseUrl">
	///     Optional upstream base URL used to compute path rewrites. Defaults to <c>http://internal/</c>.
	/// </param>
	/// <returns>A Caddyfile snippet.</returns>
	public static string CreateDockerCaddyConfig(
		Uri publicBaseUrl,
		string upstreamServiceName,
		int upstreamPort,
		DiscordAspNetCoreIngressOptions? options = null,
		Uri? upstreamBaseUrl = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(upstreamServiceName);
		ValidatePort(upstreamPort, nameof(upstreamPort));

		var publicUrls = DiscordIngressPublicUrls.Create(publicBaseUrl, options);
		var upstreamUrls = DiscordIngressPublicUrls.Create(upstreamBaseUrl ?? new Uri("http://internal/"), options);
		var siteAddress = publicBaseUrl.IsDefaultPort
			? publicBaseUrl.Host
			: $"{publicBaseUrl.Host}:{publicBaseUrl.Port}";
		var upstreamAuthority = $"{upstreamServiceName}:{upstreamPort}";

		return $$"""
		# DisCatSharp ASP.NET Core ingress behind Caddy
		{{siteAddress}} {
		    @discatsharp_ingress path {{publicUrls.IngressRootPath}} {{publicUrls.IngressRootPath}}/*
		    handle @discatsharp_ingress {
		        uri strip_prefix {{publicUrls.IngressRootPath}}
		        rewrite * {{upstreamUrls.IngressRootPath}}{uri}
		        reverse_proxy {{upstreamAuthority}} {
		            header_up Host {http.request.host}
		            header_up X-Forwarded-Host {http.request.host}
		            header_up X-Forwarded-Proto {http.request.scheme}
		            header_up X-Forwarded-Prefix {{publicUrls.IngressRootPath}}
		        }
		    }
		}
		""";
	}

	private static Uri CreateDockerUpstreamBaseUrl(string upstreamServiceName, int upstreamPort)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(upstreamServiceName);
		ValidatePort(upstreamPort, nameof(upstreamPort));

		UriBuilder builder = new("http", upstreamServiceName, upstreamPort);
		return builder.Uri;
	}

	private static string CreateTraefikRule(Uri publicBaseUrl, string ingressRootPath)
	{
		StringBuilder builder = new();
		builder.Append("Host(`");
		builder.Append(publicBaseUrl.Host);
		builder.Append("`)");
		builder.Append(" && PathPrefix(`");
		builder.Append(ingressRootPath);
		builder.Append("`)");
		return builder.ToString();
	}

	private static string EnsureTrailingSlash(string value)
		=> value.EndsWith('/') ? value : $"{value}/";

	private static void ValidatePort(int port, string paramName)
	{
		if (port is <= 0 or > 65535)
			throw new ArgumentOutOfRangeException(paramName, "Ports must be between 1 and 65535.");
	}
}
