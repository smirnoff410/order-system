import axios from 'axios'
import { StockItem, AddStockItemRequest } from '../types/order'

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000'

const stockApiClient = axios.create({
	baseURL: API_BASE_URL,
	headers: {
		'Content-Type': 'application/json',
	},
	timeout: 10000,
})

export const stockApi = {
	// Получить все товары на складе
	getStockItems: async (): Promise<StockItem[]> => {
		const response = await stockApiClient.get<StockItem[]>('/stock/list')
		return response.data
	},

	// Обновить количество товара (для админа)
	updateStockQuantity: async (
		request: AddStockItemRequest,
	): Promise<StockItem> => {
		const response = await stockApiClient.post(`/stock/addItem`, request)
		return response.data
	},
}
