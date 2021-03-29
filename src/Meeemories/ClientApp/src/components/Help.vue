<template>
    <div class="help" v-if="opened" @click="close">
        <div class="help__body">
            <a class="help__close" @click="close">&times;</a>
            <div class="help__message" v-if="android" @click="close">
                <p>Androidでダウンロードするには、<i class="material-icons">cloud_download</i>ボタンを長押ししてメニューから「リンクをダウンロードする」を選択します。</p>
                <img src="/img/menu-android.jpg" />
            </div>
            <div class="help__message" v-if="ios">
                <p>iPhone、iPadでダウンロードするには、<i class="material-icons">cloud_download</i>ボタンを長押ししてメニューから「リンク先のファイルをダウンロードする」を選択します。</p>
                <img src="/img/menu-ios.jpg" />
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        computed: {
            ios() {
                return this.$state.ios;
            },
            android() {
                return this.$state.android;
            },
            opened() {
                return this.$state.help;
            }
        },
        methods: {
            close() {
                this.$actions.help(false);
            }
        }
    }
</script>