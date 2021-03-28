<template>
    <h1 :class="{'logo':true,'logo--hidden':hidden}">meeemories</h1>
</template>
<script>
    export default {
        data() {
            return {
                hidden: false,
            }
        },
        mounted() {
            window.addEventListener('scroll', () => {
                this.hidden = window.scrollY > 10;
            });
        }
    }
</script>