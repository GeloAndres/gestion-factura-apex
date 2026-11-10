import Box from '@mui/material/Box'
import { alpha } from '@mui/material/styles'
import Tooltip from '@mui/material/Tooltip'
import type { InvoiceStatus, PaymentStatus } from '../api/types'
import { invoiceStatusDot, invoiceStatusMeta, paymentStatusMeta, paymentTones } from '../lib/status'

const pillSx = {
  display: 'inline-flex',
  alignItems: 'center',
  gap: 0.75,
  px: 1.25,
  py: 0.25,
  borderRadius: 999,
  fontSize: '0.75rem',
  fontWeight: 600,
  lineHeight: 1.7,
  whiteSpace: 'nowrap',
} as const

function Dot({ color }: { color: string }) {
  return <Box component="span" sx={{ width: 6, height: 6, borderRadius: '50%', bgcolor: color, flexShrink: 0 }} />
}

export function PaymentStatusChip({ status }: { status: PaymentStatus }) {
  const meta = paymentStatusMeta(status)
  const tone = paymentTones[status]
  return (
    <Tooltip title={meta.description}>
      <Box
        component="span"
        sx={(theme) => ({
          ...pillSx,
          bgcolor: alpha(tone.main, 0.12),
          color: tone.text,
          ...theme.applyStyles('dark', { bgcolor: alpha(tone.main, 0.2), color: tone.textDark }),
        })}
      >
        <Dot color={tone.main} />
        {meta.label}
      </Box>
    </Tooltip>
  )
}

export function InvoiceStatusChip({ status }: { status: InvoiceStatus }) {
  const meta = invoiceStatusMeta(status)
  return (
    <Tooltip title={meta.description}>
      <Box component="span" sx={{ ...pillSx, bgcolor: 'action.hover', color: 'text.primary' }}>
        <Dot color={invoiceStatusDot[status]} />
        {meta.label}
      </Box>
    </Tooltip>
  )
}
