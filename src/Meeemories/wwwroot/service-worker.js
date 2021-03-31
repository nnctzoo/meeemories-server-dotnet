'use strict';

const CACHE_NAME = '$CACHE_NAME$';
const urlsToCache = [
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

self.addEventListener('fetch', () => {});
