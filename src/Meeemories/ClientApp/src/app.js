import './styles/reset.css'
import './styles/style.css'
import Vue from 'vue'
import App from './App.vue'
import { router } from './router'
import { state, actions } from './state'

Vue.prototype.$state = state;
Vue.prototype.$actions = actions;

new Vue({
    router,
    render: h => h(App),
}).$mount('#app')