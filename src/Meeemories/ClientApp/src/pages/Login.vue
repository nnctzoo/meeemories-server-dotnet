<template>
    <section>
        <p>{{message}}</p>
        <input type="text" v-model="password" />
        <button @click="login">Login</button>
    </section>
</template>
<script>
    export default {
        data() {
            return {
                password: null,
                message: null
            }
        },
        methods: {
            async login() {
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
                    this.message = 'パスワードが違います。';
                    console.error(text);
                }
            }
        }
    }
</script>