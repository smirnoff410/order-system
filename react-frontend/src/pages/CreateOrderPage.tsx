import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useOrders } from '../hooks/useOrders'
import { useProducts } from '../hooks/useProducts'
import {
	Box,
	Button,
	Card,
	CardContent,
	TextField,
	Typography,
	IconButton,
	Alert,
	FormControl,
	InputLabel,
	Select,
	MenuItem,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'

interface OrderItem {
	productId: string
	quantity: number
	unitPrice: number
}

export const CreateOrderPage: React.FC = () => {
	const navigate = useNavigate()
	const { createOrder, isCreating } = useOrders()
	const { products } = useProducts()
	const [customerName, setCustomerName] = useState('')
	const [items, setItems] = useState<OrderItem[]>([
		{ productId: '', quantity: 1, unitPrice: 0 },
	])
	const [error, setError] = useState<string | null>(null)

	const addItem = () => {
		setItems([...items, { productId: '', quantity: 1, unitPrice: 0 }])
	}

	const removeItem = (index: number) => {
		setItems(items.filter((_, i) => i !== index))
	}

	const updateItem = (
		index: number,
		field: keyof OrderItem,
		value: string | number,
	) => {
		const newItems = [...items]
		newItems[index] = { ...newItems[index], [field]: value }
		setItems(newItems)
	}

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault()
		setError(null)

		if (!customerName) {
			setError('Customer Name is required')
			return
		}

		if (
			items.some(
				item => !item.productId || item.quantity <= 0 || item.unitPrice <= 0,
			)
		) {
			setError('All items must have valid product ID, quantity and price')
			return
		}

		console.log(products)

		createOrder(
			{
				customerName,
				items: items.map(item => ({
					productId: item.productId,
					quantity: item.quantity,
					unitPrice: item.unitPrice,
				})),
			},
			{
				onSuccess: order => {
					navigate(`/orders?highlight=${order.orderId}`)
				},
				onError: err => {
					setError(err.message)
				},
			},
		)
	}

	return (
		<Card>
			<CardContent>
				<Typography variant='h5' gutterBottom>
					Create New Order
				</Typography>

				{error && (
					<Alert severity='error' sx={{ mb: 2 }}>
						{error}
					</Alert>
				)}

				<form onSubmit={handleSubmit}>
					<TextField
						fullWidth
						label='Customer Name'
						value={customerName}
						onChange={e => setCustomerName(e.target.value)}
						margin='normal'
						required
					/>

					<Typography variant='h6' sx={{ mt: 3, mb: 2 }}>
						Order Items
					</Typography>

					{items.map((item, index) => (
						<Box key={index} sx={{ display: 'flex', gap: 2, mb: 2 }}>
							<FormControl required sx={{ flex: 2 }}>
								<InputLabel>Product</InputLabel>
								<Select
									value={item.productId}
									label='Product'
									onChange={e => updateItem(index, 'productId', e.target.value)}
								>
									<MenuItem value='' disabled>
										Select a product
									</MenuItem>
									{products?.map(product => (
										<MenuItem key={product.id} value={product.id}>
											{product.name}
										</MenuItem>
									))}
								</Select>
							</FormControl>
							<TextField
								label='Quantity'
								type='number'
								value={item.quantity}
								onChange={e =>
									updateItem(index, 'quantity', parseInt(e.target.value))
								}
								required
								sx={{ flex: 1 }}
							/>
							<TextField
								label='Unit Price'
								type='number'
								value={item.unitPrice}
								onChange={e =>
									updateItem(index, 'unitPrice', parseFloat(e.target.value))
								}
								required
								sx={{ flex: 1 }}
							/>
							<IconButton
								onClick={() => removeItem(index)}
								disabled={items.length === 1}
								color='error'
							>
								<DeleteIcon />
							</IconButton>
						</Box>
					))}

					<Button startIcon={<AddIcon />} onClick={addItem} sx={{ mt: 1 }}>
						Add Item
					</Button>

					<Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
						<Button
							type='submit'
							variant='contained'
							color='primary'
							disabled={isCreating}
						>
							{isCreating ? 'Creating...' : 'Create Order'}
						</Button>
						<Button variant='outlined' onClick={() => navigate('/orders')}>
							Cancel
						</Button>
					</Box>
				</form>
			</CardContent>
		</Card>
	)
}
