import { createRouter, createWebHistory } from 'vue-router';
import type { RouteRecordRaw } from 'vue-router';
import SetupView from '../views/SetupView.vue';
import DashboardView from '../views/DashboardView.vue';
import { useInitializationStore } from '../stores/initializationStore';

type InitializationGuardStore = Pick<ReturnType<typeof useInitializationStore>, 'isInitialized' | 'checkInitializationStatus'>

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

export async function resolveInitializationNavigation(
  toPath: string,
  initStore: InitializationGuardStore,
): Promise<true | string> {
  if (initStore.isInitialized === null) {
    await initStore.checkInitializationStatus();
  }

  if (toPath === '/setup') {
    return initStore.isInitialized === true ? '/dashboard' : true;
  }

  if (initStore.isInitialized === false) {
    return '/setup';
  }

  return true;
}

// Navigation guard to redirect to setup if not initialized
router.beforeEach(async (to) => {
  return await resolveInitializationNavigation(to.path, useInitializationStore());
});

export default router;
