<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useTravelHistoryStore } from '@/stores/useTravelHistoryStore'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const router = useRouter()
const travelHistoryStore = useTravelHistoryStore()

const isLoading = ref(true)
const currentPage = ref(1)
const tripsPerPage = 6

const allTrips = computed(() => travelHistoryStore.getAllTrips)
const totalPages = computed(() => Math.ceil(allTrips.value.length / tripsPerPage))
const paginatedTrips = computed(() => {
  const start = (currentPage.value - 1) * tripsPerPage
  return allTrips.value.slice(start, start + tripsPerPage)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value

  if (total <= 5) {
    for (let i = 1; i <= total; i++) pages.push(i)
  } else {
    pages.push(1)
    if (current > 3) pages.push('...')
    const start = Math.max(2, current - 1)
    const end = Math.min(total - 1, current + 1)
    for (let i = start; i <= end; i++) pages.push(i)
    if (current < total - 2) pages.push('...')
    pages.push(total)
  }
  return pages
})

onMounted(async () => {
  await travelHistoryStore.fetchTrips()
  isLoading.value = false
})

const goBack = () => router.back()

const goToPage = (page) => {
  if (typeof page === 'number' && page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

const formatDate = (timestamp) => {
  const date = new Date(timestamp)
  const today = new Date()
  const yesterday = new Date(today)
  yesterday.setDate(yesterday.getDate() - 1)

  if (date.toDateString() === today.toDateString()) {
    return `Hoy ${date.toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' })}`
  } else if (date.toDateString() === yesterday.toDateString()) {
    return `Ayer ${date.toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' })}`
  } else {
    return date.toLocaleDateString('es-PE', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}

const viewRouteAgain = (trip) => {
  sessionStorage.setItem('repeatTrip', JSON.stringify({
    origin: trip.origin,
    destination: trip.destination,
    autoSearch: true
  }))

  router.push({
    path: '/search-route',
    query: { origin: trip.origin, destination: trip.destination, autoSearch: true },
  })
}

const removeTrip = async (tripId) => {
  if (confirm('¿Eliminar este viaje del historial?')) {
    await travelHistoryStore.removeTrip(tripId)
    if (paginatedTrips.value.length === 0 && currentPage.value > 1) {
      currentPage.value--
    }
  }
}

const clearHistory = async () => {
  if (confirm('¿Estás seguro de eliminar todo el historial de viajes?')) {
    await travelHistoryStore.clearHistory()
    currentPage.value = 1
  }
}

const getStepIcon = (step) => {
  if (step.type === 'walk' || step.mode === 'walking') return 'walk'
  if (step.type === 'bus' || step.mode === 'transit') return 'bus'
  if (step.type === 'stop') return 'stop'
  return 'stop'
}
</script>

<template>
  <div class="th-container">
    <div class="th-inner">
      <!-- Header -->
      <div class="th-header">
        <button class="th-back" @click="goBack">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M19 12H5"/><polyline points="12 19 5 12 12 5"/>
          </svg>
          {{ $t('travelHistory.back') }}
        </button>

        <div class="th-title-row">
          <div class="th-title-group">
            <h1>{{ $t('travelHistory.title') }}</h1>
            <span v-if="allTrips.length > 0" class="th-count">{{ allTrips.length }} viajes</span>
          </div>
          <button
              v-if="allTrips.length > 0"
              @click="clearHistory"
              class="th-clear"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
            </svg>
            {{ $t('travelHistory.clearHistory') }}
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="isLoading" class="th-loading">
        <div class="th-spinner"></div>
        <p>Cargando historial...</p>
      </div>

      <!-- Empty -->
      <div v-else-if="allTrips.length === 0" class="th-empty">
        <div class="th-empty-icon">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="#bbb" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/>
          </svg>
        </div>
        <h2>{{ $t('travelHistory.emptyTitle') }}</h2>
        <p>{{ $t('travelHistory.emptySubtitle') }}</p>
        <button @click="router.push('/search-route')" class="th-search-btn">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
          </svg>
          {{ $t('travelHistory.searchRoute') }}
        </button>
      </div>

      <!-- Trips Grid -->
      <div v-else>
        <div class="th-grid">
          <div
              v-for="trip in paginatedTrips"
              :key="trip.id"
              class="th-card"
          >
            <!-- Card Header -->
            <div class="th-card-header">
              <div class="th-card-date">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/>
                </svg>
                {{ formatDate(trip.timestamp) }}
              </div>
              <div class="th-card-actions">
                <button
                    @click="viewRouteAgain(trip)"
                    class="th-btn-icon"
                    :title="$t('travelHistory.repeatTooltip')"
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="1 4 1 10 7 10"/><polyline points="23 20 23 14 17 14"/>
                    <path d="M20.49 9A9 9 0 0 0 5.64 5.64L1 10m22 4l-4.64 4.36A9 9 0 0 1 3.51 15"/>
                  </svg>
                </button>
                <button
                    @click="removeTrip(trip.id)"
                    class="th-btn-icon th-btn-delete"
                    title="Eliminar"
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Route visualization -->
            <div class="th-route">
              <div class="th-route-line">
                <div class="th-dot th-dot-origin"></div>
                <div class="th-connector"></div>

                <!-- Steps -->
                <div v-if="trip.steps && trip.steps.length > 0" class="th-steps">
                  <div
                      v-for="(step, index) in trip.steps"
                      :key="index"
                      class="th-step"
                      :class="'th-step-' + getStepIcon(step)"
                  >
                    <div class="th-step-dot"></div>
                    <div class="th-step-label">
                      <svg v-if="getStepIcon(step) === 'walk'" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M13 4a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3z"/><path d="M7 21l3-9 2.5 2v7"/><path d="M10.5 14L14 10l4 1"/>
                      </svg>
                      <svg v-else-if="getStepIcon(step) === 'bus'" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                        <rect x="3" y="3" width="18" height="14" rx="2"/><path d="M3 10h18"/><path d="M7 21v-2"/><path d="M17 21v-2"/>
                      </svg>
                      <svg v-else width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/>
                      </svg>
                      <span>{{ step.name }}</span>
                      <span v-if="step.busNumber" class="th-bus-number">{{ step.busNumber }}</span>
                    </div>
                  </div>
                </div>

                <div class="th-dot th-dot-dest"></div>
              </div>

              <div class="th-endpoints">
                <div class="th-endpoint">
                  <span class="th-endpoint-tag origin">A</span>
                  <span class="th-endpoint-name">{{ trip.origin }}</span>
                </div>
                <div class="th-endpoint">
                  <span class="th-endpoint-tag dest">B</span>
                  <span class="th-endpoint-name">{{ trip.destination }}</span>
                </div>
              </div>
            </div>

            <!-- Footer -->
            <div class="th-card-footer" v-if="trip.duration || trip.distance || trip.notes">
              <span v-if="trip.duration" class="th-meta">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/>
                </svg>
                {{ trip.duration }}
              </span>
              <span v-if="trip.distance" class="th-meta">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M18 6l-6 6-6-6"/><path d="M18 18l-6-6-6 6"/>
                </svg>
                {{ trip.distance }}
              </span>
              <span v-if="trip.notes" class="th-meta th-meta-note">
                {{ trip.notes }}
              </span>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="th-pagination">
          <button
              class="th-page-btn"
              :disabled="currentPage === 1"
              @click="goToPage(currentPage - 1)"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="15 18 9 12 15 6"/>
            </svg>
          </button>

          <template v-for="(page, idx) in visiblePages" :key="idx">
            <span v-if="page === '...'" class="th-page-dots">...</span>
            <button
                v-else
                class="th-page-btn"
                :class="{ active: page === currentPage }"
                @click="goToPage(page)"
            >
              {{ page }}
            </button>
          </template>

          <button
              class="th-page-btn"
              :disabled="currentPage === totalPages"
              @click="goToPage(currentPage + 1)"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="9 18 15 12 9 6"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>


<style scoped>
.th-container {
  min-height: 100vh;
  background: #f4f6f8;
  padding: 1.5rem;
}

.th-inner {
  max-width: 1100px;
  margin: 0 auto;
}

/* Header */
.th-back {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #1a1a1a;
  color: #fff;
  border: none;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
  margin-bottom: 1.25rem;
}

.th-back:hover {
  background: #333;
}

.th-title-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.th-title-group {
  display: flex;
  align-items: baseline;
  gap: 12px;
}

.th-title-group h1 {
  font-size: 1.75rem;
  font-weight: 800;
  color: #1a1a1a;
  margin: 0;
  letter-spacing: -0.5px;
}

.th-count {
  font-size: 0.85rem;
  color: #888;
  font-weight: 500;
  background: #e9ecef;
  padding: 3px 10px;
  border-radius: 20px;
}

.th-clear {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #fff;
  color: #e53935;
  border: 1.5px solid #fcc;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.th-clear:hover {
  background: #fef2f2;
  border-color: #e53935;
}

/* Loading */
.th-loading {
  text-align: center;
  padding: 4rem 2rem;
}

.th-spinner {
  width: 36px;
  height: 36px;
  border: 3px solid #e0e0e0;
  border-top-color: #4caf50;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.th-loading p {
  color: #888;
  font-size: 0.95rem;
}

/* Empty */
.th-empty {
  max-width: 420px;
  margin: 3rem auto;
  text-align: center;
  padding: 3rem 2rem;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 1px 8px rgba(0,0,0,0.06);
}

.th-empty-icon {
  margin-bottom: 1.25rem;
  opacity: 0.5;
}

.th-empty h2 {
  color: #333;
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0 0 0.5rem;
}

.th-empty p {
  color: #888;
  font-size: 0.95rem;
  margin: 0 0 1.5rem;
}

.th-search-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: #4caf50;
  color: #fff;
  border: none;
  padding: 12px 24px;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.th-search-btn:hover {
  background: #43a047;
  transform: translateY(-1px);
  box-shadow: 0 4px 14px rgba(76,175,80,0.3);
}

/* Grid */
.th-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 1rem;
}

/* Card */
.th-card {
  background: #fff;
  border-radius: 14px;
  padding: 1.25rem;
  box-shadow: 0 1px 4px rgba(0,0,0,0.06);
  border: 1px solid #eee;
  transition: all 0.2s ease;
}

.th-card:hover {
  box-shadow: 0 4px 16px rgba(0,0,0,0.1);
  border-color: #ddd;
}

.th-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid #f0f0f0;
}

.th-card-date {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #4caf50;
  font-weight: 600;
  font-size: 0.85rem;
}

.th-card-actions {
  display: flex;
  gap: 4px;
}

.th-btn-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  background: #f5f5f5;
  color: #666;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.th-btn-icon:hover {
  background: #e8f5e9;
  color: #4caf50;
}

.th-btn-delete:hover {
  background: #fef2f2;
  color: #e53935;
}

/* Route visualization */
.th-route {
  margin-bottom: 0.75rem;
}

.th-route-line {
  display: flex;
  align-items: center;
  gap: 0;
  padding: 0.5rem 0;
  position: relative;
}

.th-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
  z-index: 1;
}

