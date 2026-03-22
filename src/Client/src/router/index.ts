import { createRouter, createWebHistory } from 'vue-router';
import type { RouteRecordRaw } from 'vue-router';
import SetupView from '../views/SetupView.vue';
import DashboardView from '../views/DashboardView.vue';
import { useInitializationStore } from '../stores/initializationStore';

const routes: RouteRecordRaw[] = [
  {
    path: '/setup',
    name: 'Setup',
    component: SetupView,
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: DashboardView,
  },
  {
    path: '/',
    redirect: '/dashboard',
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Navigation guard to redirect to setup if not initialized
router.beforeEach((to, _from, next) => {
  // Skip guard for setup route
  if (to.path === '/setup') {
    next();
    return;
  }

  const initStore = useInitializationStore();

  // If not initialized, redirect to setup
  if (initStore.isInitialized === false) {
    next('/setup');
    return;
  }

  // Allow navigation
  next();
});

export default router;
