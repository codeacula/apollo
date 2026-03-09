import { createRouter, createWebHistory } from 'vue-router';
import Settings from '../views/Settings.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/settings',
    },
    {
      path: '/settings',
      name: 'Settings',
      component: Settings,
    },
  ],
});

export default router;
