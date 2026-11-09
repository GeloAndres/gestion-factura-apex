import AssessmentRounded from '@mui/icons-material/AssessmentRounded'
import InfoOutlined from '@mui/icons-material/InfoOutlined'
import LogoutRounded from '@mui/icons-material/LogoutRounded'
import ReceiptLongRounded from '@mui/icons-material/ReceiptLongRounded'
import UploadFileRounded from '@mui/icons-material/UploadFileRounded'
import AppBar from '@mui/material/AppBar'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Container from '@mui/material/Container'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Toolbar from '@mui/material/Toolbar'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useQueryClient } from '@tanstack/react-query'
import { NavLink, Outlet } from 'react-router'
import { sessionStore } from '../auth/session'
import { useSession } from '../auth/useSession'
import { useAsOf } from '../context/asOfContext'

const navItems = [
  { to: '/facturas', label: 'Facturas', icon: <ReceiptLongRounded /> },
  { to: '/reportes', label: 'Reportes', icon: <AssessmentRounded /> },
  { to: '/importar', label: 'Importar', icon: <UploadFileRounded /> },
]

export function AppLayout() {
  const session = useSession()
  const queryClient = useQueryClient()

  const logout = () => {
    sessionStore.set(null)
    queryClient.clear()
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar
        position="sticky"
        elevation={0}
        color="inherit"
        sx={{ borderBottom: 1, borderColor: 'divider', bgcolor: 'background.paper' }}
      >
        <Toolbar sx={{ gap: { xs: 1, md: 2 }, flexWrap: 'wrap', py: 1 }}>
          <Typography variant="h6" component="span" sx={{ fontWeight: 700, mr: 1 }}>
            Gestión de Facturas
          </Typography>
          <Stack component="nav" aria-label="Secciones" direction="row" sx={{ gap: 0.5 }}>
            {navItems.map((item) => (
              <Button
                key={item.to}
                component={NavLink}
                to={item.to}
                startIcon={item.icon}
                color="inherit"
                sx={{ color: 'text.secondary', '&.active': { color: 'primary.main', bgcolor: 'action.selected' } }}
              >
                {item.label}
              </Button>
            ))}
          </Stack>
          <Box sx={{ flexGrow: 1 }} />
          <AsOfControl />
          <Stack direction="row" sx={{ alignItems: 'center', gap: 0.5 }}>
            <Typography variant="body2" color="text.secondary">
              {session?.username}
            </Typography>
            <Tooltip title="Cerrar sesión">
              <IconButton aria-label="Cerrar sesión" onClick={logout}>
                <LogoutRounded />
              </IconButton>
            </Tooltip>
          </Stack>
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg" component="main" sx={{ py: { xs: 2, md: 4 } }}>
        <Outlet />
      </Container>
    </Box>
  )
}

const AS_OF_HELP =
  'Los estados de pago (Pendiente / Vencida) se calculan a esta fecha. Vacía = hoy. ' +
  'Los datos de ejemplo vencen en 2025: prueba con 01-03-2025 para ver los tres estados.'

function AsOfControl() {
  const { asOf, setAsOf } = useAsOf()

  return (
    <Stack direction="row" sx={{ alignItems: 'center', gap: 0.5 }}>
      <TextField
        size="small"
        type="date"
        label="Fecha de referencia"
        value={asOf ?? ''}
        onChange={(event) => setAsOf(event.target.value || null)}
        slotProps={{ inputLabel: { shrink: true }, htmlInput: { min: '2000-01-01', max: '2100-12-31' } }}
        sx={{ width: 180 }}
      />
      <Tooltip title={AS_OF_HELP}>
        <IconButton size="small" aria-label={AS_OF_HELP}>
          <InfoOutlined fontSize="small" />
        </IconButton>
      </Tooltip>
    </Stack>
  )
}
