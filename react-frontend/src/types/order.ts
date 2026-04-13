// Типы событий из вашего бэкенда
export interface OrderItem {
	productId: string
	quantity: number
	unitPrice: number
}

export interface CreateOrderRequest {
	customerId: string
	items: OrderItem[]
}

export interface Order {
	orderId: string
	customerId: string
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

// Типы ответов API
export interface ApiResponse<T> {
	success: boolean
	data?: T
	error?: string
}
