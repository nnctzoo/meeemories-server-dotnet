import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from './pages/Home.vue'
import Login from './pages/Login.vue'
import MyPage from './pages/MyPage.vue'

Vue.use(VueRouter)

export const router = new VueRouter({
    mode: 'hash',
    routes: [
        { path: '/', name: 'home', component: Home },
        { path: '/login', name: 'login', component: Login },
        { path: '/mypage', name: 'mypage', component: MyPage },
    ],
    scrollBehavior(to, from, savedPosition) {
        return savedPosition ? savedPosition : { x: 0, y: 0 }
    }
})
