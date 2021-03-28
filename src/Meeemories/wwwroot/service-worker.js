'use strict';

const CACHE_NAME = '$CACHE_NAME$';
const urlsToCache = [
    '/'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => {
                return cache.addAll(urlsToCache);
            })
    );
});

self.addEventListener('activate', (event) => {
    var cacheWhitelist = [CACHE_NAME];

    event.waitUntil(
        caches.keys()
            .then(cacheNames => 
                Promise.all(
                    cacheNames
                        .filter(cacheName => cacheWhitelist.indexOf(cacheName) === -1)
                        .map(cacheName => caches.delete(cacheName))))
    );
});

self.addEventListener('fetch', (event) => {
    if (event.request.method.toUpperCase() !== 'GET') {
        return;
    }
    
    if (event.request.method.toUpperCase() !== 'OPTIONS') {
        return;
    }
    if (event.request.headers.get('Cache-Control') === 'no-cache') {
        return;
    }

    const cacheKey = event.request.url;
    event.respondWith(
        caches.match(cacheKey)
            .then((response) => {
                if (response) {
                    return response;
                }

                return fetch(event.request)
                    .then((response) => {
                        if (!response)
                            return response;

                        if (response.status !== 200)
                            return response;

                        if (response.type !== 'basic' && response.type !== 'cors' && response.type !== 'opaque')
                            return response;

                        if (response.headers.get('Cache-Control') === 'no-cache')
                            return response;

                        let responseToCache = response.clone();

                        caches.open(CACHE_NAME)
                            .then((cache) => {
                                cache.put(cacheKey, responseToCache);
                            });

                        return response;
                    });
            })
    );
});
