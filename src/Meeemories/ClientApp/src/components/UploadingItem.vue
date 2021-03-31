<template>
    <div :class="klass">
        <div class="uploading-item__thumb" ref="thumb"></div>
        <div class="uploading-item__indicator" :style="style"></div>
        <ul class="uploading-item__breadcrumb">
            <li><span>アップロード</span></li>
            <li><span>リサイズ</span></li>
            <li><span>投稿完了</span></li>
        </ul>
         <button class="uploading-item__remove" @click="remove" v-if="deletable"><i class="material-icons">delete</i></button> 
    </div>
</template>
<script>
    export default {
        props: {
            id: String,
            file: [File, Object],
            progress: Number,
            status: String,
            thumbnail: String,
            deletable: Boolean
        },
        computed: {
            klass() {
                return {
                    'uploading-item': true,
                    'uploading-item--uploading': this.status === 'uploading',
                    'uploading-item--uploaded': this.status === 'uploaded',
                    'uploading-item--converting': this.status === 'converting',
                    'uploading-item--succeeded': this.status === 'complete',
                    'uploading-item--faild': this.status === 'fail',
                }
            },
            style() {
                return {
                    width: this.progress + '%'
                }
            },
        },
        methods: {
            setThumb(file) {
                if (file.type.match('image')) {
                    this.setThumbUrl(URL.createObjectURL(file));
                }
                else if (file.type.match('video')) {
                    const video = document.createElement('video');
                    const snap = () => {
                        const canvas = document.createElement('canvas');
                        canvas.width = 128;
                        canvas.height = 128;
                        canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
                        var url = canvas.toDataURL();
                        var success = url.length > 1000;
                        if (success) {
                            this.setThumbUrl(url);
                        }
                        return success;
                    }
                    function loadeddata() {
                        if (snap()) {
                            video.removeEventListener('timeupdate', timeupdate);
                        }
                    }
                    function timeupdate() {
                        if (snap()) {
                            video.removeEventListener('timeupdate', timeupdate);
                            video.pause();
                        }
                    }
                    video.addEventListener('timeupdate', timeupdate);
                    video.addEventListener('loadeddata', loadeddata);
                    video.preload = 'metadata';
                    video.src = URL.createObjectURL(file);
                    video.muted = true;
                    video.playsInline = true;
                    video.play();
                }
            },
            setThumbUrl(url) {
                this.$refs.thumb.style.backgroundImage = "url('" + url + "')";
            },
            src(url) {
                const token = this.$state.token;
                return url + token.substring(token.indexOf('?'));
            },
            async remove() {
                if (confirm('この写真を削除します。')) {
                    const success = await this.$actions.delete(this.id);
                    if (!success) {
                        alert('削除に失敗しました。');
                    }
                }
            }
        },
        watch: {
            thumbnail(url) {
                this.setThumbUrl(this.src(url));
            }
        },
        mounted() {
            this.$actions.startPolling(this.id);

            if (this.thumbnail)
                this.setThumbUrl(this.src(this.thumbnail));

            else if (this.file.constructor == Object)
                this.setThumb(this.file);
        }
    }
</script>