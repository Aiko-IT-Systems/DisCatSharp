importScripts('https://storage.googleapis.com/workbox-cdn/releases/6.5.4/workbox-sw.js');

if (workbox) {
	console.log("Workbox is loaded, senpai!");
	self.skipWaiting();

	// Keep first-party images available offline without intercepting CDN requests.
	workbox.routing.registerRoute(
		({ request, url }) =>
			url.origin === self.location.origin && request.destination === 'image',
		new workbox.strategies.CacheFirst({
			cacheName: 'dcs-images-v2',
			plugins: [
				new workbox.cacheableResponse.CacheableResponsePlugin({ statuses: [200] }),
				new workbox.expiration.ExpirationPlugin({
					maxEntries: 100,
					maxAgeSeconds: 7 * 24 * 60 * 60, // 1 week
				}),
			],
		})
	);

	// CDN resources already have their own caching. Only cache successful same-origin styles and scripts.
	workbox.routing.registerRoute(
		({ request, url }) =>
			url.origin === self.location.origin && (
			request.destination === 'style' ||
			request.destination === 'script' ||
			request.destination === 'worker'
			),
		new workbox.strategies.StaleWhileRevalidate({
			cacheName: 'dcs-assets-v2',
			plugins: [
				new workbox.cacheableResponse.CacheableResponsePlugin({ statuses: [200] }),
				new workbox.expiration.ExpirationPlugin({
					maxEntries: 200,
					maxAgeSeconds: 30 * 24 * 60 * 60,
				}),
			],
		})
	);

	workbox.routing.registerRoute(
		({ request, url }) =>
			url.origin === self.location.origin && request.mode === 'navigate',
		new workbox.strategies.NetworkFirst({
			cacheName: 'dcs-pages-v2',
			plugins: [
				new workbox.cacheableResponse.CacheableResponsePlugin({ statuses: [200] }),
				new workbox.expiration.ExpirationPlugin({
					maxEntries: 50,
					maxAgeSeconds: 7 * 24 * 60 * 60,
				}),
			],
		})
	);

	self.addEventListener("activate", (event) => {
		const retiredCaches = new Set(['static-assets', 'asset-cache', 'pages-cache']);
		event.waitUntil(
			caches.keys()
				.then((cacheNames) => Promise.all(
					cacheNames
						.filter((cacheName) => retiredCaches.has(cacheName))
						.map((cacheName) => caches.delete(cacheName))
				))
				.then(() => self.clients.claim())
		);
	});

	self.addEventListener("message", (event) => {
		if (event.data && event.data.type === "SKIP_WAITING") {
			self.skipWaiting();
		}
	});

} else {
	console.log("Workbox didn't load, baka!");
}
