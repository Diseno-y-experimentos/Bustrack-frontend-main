<template>
  <div class="spinner-container" :class="sizeClass">
    <div class="spinner" :style="{ borderTopColor: color }"></div>
    <p v-if="text" class="spinner-text">{{ text }}</p>
  </div>
</template>

<script setup>
defineProps({
  text: {
    type: String,
    default: null
  },
  size: {
    type: String,
    default: 'medium',
    validator: (value) => ['small', 'medium', 'large'].includes(value)
  },
  color: {
    type: String,
    default: '#2d6a2e'
  }
})

const sizeClass = computed(() => `spinner-${props.size}`)
</script>

<script>
import { computed } from 'vue'
export default {
  name: 'Spinner'
}
</script>

<style scoped>
.spinner-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 20px;
}

.spinner {
  border: 4px solid #f0f0f0;
  border-top-color: #2d6a2e;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.spinner-small {
  width: 30px;
}

.spinner-small .spinner {
  width: 24px;
  height: 24px;
  border: 3px solid #f0f0f0;
}

.spinner-medium {
  width: 50px;
}

.spinner-medium .spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f0f0f0;
}

.spinner-large {
  width: 70px;
}

.spinner-large .spinner {
  width: 60px;
  height: 60px;
  border: 5px solid #f0f0f0;
}

.spinner-text {
  font-size: 14px;
  color: #666;
  margin: 0;
  font-weight: 500;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>