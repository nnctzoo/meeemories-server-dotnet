﻿import Vue from 'vue'
import { ContainerClient } from '@azure/storage-blob'

const version = document.querySelector('meta[name=x-version]').content;

const ua = window.navigator.userAgent.toLowerCase();
const android = ua.indexOf('android') != -1;
const ios = ua.indexOf('ios') != -1 || ua.indexOf('ipod') != -1 || ua.indexOf('ipad') != -1 || (ua.indexOf('macintosh') != -1 && 'ontouchend' in document);

export const state = Vue.observable({
    scroll: window.scrollY + window.innerHeight,
    grid: false,
    uploads: JSON.parse(localStorage.getItem('uploads:' + version) || '[]').filter(o => o.status != 'uploading'),
    medias: [],
    selects:[],
    popup: null,
    help: false,
    token: localStorage.getItem('token:' + version),
    mobile: android || ios,
    android,
    ios,
})

window.addEventListener('scroll', function () {
    state.scroll = window.scrollY + window.innerHeight
})

export const actions = {
    toggleView() {
        state.grid = !state.grid;
    },
    toggleSelect(media) {
        const index = state.selects.indexOf(media);
        if (index != -1) {
            state.selects.splice(index, 1);
            return false;
        }
        else {
            state.selects.push(media);
            return true;
        }
    },
    popup(media) {
        state.popup = media;
    },
    saveScroll() {
        state.savedPosition = window.scrollY;
    },
    loadScroll() {
        window.scrollTo(0, state.savedPosition);
        state.savedPosition = 0;
    },
    prev() {
        const index = state.medias.indexOf(state.popup);
        if (index > 0) {
            actions.popup(state.medias[index - 1]);
        }
    },
    next() {
        const index = state.medias.indexOf(state.popup);
        if (index != -1) {
            actions.popup(state.medias[(index + 1) % state.medias.length]);
        }
    },
    help(flag) {
        state.help = flag;
    },
    login(token) {
        state.token = token;
        localStorage.setItem('token:' + version, token);
    },
    async upload(file) {
        const id = unique(file.name);
        const uploading = Vue.observable({
            id,
            progress: 0,
            status: 'uploading',
            thumbnail: null,
            deleteToken: null,
        })

        if (file.type.startsWith('image/')) {
            extractThumbnailFromImage(file).then(url => {
                uploading.thumbnail = url;
                localStorage.setItem('uploads:' + version, JSON.stringify(state.uploads));
            });
        }

        if (file.type.startsWith('video/')) {
            extractThumbnailFromVideo(file).then(url => {
                uploading.thumbnail = url;
                localStorage.setItem('uploads:' + version, JSON.stringify(state.uploads));
            });
        }

        state.uploads.unshift(uploading);

        if (!state.token)
            throw "expired";

        const totalBytes = file.size;
        const containerClient = new ContainerClient(state.token);
        const blockBlobClient = containerClient.getBlockBlobClient(id);
        try {
            await blockBlobClient.uploadData(file, {
                blobHTTPHeaders: {
                    blobContentType: file.type,
                    blobContentDisposition: 'attachment;'
                },
                onProgress: function (progress) {
                    uploading.progress = ~~(progress.loadedBytes / totalBytes * 100)
                }
            });
        }
        catch (err) {
            state.uploads.splice(state.uploads.indexOf(uploading), 1);

            if (err && err.statusCode == '403')
                throw "expired";

            throw err;
        }
        try {
            const response = await fetch_retry('/api/upload', {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id })
            }, 3)
            if (response.ok || response.statusCode == 409) {
                uploading.status = 'ready';
                const datum = await response.json();
                uploading.deleteToken = datum.deleteToken;
            }
        }
        catch (err) {
            state.uploads.splice(state.uploads.indexOf(uploading), 1);
            throw err;
        }
        try {
            localStorage.setItem('uploads:' + version, JSON.stringify(state.uploads));
        }
        catch { }
        this.startPolling(uploading);
    },
    startPolling(uploading) {
        const handle = setInterval(async () => {
            if (await this.polling(uploading)) {
                clearInterval(handle);
            }
        }, 2000);
    },
    async polling(uploading) {
        if (uploading.status == 'uploading' || uploading.status == 'complete' || uploading.status == 'fail')
            return true;

        const response = await fetch('/api/medias/' + uploading.id, {
            method: 'get',
            headers: {
                'Cache-Control': 'no-cache'
            }
        });

        if (!response.ok)
            return false

        const datum = await response.json();
        const status = datum.status.toLowerCase();
        if (uploading.status != status)
            uploading.status = status;

        if (status == 'fail')
            return true;

        if (status == 'complete') {
            uploading.thumbnail = datum.sources.find(src => src.width >= 400 && src.mimeType.startsWith('image')).url;
            localStorage.setItem('uploads:' + version, JSON.stringify(state.uploads));
            return true;
        }

        return false;
    },
    async delete(id) {
        const item = state.uploads.find(item => item.id == id);
        if (!item) {
            console.error('cannot delete ' + id);
            return false;
        }
        const response = await fetch('/api/delete/' + id + '?token=' + item.deleteToken, {
            method: 'delete',
        });
        if (response.ok || response.status == 404) {
            const index = state.uploads.indexOf(item);
            state.uploads.splice(index, 1);
            localStorage.setItem('uploads:' + version, JSON.stringify(state.uploads));
            return true;
        }
        return false;
    },
    async load(next) {
        let url = '/api/medias/';

        if (next && state.medias.length > 0)
            url += '?skipToken=' + state.medias[state.medias.length - 1].id;

        const response = await fetch(url, {
            method: 'get',
            headers: {
                'Cache-Control': 'no-cache'
            }
        });

        if (response.ok) {
            const data = await response.json();
            if (next) {
                while (data.length > 0) state.medias.push(data.shift());
            } else {
                state.medias.splice(0);
                while (data.length > 0) state.medias.unshift(data.pop());
            }
        }
    },
    async authorize() {
        const token = state.token;
        if (!token)
            return false;
        const url = token.replace('?', '/check.txt?');
        try {
            const response = await fetch(url, {
                method: 'get',
                mode: 'cors',
                headers: {
                    'Cache-Control': 'no-cache'
                }
            });
            return response.ok;
        }
        catch {
            return false;
        }
    }
}

