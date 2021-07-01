<template>
    <div :class="klass">
        <div class="media-item__space" :style="aspectStyle"></div>
        <img class="media-item__thumb" @click="onClick"  ref="thumb">
        <img class="media-item__cover" :style="coverStyle" @click="onClick">
        <i class="media-item__type material-icons" @click="onClick" v-if="info.type=='Video'">play_circle_outline</i>
        <a class="media-item__download material-icons" :href="src(info.url)" :download="short(info.id)">cloud_download</a>
    </div>
</template>
<script>
    export default {
        props: {
            info: Object,
        },
        data() {
            const last = this.info.sources[this.info.sources.length - 1];
            return {
                loaded: false,
                visibled: false,
                selected: false,
                animating: false,
                images: this.info.sources.filter(src => src.mimeType.startsWith('image')),
                aspect: last.height / last.width
            }
        },
        computed: {
            klass() {
                return {
                    'media-item': true,
                    'media-item--animating': this.animating,
                    'media-item--selected': this.selected,
                    'media-item--visibled': this.visibled,
                    'media-item--loaded': this.loaded,
                }
            },
            aspectStyle() {
                return {
                    'padding-top': ~~(this.aspect * 100) + '%'
                };
            },
            coverStyle() {
                return {
                    'background-image': 'url(' + this.src(this.images[0].url) + ')'
                }
            }
        },
        methods: {
            onClick() {
                this.$actions.saveScroll();
                this.$actions.popup(this.info);
                this.$router.push('/popup');
            },
            onLoad() {
                this.loaded = true;
            },
            beforeDownload(evt) {
                if (this.$state.mobile) {
                    this.$actions.help(true);
                    evt.preventDefault(); 
                }
            },
            tryVisualize(pos) {
                const viewportBottom = pos;
                const mediaTop = this.$el.offsetTop;

                if (
                    mediaTop <= viewportBottom
                ) {
                    this.visibled = true;
                    if (!this.$refs.thumb.srcset && !this.$refs.thumb.src) {
                        this.$refs.thumb.onload = () => this.loaded = true;

                        if (this.images.length > 0) {
                            this.$refs.thumb.srcset = this.images.map(img => this.src(img.url) + ' ' + img.width + 'w').join(',');
                            this.$refs.thumb.sizes = this.images.map(img => '(max-width:' + img.width + 'px) ' + img.width + 'px').concat([this.images[this.images.length - 1].width + 'px']).join(',');
                        }

                        this.$refs.thumb.src = this.src(this.images[this.images.length - 1].url);
                    }
                }
            },
            src(url) {
                const token = this.$state.token;
                return url + token.substring(token.indexOf('?'));
            },
            select() {
                this.selected = this.$actions.toggleSelect(this.info);
            },
            short(id) {
                return id.substring(0, 16) + id.substring(id.length - 16);
            }
        },
        watch: {
            '$state.scroll'(pos) {
                this.tryVisualize(pos);
            }
        },
        mounted() {
            this.tryVisualize(this.$state.scroll);
        }
    }
</script>