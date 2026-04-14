import axios from 'axios'
import { Product, CreateProductRequest, ApiResponse } from '../types/order'

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

export const productApi = {
	createProduct: async (request: CreateProductRequest): Promise<Product> => {
		const response = await apiClient.post<Product>('/product/create', request)
		return response.data
	},

	getProducts: async (): Promise<Product[]> => {
		const response = await apiClient.get<Product[]>('/product/list')
		return response.data
	},
}
