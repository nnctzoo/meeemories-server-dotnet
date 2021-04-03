<template>
    <article class="popup" v-if="media!=null" @touchstart="onTouchStart" @touchend="onTouchEnd">
        <button class="popup__close" @click="close">
            <i class="material-icons">arrow_back</i>
        </button>
        <template v-if="media.type=='Image'">
            <img class="popup__image" :src="thumbnail.src" :srcset="thumbnail.srcset" :sizes="thumbnail.sizes" v-if="loaded">
            <img class="popup__image" :src="large.src" @load="onLoad" v-else>
        </template>
        <template v-else-if="media.type=='Video'">
            <video class="popup__image" autoplay controls @touchstart="onTouchStart" @touchend="onTouchEnd">
                <source :src="video.src" :type="video.mime" :media="video.media" v-for="(video, index) in videoSources" :key="index"/>
            </video>
        </template>
    </article>
</template>
<script>
    export default {
        data() {
            return {
                loaded: false,
            }
        },
        computed: {
            media() {
                return this.$state.popup;
            },
            thumbnail() {
                const images = this.media.sources.filter(src => src.mimeType.startsWith('image'))
                const last = images[images.length - 1];
                const srcset = images.map(img => this.src(img.url) + ' ' + img.width + 'w').join(',');
                const sizes = images.map(img => '(max-width:' + img.width + 'px) ' + img.width + 'px').concat([last.width + 'px']).join(',');
                return {
                    src: this.src(last.url),
                    srcset,
                    sizes
                }
            },
            large() {
                const images = this.media.sources.filter(src => src.mimeType.startsWith('image'))
                const last = images[images.length - 1];
                return {
                    src: this.src(last.url),
                }
            },
            videoSources() {
                const videos = this.media.sources.filter(src => src.mimeType.startsWith('video'));
                const last = videos[videos.length - 1];
                return videos.map(video => ({
                    src: this.src(video.url),
                    mime: video.mimeType,
                    media: 'all and (max-width: ' + video.width + 'px)'
                })).concat([{
                    src: this.src(last.url),
                    mime: last.mimeType,
                    media: null
                }])
            }
        },
        methods: {
            close() {
                this.$actions.popup(null);
                this.$router.go(-1);
            },
            onLoad() {
                this.loaded = true;
            },
            onTouchStart(evt) {
                if (evt.touches.length == 1) {
                    this._touch = {
                        time: new Date().getTime(),
                        x: evt.touches[0].screenX,
                        y: evt.touches[0].screenY,
                    }
                }
            },
            onTouchEnd(evt) {
                if (evt.changedTouches.length == 1 && this._touch) {
                    const diff = {
                        time: new Date().getTime() - this._touch.time,
                        x: evt.changedTouches[0].screenX - this._touch.x,
                        y: evt.changedTouches[0].screenY - this._touch.y,
                    }
                    if (diff.time > 2 && diff.time < 500) {
                        const a = 500 / diff.time;
                        const X = diff.x * diff.x;
                        const Y = diff.y * diff.y;
                        if (X > Y && X * a > 10000) {
                            if (diff.x > 0) {
                                this.$actions.prev();
                            }
                            else {
                                this.$actions.next();
                            }
                        }
                        if (X < Y && Y * a > 10000) {
                            if (diff.y > 0) {
                                this.close();
                            }
                        }
                    }
                }
            },
            src(url) {
                const token = this.$state.token;
                return url + token.substring(token.indexOf('?'));
            }
        },
        mounted() {
            if (this.media == null) {
                return this.$router.push('/');
            }
        }
    }
</script>