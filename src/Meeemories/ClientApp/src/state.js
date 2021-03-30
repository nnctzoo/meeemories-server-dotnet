import Vue from 'vue'
import { ContainerClient } from '@azure/storage-blob'

const ua = window.navigator.userAgent.toLowerCase();
const android = ua.indexOf('android') != -1;
const ios = ua.indexOf('ios') != -1 || ua.indexOf('ipod') != -1 || ua.indexOf('ipad') != -1 || (ua.indexOf('macintosh') != -1 && 'ontouchend' in document);

export const state = Vue.observable({
    scroll: window.scrollY + window.innerHeight,
    grid: false,
    uploads: JSON.parse(localStorage.getItem('uploads') || '[]'),
    medias: [],
    selects:[],
    popup: null,
    help: false,
    token: localStorage.getItem('token'),
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
    help(flag) {
        state.help = flag;
    },
    async upload(file) {
        const id = unique(file.name);
        const uploading = Vue.observable({
            id,
            file,
            progress: 0,
            status: 'uploading',
            thumbnail: null,
            deleteToken: null,
        })

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
            throw err;
        }
        try {
            await fetch('/api/upload', {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id })
            })
        }
        catch (err) {
            console.error(err);
        }

        uploading.status = 'uploaded';

        const polling = () => {
            setTimeout(async () => {
                const response = await fetch('/api/medias/' + id, {
                    method: 'get',
                    headers: {
                        'Cache-Control': 'no-cache'
                    }
                });
                if (response.ok) {
                    const datum = await response.json();
                    const status = datum.status.toLowerCase();
                    if (uploading.status != status)
                        uploading.status = status;

                    if (status != 'complete' && status != 'fail')
                        polling();

                    if (status == 'complete') {
                        uploading.thumbnail = datum.sources.find(src => src.width > 400 && src.mimeType.startsWith('image')).url;
                        uploading.deleteToken = datum.deleteToken;
                        localStorage.setItem('uploads', JSON.stringify(state.uploads));
                    }
                }
            }, 2000);
        }

        polling();
        
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
            localStorage.setItem('uploads', JSON.stringify(state.uploads));
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
            while (data.length > 0) {
                state.medias.unshift(data.pop());
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