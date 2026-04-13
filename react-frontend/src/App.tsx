import React from 'react'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import AppBar from '@mui/material/AppBar'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Container from '@mui/material/Container'

import { OrdersPage } from './pages/OrdersPage'
import { CreateOrderPage } from './pages/CreateOrderPage'
import { StockPage } from './pages/StockPage'

const queryClient = new QueryClient()
const theme = createTheme()

function App() {
	return (
		<QueryClientProvider client={queryClient}>
			<ThemeProvider theme={theme}>
				<CssBaseline />
				<Router>
					<AppBar position='static'>
						<Toolbar>
							<Typography variant='h6' sx={{ flexGrow: 1 }}>
								Order System
							</Typography>
							<Button color='inherit' component={Link} to='/orders'>
								Orders
							</Button>
							<Button color='inherit' component={Link} to='/create-order'>
								Create Order
							</Button>
							<Button color='inherit' component={Link} to='/stock'>
								Stock
							</Button>
						</Toolbar>
					</AppBar>
					<Container sx={{ mt: 4 }}>
						<Routes>
							<Route path='/orders' element={<OrdersPage />} />
							<Route path='/create-order' element={<CreateOrderPage />} />
							<Route path='/stock' element={<StockPage />} />
						</Routes>
					</Container>
				</Router>
			</ThemeProvider>
		</QueryClientProvider>
	)
}

export default App
