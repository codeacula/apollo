<script setup lang="ts">
import type {
  DashboardConversationSummary,
  DashboardReminderSummary,
  DashboardToDoSummary,
} from '../../services/dashboardApi'

interface Props {
  toDos: DashboardToDoSummary
  reminders: DashboardReminderSummary
  conversations: DashboardConversationSummary
}

defineProps<Props>()
</script>

<template>
  <article class="panel panel-workload">
    <div class="panel-head">
      <h2>Workload snapshot</h2>
      <p>What Apollo is juggling right now.</p>
    </div>

    <dl class="workload-list">
      <div>
        <dt>To-dos completed</dt>
        <dd>{{ toDos.completed }}</dd>
      </div>
      <div>
        <dt>To-dos created today</dt>
        <dd>{{ toDos.createdToday }}</dd>
      </div>
      <div>
        <dt>Reminders scheduled</dt>
        <dd>{{ reminders.scheduled }}</dd>
      </div>
      <div>
        <dt>Reminders sent today</dt>
        <dd>{{ reminders.sentToday }}</dd>
      </div>
      <div>
        <dt>Reminders acknowledged</dt>
        <dd>{{ reminders.acknowledged }}</dd>
      </div>
      <div>
        <dt>Conversations tracked</dt>
        <dd>{{ conversations.total }}</dd>
      </div>
    </dl>
  </article>
</template>

<style scoped>
@import '../../assets/panel.css';

.panel-workload {
  padding: 1.4rem;
}

.workload-list {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.9rem;
  margin: 0;
}

.workload-list div {
  padding: 0.95rem 1rem;
  border-radius: 1rem;
  background: #fff7ef;
  border: 1px solid rgba(111, 79, 59, 0.1);
}

.workload-list dt {
  margin: 0;
  font-size: 0.88rem;
  color: #75665d;
}

.workload-list dd {
  margin: 0.35rem 0 0;
  font-size: 1.35rem;
  font-weight: 700;
}

@media (max-width: 960px) {
  .workload-list {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 720px) {
  .workload-list {
    grid-template-columns: 1fr;
  }
}
</style>
