import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from './pages/Home.vue'
import Login from './pages/Login.vue'
import MyPage from './pages/MyPage.vue'
import Popup from './pages/Popup.vue'
import Meet from './pages/Meet.vue'

Vue.use(VueRouter)

export const router = new VueRouter({
    mode: 'hash',
    routes: [
        { path: '/', name: 'home', component: Home },
        { path: '/login', name: 'login', component: Login, meta: { naked: true } },
        { path: '/mypage', name: 'mypage', component: MyPage },
        { path: '/popup', name: 'popup', component: Popup },
        { path: '/meet', name: 'meet', component: Meet },
    ],
    scrollBehavior(to, from, savedPosition) {
        return savedPosition ? savedPosition : { x: 0, y: 0 }
    }
})
