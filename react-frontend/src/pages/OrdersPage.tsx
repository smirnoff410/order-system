import React, { useState } from 'react'
import { useOrders } from '../hooks/useOrders'
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
} from '@mui/material'

export const OrdersPage: React.FC = () => {
	const { orders, isLoading, error } = useOrders()

	// Только пагинация
	const [page, setPage] = useState(0)
	const [rowsPerPage, setRowsPerPage] = useState(10)

	// Форматирование даты
	const formatDate = (dateString: string) => {
		const date = new Date(dateString)
		return date.toLocaleString('ru-RU')
	}

	// Форматирование цены
	const formatPrice = (price: number) => {
		return `${price.toFixed(2)} ₽`
	}

	// Цвет чипа в зависимости от статуса
	const getStatusColor = (
		status: string,
	): 'success' | 'error' | 'warning' | 'info' | 'default' => {
		switch (status) {
			case 'Completed':
				return 'success'
			case 'Failed':
				return 'error'
			case 'Paid':
				return 'success'
			case 'StockReserved':
				return 'warning'
			case 'Pending':
				return 'info'
			default:
				return 'default'
		}
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
				Ошибка загрузки заказов: {error.message}
			</Alert>
		)
	}

	// Пагинация
	const paginatedOrders = orders?.slice(
		page * rowsPerPage,
		page * rowsPerPage + rowsPerPage,
	)

	return (
		<Box>
			<Typography variant='h4' component='h1' gutterBottom>
				Заказы
			</Typography>

			<Card>
				<CardContent>
					<TableContainer>
						<Table>
							<TableHead>
								<TableRow>
									<TableCell>ID заказа</TableCell>
									<TableCell>ID клиента</TableCell>
									<TableCell align='right'>Сумма</TableCell>
									<TableCell>Статус</TableCell>
									<TableCell>Дата создания</TableCell>
								</TableRow>
							</TableHead>
							<TableBody>
								{paginatedOrders?.map(order => (
									<TableRow key={order.orderId}>
										<TableCell>{order.orderId.slice(0, 8)}...</TableCell>
										<TableCell>{order.customerId.slice(0, 8)}...</TableCell>
										<TableCell align='right'>
											<Typography>{formatPrice(order.totalAmount)}</Typography>
										</TableCell>
										<TableCell>
											<Chip
												label={order.status}
												color={getStatusColor(order.status)}
												size='small'
											/>
										</TableCell>
										<TableCell>{formatDate(order.createdAt)}</TableCell>
									</TableRow>
								))}
							</TableBody>
						</Table>
					</TableContainer>

					<TablePagination
						rowsPerPageOptions={[5, 10, 25]}
						component='div'
						count={orders?.length || 0}
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
