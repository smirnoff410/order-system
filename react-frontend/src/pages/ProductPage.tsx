import React, { useState } from 'react'
import { useProducts } from '../hooks/useProducts'
import { useStock } from '../hooks/useStock'
import AddIcon from '@mui/icons-material/Add'
import {
	Box,
	Card,
	CardContent,
	Typography,
	Table,
	TableBody,
	TableCell,
	TableContainer,
	TableHead,
	TableRow,
	TablePagination,
	Chip,
	CircularProgress,
	Alert,
	TextField,
	Button,
} from '@mui/material'

export const ProductsPage: React.FC = () => {
	const { products, isLoading, error } = useProducts()
	const { updateStock } = useStock()

	// Только пагинация
	const [page, setPage] = useState(0)
	const [rowsPerPage, setRowsPerPage] = useState(10)

	// Форматирование даты
	const formatDate = (dateString: string) => {
		const date = new Date(dateString)
		return date.toLocaleString('ru-RU')
	}

	const addItem = (productId: string, productName: string) => {
		console.log(productId)
		console.log(productName)
		updateStock({
			productId,
			productName,
			quantity: 1,
		})
	}

	if (isLoading) {
		return (
			<Box>
				<CircularProgress />
			</Box>
		)
	}

	if (error) {
		return (
			<Alert severity='error' sx={{ m: 2 }}>
				Ошибка загрузки продуктов: {error.message}
			</Alert>
		)
	}

	// Пагинация
	const paginatedProducts = products?.slice(
		page * rowsPerPage,
		page * rowsPerPage + rowsPerPage,
	)

	return (
		<Box>
			<Typography variant='h4' component='h1' gutterBottom>
				Продукты
			</Typography>

			<Card>
				<CardContent>
					<TableContainer>
						<Table>
							<TableHead>
								<TableRow>
									<TableCell>ID продукта</TableCell>
									<TableCell>Имя продукта</TableCell>
									<TableCell>Добавить на склад</TableCell>
								</TableRow>
							</TableHead>
							<TableBody>
								{paginatedProducts?.map(product => (
									<TableRow key={product.id}>
										<TableCell>{product.id}</TableCell>
										<TableCell>{product.name}</TableCell>
										<TableCell>
											<Button
												startIcon={<AddIcon />}
												onClick={() => addItem(product.id, product.name)}
												sx={{ mt: 1 }}
											>
												Add Item
											</Button>
										</TableCell>
									</TableRow>
								))}
							</TableBody>
						</Table>
					</TableContainer>

					<TablePagination
						rowsPerPageOptions={[5, 10, 25]}
						component='div'
						count={products?.length || 0}
						rowsPerPage={rowsPerPage}
						page={page}
						onPageChange={(_, newPage) => setPage(newPage)}
						onRowsPerPageChange={e => {
							setRowsPerPage(parseInt(e.target.value, 10))
							setPage(0)
						}}
					/>
				</CardContent>
			</Card>
		</Box>
	)
}
