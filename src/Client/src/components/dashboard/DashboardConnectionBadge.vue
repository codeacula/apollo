<script setup lang="ts">
interface Props {
  mode: 'signalr' | 'polling' | 'reconnecting'
  label: string
}

defineProps<Props>()
</script>

<template>
  <span class="connection-badge" :data-mode="mode">{{ label }}</span>
</template>

<style scoped>
.connection-badge {
  padding: 0.5rem 0.85rem;
  border-radius: 999px;
  font-size: 0.82rem;
  font-weight: 700;
  letter-spacing: 0.02em;
  border: 1px solid rgba(111, 79, 59, 0.18);
  background: rgba(255, 253, 249, 0.92);
}

.connection-badge[data-mode='signalr'] {
  color: #1f6d43;
  background: rgba(218, 248, 230, 0.92);
}

.connection-badge[data-mode='polling'] {
  color: #8d5d0b;
  background: rgba(255, 244, 200, 0.95);
}

.connection-badge[data-mode='reconnecting'] {
  color: #8a4b10;
  background: rgba(255, 226, 196, 0.95);
}
</style>
