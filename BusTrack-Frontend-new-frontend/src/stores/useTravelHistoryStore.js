import { defineStore } from 'pinia'
import { BaseApi } from '@/shared/infrastructure/base-api.js'
import { useUserStore } from '@/stores/useUserStore'

const api = new BaseApi()

function normalizeTripResource(resource = {}) {
    return {
        id: resource.Id ?? resource.id ?? Date.now(),
        userId: resource.UserId ?? resource.userId ?? null,
        routeId: resource.RouteId ?? resource.routeId ?? null,
        origin: resource.Origin ?? resource.origin ?? '',
        destination: resource.Destination ?? resource.destination ?? '',
        timestamp: resource.StartedAt ?? resource.startedAt ?? resource.CreatedAt ?? resource.createdAt ?? resource.timestamp ?? new Date().toISOString(),
        startedAt: resource.StartedAt ?? resource.startedAt ?? null,
        endedAt: resource.EndedAt ?? resource.endedAt ?? null,
        notes: resource.Notes ?? resource.notes ?? '',
        steps: resource.steps ?? [],
        duration: resource.duration ?? null,
        distance: resource.distance ?? null,
        createdAt: resource.CreatedAt ?? resource.createdAt ?? null,
        updatedAt: resource.UpdatedAt ?? resource.updatedAt ?? null
    }
}

function getCurrentUserId() {
    const userStore = useUserStore()
    return userStore.user?.id ?? userStore.profile?.Id ?? userStore.profile?.id ?? null
}

export const useTravelHistoryStore = defineStore('travelHistory', {
    state: () => ({
        userTrips: [],
    }),
    getters: {
        getAllTrips: (state) => state.userTrips,
        recentTrips: (state) => state.userTrips.slice(0, 10),
        tripsByDate: (state) => (date) => {
            return state.userTrips.filter(trip => {
                return new Date(trip.timestamp).toDateString() === new Date(date).toDateString()
            })
        }
    },
    actions: {
        async fetchTrips(userId = getCurrentUserId()) {
            if (!userId) return this.userTrips

            try {
                const response = await api.http.get(`/users/${userId}/trips`)
                const resources = Array.isArray(response.data) ? response.data : []
                this.userTrips = resources.map(normalizeTripResource)
            } catch (error) {
                console.warn('⚠️ No se pudo sincronizar historial:', error?.response?.status, error?.response?.data || error.message)
            }
            return this.userTrips
        },
        /**
         * Adds a new trip to the user's travel history.
         * Automatically assigns an ID (timestamp) and current timestamp, then persists to localStorage.
         *
         * @param {TripPayload} tripData - Trip object to save
         * @param {string} tripData.origin - Starting point/origin location of the trip
         * @param {string} tripData.destination - End point/destination location of the trip
         * @param {Array<TripStep>} [tripData.steps=[]] - Array of trip steps (walk, bus, stop)
         * @param {string} [tripData.duration=null] - Total trip duration (e.g., "45 min")
         * @param {string} [tripData.distance=null] - Total trip distance (e.g., "12.5 km")
         * @returns {void}
         */
        async addTrip(tripData) {
            const userId = tripData.userId ?? getCurrentUserId()
            const body = {
                RouteId: tripData.routeId ?? tripData.routeData?.id ?? tripData.routeData?.Id ?? null,
                Origin: tripData.origin,
                Destination: tripData.destination,
                StartedAt: tripData.startedAt ?? tripData.timestamp ?? new Date().toISOString(),
                EndedAt: tripData.endedAt ?? null,
                Notes: tripData.notes ?? ''
            }

            try {
                if (userId) {
                    const response = await api.http.post(`/users/${userId}/trips`, body)
                    const createdTrip = normalizeTripResource({
                        ...response.data,
                        steps: tripData.steps || [],
                        duration: tripData.duration || null,
                        distance: tripData.distance || null
                    })
                    this.userTrips.unshift(createdTrip)
                    return createdTrip
                }
            } catch (error) {
                console.warn('⚠️ Error guardando viaje:', error?.response?.status, error?.response?.data || error.message)
            }
        },

        /**
         * Removes a specific trip from the user's travel history.
         * Demo trips (IDs starting with 'demo-') cannot be deleted and will trigger an alert.
         *
         * @param {number|string} tripId - Unique identifier of the trip to remove
         * @returns {void}
         */
        async removeTrip(tripId) {
            const userId = getCurrentUserId()

            try {
                if (userId) {
                    await api.http.delete(`/users/${userId}/trips/${tripId}`)
                }
            } catch (error) {
                console.warn('⚠️ Error eliminando viaje:', error?.response?.data || error.message)
            } finally {
                this.userTrips = this.userTrips.filter(trip => trip.id !== tripId)
            }
        },

        /**
         * Clears all user trips from the history.
         * Demo trips are preserved and remain intact.
         * @returns {void}
         */
        async clearHistory() {
            const trips = [...this.userTrips]
            await Promise.all(trips.map(trip => this.removeTrip(trip.id)))
            this.userTrips = []
        },

        /**
         * Clears ALL trips including demo trips.
         * This is a destructive action that removes both user and demo data.
         * @returns {void}
         */
        clearAllIncludingDemo() {
            this.userTrips = []
            this.saveToLocalStorage()
        },

        /**
         * Persists the current user trips array to localStorage.
         * Called automatically after every modification to maintain data consistency.
         * Note: Demo trips are not persisted as they are hardcoded.
         * @returns {void}
         */
        saveToLocalStorage() {
            localStorage.setItem('travelHistory', JSON.stringify(this.userTrips))
        }
    },
})