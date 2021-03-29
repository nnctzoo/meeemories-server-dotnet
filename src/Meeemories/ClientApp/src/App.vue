<template>
    <div class="app" :class="{'app--grid-view': grid, 'app--selecting': selecting}">
        <header class="app__header" v-if="!$route.meta.naked">
            <Logo></Logo>
        </header>
        <main class="app__main" data-target="app.main">
            <router-view></router-view>
        </main>
        <footer class="app__footer" v-if="!$route.meta.naked">
            <ul class="actions">
                <li class="actions__space"></li>
                <li class="actions__item" @click="go('/')">
                    <i :class="['icon','icon__home',$route.name=='home'?'icon--active':null]"></i>
                </li>
                <li class="actions__space"></li>
                <li class="actions__item">
                    <label>
                        <i class="icon icon__add"></i>
                        <input type="file" name="files[]" accept="image/*,video/*" multiple="multiple" @change="upload">
                    </label>
                </li>
                <li class="actions__space"></li>
                <li class="actions__item" @click="go('/mypage')">
                    <i :class="['icon','icon__person',$route.name=='mypage'?'icon--active':null]"></i>
                </li>
                <li class="actions__space"></li>
            </ul>
        </footer>
        <Help></Help>
    </div>
</template>
<script>
    import Logo from './components/Logo.vue';
    import Help from './components/Help.vue';
    export default {
        components: {
            Logo, Help
        },
        computed: {
            grid() {
                return this.$state.grid;
            },
            selecting() {
                return this.$state.selects.length > 0
            }
        },
        methods: {
            go(path) {
                this.$router.push(path);
            },
            upload(evt) {
                Promise.all(
                    Array.from(evt.target.files)
                        .map(file => this.$actions.upload(file))
                ).catch(() => {
                    this.$router.push('/login');
                });

                evt.target.value = '';

                if (this.$route.path != '/mypage') {
                    this.$router.push('/mypage');
                }
            }
        }
    }
</script>