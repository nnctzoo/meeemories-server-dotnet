<template>
    <section id="mypage" class="page page--active">
        <div class="app__description" v-if="uploads.length == 0">
            <p>まだアップロードされた写真がありません</p>
            <p><i class="icon icon__add"></i>ボタンでアップロードしましょう(๑˃̵ᴗ˂̵)و</p>
            <table class="support">
                <caption>複数ファイル選択対応ブラウザ</caption>
                <tbody>
                    <tr><th>iPhone</th><td>Safari</td><td>〇</td></tr>
                    <tr><th>iPad</th><td>Safari</td><td>〇</td></tr>
                    <tr><th>Android</th><td>Chrome</td><td>×</td></tr>
                    <tr><th>Android</th><td>Firefox</td><td>〇</td></tr>
                    <tr><th>PC</th><td>Any</td><td>〇</td></tr>
                </tbody>
            </table>
        </div>
        <UploadingItem v-for="item in uploads" :key="item.id" 
                       :id="item.id" 
                       :progress="item.progress" 
                       :status="item.status" 
                       :thumbnail="item.thumbnail"
                       :deletable="!!item.deleteToken"
                       :file="item.file"></UploadingItem>
    </section>
</template>
<script>
    import UploadingItem from '../components/UploadingItem.vue'
    export default {
        components: {
            UploadingItem
        },
        computed: {
            uploads() {
                return this.$state.uploads;
            }
        },
        async mounted() {
            if (!await this.$actions.authorize()) {
                this.$router.push('/login');
            }
            else {
                this.$actions.load();
            }
        }
    }
</script>