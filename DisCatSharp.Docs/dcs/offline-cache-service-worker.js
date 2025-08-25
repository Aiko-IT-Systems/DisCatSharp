importScripts('https://storage.googleapis.com/workbox-cdn/releases/6.5.4/workbox-sw.js');

if (workbox) {
	console.log("Workbox is loaded, senpai!");

	workbox.precaching.precacheAndRoute([{ url: '/index.html', revision: null }]);

	workbox.routing.registerRoute(
		({ url }) =>
			url.pathname.startsWith('/images/') || url.pathname.startsWith('/public/'),
		new workbox.strategies.CacheFirst({
			cacheName: 'static-assets',
			plugins: [
				new workbox.expiration.ExpirationPlugin({
					maxEntries: 100,
					maxAgeSeconds: 7 * 24 * 60 * 60, // 1 week
				}),
			],
		})
	);

	workbox.routing.registerRoute(
		({ request }) =>
			request.destination === 'image' ||
			request.destination === 'style' ||
			request.destination === 'script',
		new workbox.strategies.StaleWhileRevalidate({
			cacheName: 'asset-cache',
			plugins: [
				new workbox.expiration.ExpirationPlugin({
					maxEntries: 200,
					maxAgeSeconds: 30 * 24 * 60 * 60,
				}),
			],
		})
	);

	workbox.routing.registerRoute(
		({ request }) => request.mode === 'navigate',
		new workbox.strategies.NetworkFirst({
			cacheName: 'pages-cache',
			plugins: [
				new workbox.expiration.ExpirationPlugin({
					maxEntries: 50,
					maxAgeSeconds: 7 * 24 * 60 * 60,
				}),
			],
		})
	);

	self.addEventListener("message", (event) => {
		if (event.data && event.data.type === "SKIP_WAITING") {
			self.skipWaiting();
		}
	});

} else {
	console.log("Workbox didn't load, baka!");
}