for (let uploading of state.uploads) {
    actions.startPolling(uploading);
}

const fetch_retry = async (url, options, n) => {
    try {
        return await fetch(url, options);
    } catch (err) {
        if (n === 1) throw err;
        return await fetch_retry(url, options, n - 1);
    }
};

function unique(filename) {
    return (Number.MAX_SAFE_INTEGER - new Date().getTime()) + '-' + uuid() + '-' + filename
}

function uuid() {
    let uuid = '', i, random;
    for (i = 0; i < 32; i++) {
        random = Math.random() * 16 | 0;

        if (i == 8 || i == 12 || i == 16 || i == 20) {
            uuid += '-'
        }
        uuid += (i == 12 ? 4 : (i == 16 ? (random & 3 | 8) : random)).toString(16);
    }
    return uuid;
}

function extractThumbnailFromImage(file) {
    return new Promise(function (resolve) {
        const img = new Image();
        img.onload = function () {
            const canvas = document.createElement('canvas');
            const aspect = img.height / img.width;
            canvas.width = 400;
            canvas.height = 400 * aspect;
            canvas.getContext('2d').drawImage(img, 0, 0, canvas.width, canvas.height);
            resolve(canvas.toDataURL());
        }
        img.src = URL.createObjectURL(file);
    });
}

function extractThumbnailFromVideo(file) {
    return new Promise(function (resolve) {
        const video = document.createElement('video');
        const snap = () => {
            const aspect = video.videoHeight / video.videoWidth;
            const canvas = document.createElement('canvas');
            canvas.width = 400;
            canvas.height = 400 * aspect;
            canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
            const url = canvas.toDataURL();
            const success = url.length > 1000;
            return success ? url : null;
        }
        function timeupdate() {
            const url = snap();
            if (url) {
                video.removeEventListener('timeupdate', timeupdate);
                video.pause();
                resolve(url);
            }
        }
        video.addEventListener('timeupdate', timeupdate);
        video.preload = 'metadata';
        video.src = URL.createObjectURL(file);
        video.muted = true;
        video.playsInline = true;
        video.play();
    });
}
