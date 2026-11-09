import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider } from '@mui/material/styles'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes } from 'react-router'
import { ApiError } from './api/client'
import { RequireAuth } from './auth/RequireAuth'
import { AppLayout } from './components/AppLayout'
import { AsOfProvider } from './context/AsOfProvider'
import { LoginPage } from './features/auth/LoginPage'
import { theme } from './theme'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      // Los errores 4xx (validación, 404, sesión) no mejoran reintentando.
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
                {/* Versión parcial: facturas, reportes e importación llegan en las etapas 13 y 14.
                    Mientras tanto, cualquier ruta protegida muestra solo la barra superior. */}
                <Route path="*" element={null} />
              </Route>
            </Routes>
          </BrowserRouter>
        </AsOfProvider>
      </QueryClientProvider>
    </ThemeProvider>
  )
}
