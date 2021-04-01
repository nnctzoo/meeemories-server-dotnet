<template>
    <div :class="klass">
        <div class="uploading-item__thumb" :style="thumbnailStyle"></div>
        <div class="uploading-item__indicator" :style="progressStyle"></div>
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
                    'uploading-item--uploaded': this.status === 'ready',
                    'uploading-item--converting': this.status === 'converting',
                    'uploading-item--succeeded': this.status === 'complete',
                    'uploading-item--faild': this.status === 'fail',
                }
            },
            thumbnailStyle() {
                if (!this.thumbnail)
                    return {}

                const url = this.thumbnail.startsWith('data') ? this.thumbnail : this.src(this.thumbnail);

                return {
                    'background-image': 'url(' + url + ')'
                }
            },
            progressStyle() {
                return {
                    width: this.progress + '%'
                }
            },
        },
        methods: {
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
        }
    }
</script>
