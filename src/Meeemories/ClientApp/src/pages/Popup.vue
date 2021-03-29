<template>
    <article class="popup" v-if="media!=null">
        <button class="popup__close" @click="close">
            <i class="material-icons">arrow_back</i>
        </button>
        <template v-if="media.type=='Image'">
            <img class="popup__image" :src="thumbnail.src" :srcset="thumbnail.srcset" :sizes="thumbnail.sizes" v-if="loaded">
            <img class="popup__image" :src="large.src" @load="onLoad" v-else>
        </template>
        <template v-else-if="media.type=='Video'">
            <video class="popup__image" autoplay controls>
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