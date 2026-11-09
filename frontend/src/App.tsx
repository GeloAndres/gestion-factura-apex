import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider } from '@mui/material/styles'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { ApiError } from './api/client'
import { RequireAuth } from './auth/RequireAuth'
import { AppLayout } from './components/AppLayout'
import { AsOfProvider } from './context/AsOfProvider'
import { LoginPage } from './features/auth/LoginPage'
import { InvoiceDetailPage } from './features/invoices/InvoiceDetailPage'
import { InvoicesPage } from './features/invoices/InvoicesPage'
import { theme } from './theme'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      retry: (failureCount, error) =>
        !(error instanceof ApiError && error.status >= 400 && error.status < 500) && failureCount < 2,
    },
  },
})

export function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <QueryClientProvider client={queryClient}>
        <AsOfProvider>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route
                element={
                  <RequireAuth>
                    <AppLayout />
                  </RequireAuth>
                }
              >
                <Route index element={<Navigate to="/facturas" replace />} />
                <Route path="/facturas" element={<InvoicesPage />} />
                <Route path="/facturas/:invoiceNumber" element={<InvoiceDetailPage />} />
                <Route path="*" element={null} />
              </Route>
            </Routes>
          </BrowserRouter>
        </AsOfProvider>
      </QueryClientProvider>
    </ThemeProvider>
  )
}
