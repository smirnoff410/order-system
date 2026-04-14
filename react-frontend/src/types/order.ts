// Типы событий из вашего бэкенда
export interface OrderItem {
	productId: string
	quantity: number
	unitPrice: number
}

export interface CreateOrderRequest {
	customerName: string
	items: OrderItem[]
}
export interface CreateProductRequest {
	id: string
	name: string
}
export interface AddStockItemRequest {
	productId: string
	productName: string
	quantity: number
}

export interface Order {
	orderId: string
	customerName: string
	totalAmount: number
	status: 'Pending' | 'StockReserved' | 'Paid' | 'Failed' | 'Completed'
	failureReason?: string
	createdAt: string
	updatedAt?: string
	items: OrderItem[]
}

// Типы для WebSocket событий
export interface OrderStatusEvent {
	orderId: string
	status: Order['status']
	message?: string
	timestamp: string
}

export interface StockItem {
	id: string
	productId: string
	productName?: string
	quantityAvailable: number
	reservedQuantity?: number
	lastUpdated?: string
}

export interface Reservation {
	id: string
	orderId: string
	productId: string
	quantity: number
	reservedAt: string
	isActive: boolean
}

export interface Product {
	id: string
	name: string
}

// Типы ответов API
export interface ApiResponse<T> {
	success: boolean
	data?: T
	error?: string
}
