<script setup lang="ts">
import type { DashboardActivityItem } from '../../services/dashboardApi'

interface Props {
  items: DashboardActivityItem[]
}

defineProps<Props>()
</script>

<template>
  <article class="panel panel-activity">
    <div class="panel-head">
      <h2>Recent activity</h2>
      <p>The latest moves across reminders, tasks, and conversations.</p>
    </div>

    <ol v-if="items.length > 0" class="activity-list">
      <li v-for="item in items" :key="item.id" class="activity-item">
        <div class="activity-meta">
          <span class="activity-kind">{{ item.title }}</span>
          <time :datetime="item.occurredOnUtc">{{ new Date(item.occurredOnUtc).toLocaleString() }}</time>
        </div>
        <p>{{ item.description }}</p>
      </li>
    </ol>

    <p v-else class="empty-state">No activity yet. Apollo is either napping or freshly initialized.</p>
  </article>
</template>

<style scoped>
@import '../../assets/panel.css';

.panel-activity {
  padding: 1.4rem;
}

.activity-list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.activity-item {
  padding: 1rem;
  border-radius: 1rem;
  background: #fffdf9;
  border: 1px solid rgba(111, 79, 59, 0.1);
}

.activity-item p {
  margin: 0.45rem 0 0;
}

.activity-meta {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  font-size: 0.88rem;
  color: #75665d;
}

.activity-kind {
  font-weight: 700;
  color: #a1512a;
}

.empty-state {
  color: #75665d;
}

@media (max-width: 720px) {
  .activity-meta {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
