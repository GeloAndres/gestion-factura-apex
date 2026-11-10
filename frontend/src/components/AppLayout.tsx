import AssessmentRounded from '@mui/icons-material/AssessmentRounded'
import LogoutRounded from '@mui/icons-material/LogoutRounded'
import MenuRounded from '@mui/icons-material/MenuRounded'
import ReceiptLongRounded from '@mui/icons-material/ReceiptLongRounded'
import UploadFileRounded from '@mui/icons-material/UploadFileRounded'
import AppBar from '@mui/material/AppBar'
import Avatar from '@mui/material/Avatar'
import Box from '@mui/material/Box'
import Drawer from '@mui/material/Drawer'
import IconButton from '@mui/material/IconButton'
import List from '@mui/material/List'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
import Stack from '@mui/material/Stack'
import Toolbar from '@mui/material/Toolbar'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { NavLink, Outlet } from 'react-router'
import { sessionStore } from '../auth/session'
import { useSession } from '../auth/useSession'
import { sidebar } from '../theme'
import { AsOfControl } from './AsOfControl'
import { Brand } from './Brand'

const navItems = [
  { to: '/facturas', label: 'Facturas', icon: <ReceiptLongRounded fontSize="small" /> },
  { to: '/reportes', label: 'Reportes', icon: <AssessmentRounded fontSize="small" /> },
  { to: '/importar', label: 'Importar', icon: <UploadFileRounded fontSize="small" /> },
]

const drawerPaperSx = {
  width: sidebar.width,
  bgcolor: sidebar.background,
  color: sidebar.text,
  borderRight: 'none',
}

export function AppLayout() {
  const [menuOpen, setMenuOpen] = useState(false)
  const content = <SidebarContent onNavigate={() => setMenuOpen(false)} />

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar
        position="fixed"
        elevation={0}
        sx={{ display: { md: 'none' }, bgcolor: sidebar.background, color: sidebar.activeText }}
      >
        <Toolbar sx={{ gap: 1 }}>
          <IconButton color="inherit" edge="start" aria-label="Abrir menú" onClick={() => setMenuOpen(true)}>
            <MenuRounded />
          </IconButton>
          <Brand />
        </Toolbar>
      </AppBar>

      <Drawer
        variant="temporary"
        open={menuOpen}
        onClose={() => setMenuOpen(false)}
        sx={{ display: { xs: 'block', md: 'none' }, '& .MuiDrawer-paper': drawerPaperSx }}
      >
        {content}
      </Drawer>
      <Drawer
        variant="permanent"
        open
        sx={{
          display: { xs: 'none', md: 'block' },
          width: sidebar.width,
          flexShrink: 0,
          '& .MuiDrawer-paper': drawerPaperSx,
        }}
      >
        {content}
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1, minWidth: 0, px: { xs: 2, md: 5 }, pt: { xs: 11, md: 5 }, pb: 6 }}>
        <Box sx={{ maxWidth: 1180, mx: 'auto' }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}

function SidebarContent({ onNavigate }: { onNavigate: () => void }) {
  const session = useSession()
  const queryClient = useQueryClient()

  const logout = () => {
    sessionStore.set(null)
    queryClient.clear()
  }

  return (
    <Stack sx={{ height: '100%', p: 2, gap: 3 }}>
      <Box sx={{ px: 1, pt: 1, color: sidebar.activeText }}>
        <Brand />
      </Box>

      <List component="nav" aria-label="Secciones" disablePadding sx={{ display: 'grid', gap: 0.5 }}>
        {navItems.map((item) => (
          <ListItemButton
            key={item.to}
            component={NavLink}
            to={item.to}
            onClick={onNavigate}
            sx={{
              borderRadius: 2,
              py: 1,
              color: sidebar.text,
              '&:hover': { bgcolor: sidebar.hover, color: sidebar.activeText },
              '&.active': {
                bgcolor: sidebar.active,
                color: sidebar.activeText,
                '& .MuiListItemIcon-root': { color: sidebar.accent },
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 36, color: 'inherit' }}>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontWeight: 600, fontSize: '0.875rem' } } }} />
          </ListItemButton>
        ))}
      </List>

      <Box sx={{ flexGrow: 1 }} />

      <AsOfControl />

      <Stack direction="row" sx={{ alignItems: 'center', gap: 1.5, pt: 2, borderTop: 1, borderColor: sidebar.border }}>
        <Avatar sx={{ width: 32, height: 32, fontSize: '0.875rem', fontWeight: 700, bgcolor: '#2563eb' }}>
          {session?.username.charAt(0).toUpperCase()}
        </Avatar>
        <Box sx={{ flexGrow: 1, minWidth: 0 }}>
          <Typography variant="body2" noWrap sx={{ color: sidebar.activeText, fontWeight: 600 }}>
            {session?.username}
          </Typography>
          <Typography component="div" sx={{ fontSize: '0.75rem', color: sidebar.text }}>
            Sesión iniciada
          </Typography>
        </Box>
        <Tooltip title="Cerrar sesión">
          <IconButton aria-label="Cerrar sesión" onClick={logout} sx={{ color: sidebar.text, '&:hover': { color: sidebar.activeText } }}>
            <LogoutRounded fontSize="small" />
          </IconButton>
        </Tooltip>
      </Stack>
    </Stack>
  )
}
