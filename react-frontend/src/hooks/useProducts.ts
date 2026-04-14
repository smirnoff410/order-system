import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { productApi } from '../services/productApi'
import { CreateProductRequest, Product } from '../types/order'

export const useProducts = () => {
	const queryClient = useQueryClient()

	// Получение всех заказов
	const {
		data: products,
		isLoading,
		error,
	} = useQuery({
		queryKey: ['products'],
		queryFn: productApi.getProducts,
	})

	// Создание заказа
	const createProductMutation = useMutation({
		mutationFn: (request: CreateProductRequest) =>
			productApi.createProduct(request),
		onSuccess: () => {
			// Инвалидируем кеш заказов после создания
			queryClient.invalidateQueries({ queryKey: ['products'] })
		},
	})

	return {
		products,
		isLoading,
		error,
		createProduct: createProductMutation.mutate,
		isCreating: createProductMutation.isPending,
	}
}
