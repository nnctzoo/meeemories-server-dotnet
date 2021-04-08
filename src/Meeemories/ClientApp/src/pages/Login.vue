<template>
    <section id="login">
        <img src="/img/mstile-150x150.png"/>
        <p class="error" v-if="message">{{message}}</p>
        <input class="input" type="text" v-model="password" @keyup.enter="trigger" placeholder="合言葉"/>
        <button :class="btnClass" @click="login">ログイン</button>
    </section>
</template>
<script>
    export default {
        data() {
            return {
                password: null,
                message: null,
                loading: false,
            }
        },
        computed: {
            btnClass() {
                return {
                    'btn': true,
                    'btn-primary': true,
                    'btn-loading': this.loading
                }
            }
        },
        methods: {
            async login() {
                this.loading = true;
                try {
                    const response = await fetch('/api/token', {
                        method: 'post',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ password: this.password })
                    })
                    const text = await response.text();
                    if (response.ok) {
                        const token = JSON.parse(text);
                        this.message = null;
                        this.$state.token = token;
                        localStorage.setItem('token', token);
                        this.$router.push('/');
                    }
                    else {
                        this.message = '合言葉が違います。';
                        console.error(text);
                    }
                } finally {
                    this.loading = false;
                }
            },
            trigger(evt) {
                if (evt.keyCode == 13) {
                    this.login();
                }
            }
        }
    }
</script>