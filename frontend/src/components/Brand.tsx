import ReceiptLongRounded from '@mui/icons-material/ReceiptLongRounded'
import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

export function Brand({ color = 'inherit' }: { color?: string }) {
  return (
    <Stack direction="row" sx={{ alignItems: 'center', gap: 1.25 }}>
      <Box
        sx={{
          width: 32,
          height: 32,
          borderRadius: 2,
          display: 'grid',
          placeItems: 'center',
          bgcolor: '#2563eb',
          color: '#ffffff',
          flexShrink: 0,
        }}
      >
        <ReceiptLongRounded fontSize="small" />
      </Box>
      <Typography component="span" sx={{ fontWeight: 700, fontSize: '1rem', letterSpacing: '-0.01em', color }}>
        Gestión de Facturas
      </Typography>
    </Stack>
  )
}
