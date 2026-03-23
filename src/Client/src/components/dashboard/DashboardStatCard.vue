<script setup lang="ts">
interface Props {
  label: string
  value: number | string
  tone: 'warm' | 'sun' | 'mint' | 'sky'
}

defineProps<Props>()
</script>

<template>
  <article class="stat-card" :data-tone="tone">
    <p class="stat-label">{{ label }}</p>
    <p class="stat-value">{{ value }}</p>
  </article>
</template>

<style scoped>
.stat-card {
  padding: 1rem 1.2rem;
  border-radius: 1.1rem;
  border: 1px solid rgba(111, 79, 59, 0.18);
  box-shadow: 0 22px 55px rgba(82, 56, 39, 0.12);
  backdrop-filter: blur(8px);
}

.stat-card[data-tone='warm'] {
  background: linear-gradient(180deg, rgba(255, 222, 194, 0.95), rgba(255, 253, 249, 0.9));
}

.stat-card[data-tone='sun'] {
  background: linear-gradient(180deg, rgba(255, 242, 188, 0.95), rgba(255, 253, 249, 0.9));
}

.stat-card[data-tone='mint'] {
  background: linear-gradient(180deg, rgba(214, 247, 228, 0.95), rgba(255, 253, 249, 0.9));
}

.stat-card[data-tone='sky'] {
  background: linear-gradient(180deg, rgba(214, 234, 255, 0.95), rgba(255, 253, 249, 0.9));
}

.stat-label {
  margin: 0;
  font-size: 0.88rem;
  color: #75665d;
}

.stat-value {
  margin: 0.45rem 0 0;
  font-size: clamp(1.9rem, 3vw, 2.5rem);
  font-weight: 700;
  color: #2f221b;
}
</style>
