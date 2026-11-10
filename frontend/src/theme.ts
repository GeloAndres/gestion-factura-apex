import { createTheme } from '@mui/material/styles'

export const sidebar = {
  width: 248,
  background: '#161b26',
  border: 'rgba(255, 255, 255, 0.08)',
  text: 'rgba(255, 255, 255, 0.68)',
  hover: 'rgba(255, 255, 255, 0.06)',
  active: 'rgba(255, 255, 255, 0.1)',
  activeText: '#ffffff',
  accent: '#93b4ff',
} as const

export const theme = createTheme({
  cssVariables: true,
  colorSchemes: {
    light: {
      palette: {
        primary: { main: '#2563eb' },
        background: { default: '#f4f5f7', paper: '#ffffff' },
        text: { primary: '#111827', secondary: '#6b7280' },
        divider: '#e5e7eb',
      },
    },
    dark: {
      palette: {
        primary: { main: '#7aa7ff' },
        background: { default: '#0f1115', paper: '#171a21' },
      },
    },
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: 'system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
    h1: { fontSize: '1.625rem', fontWeight: 700, letterSpacing: '-0.02em' },
    h2: { fontSize: '1.0625rem', fontWeight: 650 },
    h3: { fontSize: '0.9375rem', fontWeight: 650 },
    overline: { fontSize: '0.6875rem', fontWeight: 600, letterSpacing: '0.06em', lineHeight: 1.6 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { borderRadius: 8 } },
    },
    MuiCard: {
      defaultProps: { variant: 'outlined' },
      styleOverrides: { root: { borderRadius: 14 } },
    },
    MuiTableCell: {
      styleOverrides: {
        root: ({ theme }) => ({ borderColor: theme.vars?.palette.divider }),
        head: ({ theme }) => ({
          fontSize: '0.6875rem',
          fontWeight: 600,
          letterSpacing: '0.06em',
          textTransform: 'uppercase',
          color: theme.vars?.palette.text.secondary,
          whiteSpace: 'nowrap',
        }),
      },
    },
    MuiTab: {
      styleOverrides: { root: { textTransform: 'none', fontWeight: 600, minHeight: 48 } },
    },
  },
})
