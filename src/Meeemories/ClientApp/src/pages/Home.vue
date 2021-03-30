<template>
    <section id="home" class="page page--active">
        <div class="list__container">
            <Media v-for="item in items" :key="item.id" :info="item"></Media>
        </div>
        <div class="list__next" ref="next"></div>
        <button class="fab app__view-switch material-icons" @click="toggleView"></button>
        <button class="fab app__download material-icons" @click="download">cloud_download</button>
    </section>
</template>
<script>
    import Media from '../components/Media.vue'
    export default {
        components: {
            Media
        },
        computed: {
            items() {
                return this.$state.medias;
            }
        },
        methods: {
            toggleView() {
                this.$actions.toggleView();
            },
            download() {
                this.$actions.download();
            }
        },
        async mounted() {
            if (!await this.$actions.authorize()) {
                this.$router.push('/login');
            }
            else {
                new IntersectionObserver(entries => {
                    for (let entry of entries) {
                        if (entry.isIntersecting) {
                            this.$actions.load(true);
                        }
                    }
                }).observe(this.$refs.next);
            }
        }
    }
</script>