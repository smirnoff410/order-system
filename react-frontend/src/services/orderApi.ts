import axios from 'axios'
import { Order, CreateOrderRequest, ApiResponse } from '../types/order'

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000'

const apiClient = axios.create({
	baseURL: API_BASE_URL,
	headers: {
		'Content-Type': 'application/json',
	},
	timeout: 10000,
})

// Интерсептор для обработки ошибок
apiClient.interceptors.response.use(
	response => response,
	error => {
		console.error('API Error:', error.response?.data || error.message)
		return Promise.reject(error)
	},
)

export const orderApi = {
	// Создать заказ
	createOrder: async (request: CreateOrderRequest): Promise<Order> => {
		const response = await apiClient.post<Order>('/orders/create', request)
		return response.data
	},

	// Получить все заказы
	getOrders: async (): Promise<Order[]> => {
		const response = await apiClient.get<Order[]>('/orders/list')
		return response.data
	},
}
