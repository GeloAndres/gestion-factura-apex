import { createTheme } from '@mui/material/styles'

export const theme = createTheme({
  cssVariables: true,
  colorSchemes: {
    light: {
      palette: {
        primary: { main: '#1d4ed8' },
        background: { default: '#f6f7f9', paper: '#ffffff' },
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
    h1: { fontSize: '1.75rem', fontWeight: 700, letterSpacing: '-0.01em' },
    h2: { fontSize: '1.25rem', fontWeight: 650 },
    h3: { fontSize: '1rem', fontWeight: 650 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  components: {
    MuiButton: { defaultProps: { disableElevation: true } },
    MuiCard: { defaultProps: { variant: 'outlined' } },
    MuiTableCell: {
      styleOverrides: {
        head: ({ theme }) => ({ fontWeight: 600, color: theme.vars?.palette.text.secondary, whiteSpace: 'nowrap' }),
      },
    },
  },
})