.th-dot-origin {
  background: #4caf50;
  box-shadow: 0 0 0 3px rgba(76,175,80,0.2);
}

.th-dot-dest {
  background: #e53935;
  box-shadow: 0 0 0 3px rgba(229,57,53,0.2);
}

.th-connector {
  flex: 1;
  height: 2px;
  background: linear-gradient(90deg, #4caf50, #e53935);
  opacity: 0.3;
}

.th-steps {
  flex: 1;
  display: flex;
  align-items: center;
  position: relative;
}

.th-steps::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 2px;
  background: linear-gradient(90deg, #4caf50 0%, #aaa 50%, #e53935 100%);
  opacity: 0.25;
}

.th-step {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  z-index: 1;
}

.th-step-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #ccc;
  margin-bottom: 4px;
}

.th-step-walk .th-step-dot {
  background: #ff9800;
}

.th-step-bus .th-step-dot {
  background: #2196f3;
}

.th-step-stop .th-step-dot {
  background: #9c27b0;
}

.th-step-label {
  display: flex;
  align-items: center;
  gap: 3px;
  font-size: 0.7rem;
  color: #888;
  white-space: nowrap;
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.th-step-label span {
  overflow: hidden;
  text-overflow: ellipsis;
}

.th-bus-number {
  background: #e3f2fd;
  color: #1976d2;
  padding: 0 4px;
  border-radius: 3px;
  font-weight: 700;
  font-size: 0.65rem;
  flex-shrink: 0;
}

.th-endpoints {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  margin-top: 0.5rem;
}

.th-endpoint {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  min-width: 0;
}

.th-endpoint:last-child {
  justify-content: flex-end;
  text-align: right;
}

.th-endpoint-tag {
  width: 22px;
  height: 22px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 0.7rem;
  flex-shrink: 0;
  color: #fff;
}

.th-endpoint-tag.origin {
  background: #4caf50;
}

.th-endpoint-tag.dest {
  background: #e53935;
}

.th-endpoint-name {
  font-size: 0.85rem;
  font-weight: 600;
  color: #333;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Footer */
.th-card-footer {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  padding-top: 0.75rem;
  border-top: 1px solid #f0f0f0;
}

.th-meta {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.8rem;
  color: #888;
  background: #f8f8f8;
  padding: 3px 8px;
  border-radius: 6px;
}

.th-meta-note {
  color: #666;
  font-style: italic;
}

/* Pagination */
.th-pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 6px;
  margin-top: 2rem;
  padding-bottom: 2rem;
}

.th-page-btn {
  min-width: 36px;
  height: 36px;
  border: 1px solid #ddd;
  background: #fff;
  color: #555;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.th-page-btn:hover:not(:disabled):not(.active) {
  background: #f5f5f5;
  border-color: #ccc;
}

.th-page-btn.active {
  background: #4caf50;
  color: #fff;
  border-color: #4caf50;
}

.th-page-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.th-page-dots {
  color: #aaa;
  font-size: 0.85rem;
  padding: 0 4px;
}

/* Responsive */
@media (max-width: 768px) {
  .th-container {
    padding: 1rem;
  }

  .th-title-group h1 {
    font-size: 1.4rem;
  }

  .th-grid {
    grid-template-columns: 1fr;
  }

  .th-step-label {
    display: none;
  }
}
</style>