import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import type { ReactNode } from 'react'

export function Metric({ label, value, emphasis = false }: { label: string; value: ReactNode; emphasis?: boolean }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary" component="div">
        {label}
      </Typography>
      <Typography component="div" sx={{ fontWeight: emphasis ? 700 : 500, fontSize: emphasis ? '1.25rem' : '1rem' }}>
        {value}
      </Typography>
    </Box>
  )
}
