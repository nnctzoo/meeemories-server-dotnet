<template>
    <section id="mypage" class="page page--active">
        <div class="app__description" v-if="uploads.length == 0">
            <p>まだアップロードされた写真がありません＞＜</p>
            <p><i class="icon icon__add"></i>ボタンでアップロードしましょう(๑˃̵ᴗ˂̵)و</p>
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