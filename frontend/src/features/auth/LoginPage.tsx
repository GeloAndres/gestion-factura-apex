import { zodResolver } from '@hookform/resolvers/zod'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { Navigate, useLocation } from 'react-router'
import { z } from 'zod'
import { errorMessage } from '../../api/client'
import { invoicesApi } from '../../api/invoicesApi'
import { sessionStore } from '../../auth/session'
import { useSession } from '../../auth/useSession'
import { Brand } from '../../components/Brand'

const loginSchema = z.object({
  username: z.string().trim().min(1, 'Ingresa tu usuario.'),
  password: z.string().min(1, 'Ingresa tu contraseña.'),
})

type LoginForm = z.infer<typeof loginSchema>

export function LoginPage() {
  const session = useSession()
  const location = useLocation()
  const queryClient = useQueryClient()
  const from = (location.state as { from?: string } | null)?.from ?? '/facturas'

  const { control, handleSubmit } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { username: '', password: '' },
  })

  const login = useMutation({
    mutationFn: (values: LoginForm) => invoicesApi.login(values.username, values.password),
    onSuccess: (response) => {
      queryClient.clear()
      sessionStore.set({ token: response.accessToken, username: response.username, expiresAt: response.expiresAt })
    },
  })

  if (session) return <Navigate to={from} replace />

  return (
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', p: 2, bgcolor: 'background.default' }}>
      <Card sx={{ width: '100%', maxWidth: 420, boxShadow: '0 12px 32px rgba(16, 24, 40, 0.08)' }}>
        <CardContent sx={{ p: { xs: 3, sm: 5 } }}>
          <Stack component="form" noValidate onSubmit={handleSubmit((values) => login.mutate(values))} sx={{ gap: 2.5 }}>
            <Brand />
            <div>
              <Typography variant="h1">Bienvenido</Typography>
              <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                Inicia sesión para gestionar tus facturas.
              </Typography>
            </div>

            {login.isError && <Alert severity="error">{errorMessage(login.error)}</Alert>}

            <Controller
              name="username"
              control={control}
              render={({ field: { ref, ...field }, fieldState }) => (
                <TextField
                  {...field}
                  inputRef={ref}
                  label="Usuario"
                  autoComplete="username"
                  autoFocus
                  error={Boolean(fieldState.error)}
                  helperText={fieldState.error?.message}
                />
              )}
            />
            <Controller
              name="password"
              control={control}
              render={({ field: { ref, ...field }, fieldState }) => (
                <TextField
                  {...field}
                  inputRef={ref}
                  type="password"
                  label="Contraseña"
                  autoComplete="current-password"
                  error={Boolean(fieldState.error)}
                  helperText={fieldState.error?.message}
                />
              )}
            />

            <Button type="submit" variant="contained" size="large" loading={login.isPending}>
              Ingresar
            </Button>

            <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center' }}>
              Usuario de prueba: <strong>admin</strong> · contraseña <strong>Admin123!</strong>
            </Typography>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  )
}
