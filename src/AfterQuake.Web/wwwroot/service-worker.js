const CACHE_VERSION = 'v1';
const STATIC_CACHE = `afterquake-static-${CACHE_VERSION}`;
const API_CACHE = `afterquake-api-${CACHE_VERSION}`;
const PAGE_CACHE = `afterquake-pages-${CACHE_VERSION}`;

const CRITICAL_PAGES = [
  '/',
  '/Emergency/Report',
  '/Person',
  '/HelpRequest/Create',
  '/Shelter/Map',
  '/Account/Login',
  '/Account/Register',
  '/Guide',
  '/Directory'
];

const STATIC_ASSETS = [
  '/css/site.css',
  '/js/site.js',
  '/lib/bootstrap/js/bootstrap.js',
  '/lib/jquery/jquery.js',
  '/lib/jquery-validation/jquery.validate.js',
  '/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js',
  '/favicon.ico'
];

const CDN_ASSETS = [
  'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css',
  'https://cdn.tailwindcss.com',
  'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js',
  'https://cdn.jsdelivr.net/npm/microsoft/signalr@8.0.0/dist/browser/signalr.min.js'
];

const API_PATTERNS = [/\/api\//, /\/hubs\//];

const OFFLINE_PAGE = '/offline.html';

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(STATIC_CACHE).then((cache) => {
      return cache.addAll(STATIC_ASSETS.concat(CDN_ASSETS));
    })
  );
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys
          .filter((key) => key !== STATIC_CACHE && key !== API_CACHE && key !== PAGE_CACHE)
          .map((key) => caches.delete(key))
      );
    })
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  if (request.method !== 'GET') return;

  if (url.pathname === OFFLINE_PAGE) {
    event.respondWith(caches.match(request));
    return;
  }

  const isStatic = STATIC_ASSETS.some((asset) => url.pathname === asset) ||
    CDN_ASSETS.some((cdn) => url.href.startsWith(cdn)) ||
    url.pathname.startsWith('/css/') ||
    url.pathname.startsWith('/js/') ||
    url.pathname.startsWith('/lib/') ||
    url.pathname.startsWith('/fonts/') ||
    url.pathname.startsWith('/images/');

  const isApi = url.origin === self.location.origin &&
    API_PATTERNS.some((pattern) => pattern.test(url.pathname));

  const isCriticalPage = url.origin === self.location.origin &&
    CRITICAL_PAGES.some((page) => url.pathname === page || url.pathname === page + '/');

  if (isStatic) {
    event.respondWith(cacheFirst(request, STATIC_CACHE));
  } else if (isApi) {
    event.respondWith(networkFirst(request, API_CACHE));
  } else if (isCriticalPage) {
    event.respondWith(networkFirst(request, PAGE_CACHE));
  }
});

async function cacheFirst(request, cacheName) {
  const cached = await caches.match(request);
  if (cached) return cached;
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(cacheName);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    return caches.match(OFFLINE_PAGE);
  }
}

async function networkFirst(request, cacheName) {
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(cacheName);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    const cached = await caches.match(request);
    if (cached) return cached;
    if (request.mode === 'navigate') {
      return caches.match(OFFLINE_PAGE);
    }
    return new Response('Offline', { status: 503 });
  }
}
