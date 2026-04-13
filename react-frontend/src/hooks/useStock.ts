import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { stockApi } from '../services/stockApi'
import { StockItem } from '../types/order'

export const useStock = () => {
	const queryClient = useQueryClient()

	// Получение всех товаров
	const {
		data: stockItems,
		isLoading,
		error,
		refetch,
	} = useQuery({
		queryKey: ['stock'],
		queryFn: stockApi.getStockItems,
	})

	// Обновление количества товара
	const updateStockMutation = useMutation({
		mutationFn: ({
			productId,
			quantity,
		}: {
			productId: string
			quantity: number
		}) => stockApi.updateStockQuantity(productId, quantity),
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ['stock'] })
		},
	})

	return {
		stockItems,
		isLoading,
		error,
		refetch,
		updateStock: updateStockMutation.mutate,
		isUpdating: updateStockMutation.isPending,
	}
}
