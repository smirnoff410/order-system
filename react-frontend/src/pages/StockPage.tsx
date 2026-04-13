import React, { useState } from 'react'
import { useStock } from '../hooks/useStock'
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
	Dialog,
	DialogTitle,
	DialogContent,
	DialogActions,
	IconButton,
	Tooltip,
} from '@mui/material'
import { Refresh as RefreshIcon, Edit as EditIcon } from '@mui/icons-material'

export const StockPage: React.FC = () => {
	const { stockItems, isLoading, error, refetch, updateStock, isUpdating } =
		useStock()

	const [page, setPage] = useState(0)
	const [rowsPerPage, setRowsPerPage] = useState(10)

	// Состояния для редактирования
	const [editDialogOpen, setEditDialogOpen] = useState(false)
	const [selectedProduct, setSelectedProduct] = useState<{
		id: string
		quantity: number
		name?: string
	} | null>(null)
	const [newQuantity, setNewQuantity] = useState<number>(0)

	// Форматирование даты
	const formatDate = (dateString?: string) => {
		if (!dateString) return '—'
		const date = new Date(dateString)
		return date.toLocaleString('ru-RU')
	}

	// Определение статуса количества
	const getStockStatus = (quantity: number) => {
		if (quantity <= 0)
			return { label: 'Нет в наличии', color: 'error' as const }
		if (quantity < 10) return { label: 'Мало', color: 'warning' as const }
		if (quantity < 50) return { label: 'Норма', color: 'info' as const }
		return { label: 'Много', color: 'success' as const }
	}

	// Открыть диалог редактирования
	const handleEditClick = (item: any) => {
		setSelectedProduct({
			id: item.productId,
			quantity: item.quantityAvailable,
			name: item.productName,
		})
		setNewQuantity(item.quantityAvailable)
		setEditDialogOpen(true)
	}

	// Сохранить изменения
	const handleSaveQuantity = () => {
		if (selectedProduct && newQuantity >= 0) {
			updateStock({
				productId: selectedProduct.id,
				quantity: newQuantity,
			})
			setEditDialogOpen(false)
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
				Ошибка загрузки склада: {error.message}
			</Alert>
		)
	}

	// Пагинация
	const paginatedItems = stockItems?.slice(
		page * rowsPerPage,
		page * rowsPerPage + rowsPerPage,
	)

	return (
		<Box>
			{/* Заголовок */}
			<Box>
				<Typography variant='h4' component='h1'>
					Склад
				</Typography>
				<Tooltip title='Обновить'>
					<IconButton onClick={() => refetch()} color='primary'>
						<RefreshIcon />
					</IconButton>
				</Tooltip>
			</Box>

			{/* Статистика */}
			<Box>
				<Card sx={{ flex: 1 }}>
					<CardContent>
						<Typography variant='h4' color='primary'>
							{stockItems?.length || 0}
						</Typography>
						<Typography variant='body2' color='textSecondary'>
							Всего товаров
						</Typography>
					</CardContent>
				</Card>
				<Card sx={{ flex: 1 }}>
					<CardContent>
						<Typography variant='h4' color='error'>
							{stockItems?.filter(i => i.quantityAvailable <= 0).length || 0}
						</Typography>
						<Typography variant='body2' color='textSecondary'>
							Закончились
						</Typography>
					</CardContent>
				</Card>
				<Card sx={{ flex: 1 }}>
					<CardContent>
						<Typography variant='h4' color='warning'>
							{stockItems?.filter(
								i => i.quantityAvailable > 0 && i.quantityAvailable < 10,
							).length || 0}
						</Typography>
						<Typography variant='body2' color='textSecondary'>
							Осталось мало
						</Typography>
					</CardContent>
				</Card>
			</Box>

			{/* Таблица товаров */}
			<Card>
				<CardContent>
					<TableContainer>
						<Table>
							<TableHead>
								<TableRow>
									<TableCell>ID товара</TableCell>
									<TableCell align='center'>Количество</TableCell>
									<TableCell align='center'>Статус</TableCell>
									<TableCell align='center'>Действия</TableCell>
								</TableRow>
							</TableHead>
							<TableBody>
								{paginatedItems?.map(item => {
									const status = getStockStatus(item.quantityAvailable)
									return (
										<TableRow key={item.id}>
											<TableCell>
												<Typography>{item.productId}</Typography>
											</TableCell>
											<TableCell align='center'>
												<Typography>{item.quantityAvailable}</Typography>
											</TableCell>
											<TableCell align='center'>
												<Chip
													label={status.label}
													color={status.color}
													size='small'
												/>
											</TableCell>
											<TableCell align='center'>
												<Tooltip title='Редактировать количество'>
													<IconButton
														size='small'
														onClick={() => handleEditClick(item)}
														color='primary'
													>
														<EditIcon />
													</IconButton>
												</Tooltip>
											</TableCell>
										</TableRow>
									)
								})}
							</TableBody>
						</Table>
					</TableContainer>

					<TablePagination
						rowsPerPageOptions={[5, 10, 25, 50]}
						component='div'
						count={stockItems?.length || 0}
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
