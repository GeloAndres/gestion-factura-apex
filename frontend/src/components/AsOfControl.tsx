import CalendarMonthRounded from '@mui/icons-material/CalendarMonthRounded'
import ExpandMoreRounded from '@mui/icons-material/ExpandMoreRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import ButtonBase from '@mui/material/ButtonBase'
import Popover from '@mui/material/Popover'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import { SAMPLE_AS_OF, useAsOf } from '../context/asOfContext'
import { formatDate } from '../lib/format'
import { sidebar } from '../theme'

export function AsOfControl() {
  const { asOf, setAsOf } = useAsOf()
  const [anchor, setAnchor] = useState<HTMLElement | null>(null)

  const choose = (value: string | null) => {
    setAsOf(value)
    setAnchor(null)
  }

  return (
    <>
      <ButtonBase
        onClick={(event) => setAnchor(event.currentTarget)}
        aria-haspopup="dialog"
        aria-label={`Fecha para calcular el estado de pago: ${asOf ? formatDate(asOf) : 'hoy'}. Cambiar.`}
        sx={{
          width: '100%',
          justifyContent: 'flex-start',
          gap: 1.25,
          p: 1.25,
          borderRadius: 2,
          border: 1,
          borderColor: asOf ? 'rgba(251, 191, 36, 0.5)' : sidebar.border,
          color: sidebar.activeText,
          textAlign: 'left',
          '&:hover': { bgcolor: sidebar.hover },
          '&.Mui-focusVisible': { outline: `2px solid ${sidebar.accent}` },
        }}
      >
        <CalendarMonthRounded fontSize="small" sx={{ color: asOf ? '#fbbf24' : sidebar.text }} />
        <Box sx={{ flexGrow: 1, minWidth: 0 }}>
          <Typography component="div" sx={{ fontSize: '0.6875rem', color: sidebar.text }}>
            Estados de pago al
          </Typography>
          <Typography component="div" sx={{ fontSize: '0.875rem', fontWeight: 600 }}>
            {asOf ? formatDate(asOf) : 'Día de hoy'}
          </Typography>
        </Box>
        <ExpandMoreRounded fontSize="small" sx={{ color: sidebar.text }} />
      </ButtonBase>

      <Popover
        open={anchor !== null}
        anchorEl={anchor}
        onClose={() => setAnchor(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
        transformOrigin={{ vertical: 'bottom', horizontal: 'left' }}
        slotProps={{ paper: { sx: { p: 2.5, width: 300, ml: 3 } } }}
      >
        <Stack sx={{ gap: 2 }}>
          <div>
            <Typography variant="h3">Fecha de cálculo</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              Define si una factura sin pago está Pendiente o Vencida. Los datos de ejemplo vencen en 2024 y 2025.
            </Typography>
          </div>
          <TextField
            size="small"
            type="date"
            label="Fecha"
            value={asOf ?? ''}
            onChange={(event) => setAsOf(event.target.value || null)}
            slotProps={{ inputLabel: { shrink: true }, htmlInput: { min: '2000-01-01', max: '2100-12-31' } }}
          />
          <Stack direction="row" sx={{ gap: 1 }}>
            <Button variant={asOf ? 'outlined' : 'contained'} onClick={() => choose(null)} sx={{ flex: 1 }}>
              Hoy
            </Button>
            <Button
              variant={asOf === SAMPLE_AS_OF ? 'contained' : 'outlined'}
              onClick={() => choose(SAMPLE_AS_OF)}
              sx={{ flex: 1 }}
            >
              {formatDate(SAMPLE_AS_OF)}
            </Button>
          </Stack>
        </Stack>
      </Popover>
    </>
  )
}
