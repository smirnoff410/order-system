import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { orderApi } from '../services/orderApi'
import { CreateOrderRequest, Order } from '../types/order'

export const useOrders = () => {
	const queryClient = useQueryClient()

	// Получение всех заказов
	const {
		data: orders,
		isLoading,
		error,
	} = useQuery({
		queryKey: ['orders'],
		queryFn: orderApi.getOrders,
	})

	// Создание заказа
	const createOrderMutation = useMutation({
		mutationFn: (request: CreateOrderRequest) => orderApi.createOrder(request),
		onSuccess: () => {
			// Инвалидируем кеш заказов после создания
			queryClient.invalidateQueries({ queryKey: ['orders'] })
		},
	})

	return {
		orders,
		isLoading,
		error,
		createOrder: createOrderMutation.mutate,
		isCreating: createOrderMutation.isPending,
	}
}
