import BlockRounded from '@mui/icons-material/BlockRounded'
import CheckCircleRounded from '@mui/icons-material/CheckCircleRounded'
import ContrastRounded from '@mui/icons-material/ContrastRounded'
import DescriptionOutlined from '@mui/icons-material/DescriptionOutlined'
import ErrorRounded from '@mui/icons-material/ErrorRounded'
import ScheduleRounded from '@mui/icons-material/ScheduleRounded'
import Chip from '@mui/material/Chip'
import type { SvgIconProps } from '@mui/material/SvgIcon'
import Tooltip from '@mui/material/Tooltip'
import type { InvoiceStatus, PaymentStatus } from '../api/types'
import { invoiceStatusMeta, paymentStatusColor, paymentStatusMeta } from '../lib/status'

const paymentIcons = { Paid: CheckCircleRounded, Pending: ScheduleRounded, Overdue: ErrorRounded }
const invoiceIcons = { Issued: DescriptionOutlined, Partial: ContrastRounded, Cancelled: BlockRounded }

/** Ícono del estado de pago, en su color de estado. */
export function PaymentStatusIcon({ status, ...props }: { status: PaymentStatus } & SvgIconProps) {
  const Icon = paymentIcons[status]
  return <Icon {...props} sx={{ color: paymentStatusColor[status], ...props.sx }} />
}

// El texto queda en el color de tinta normal; el color de estado va solo en el ícono.
const chipSx = (iconColor: string) => ({
  fontWeight: 600,
  bgcolor: 'background.paper',
  '& .MuiChip-icon': { color: iconColor },
})

export function PaymentStatusChip({ status }: { status: PaymentStatus }) {
  const meta = paymentStatusMeta(status)
  const Icon = paymentIcons[status]
  return (
    <Tooltip title={`${meta.description} (${status})`}>
      <Chip size="small" variant="outlined" icon={<Icon />} label={meta.label} sx={chipSx(paymentStatusColor[status])} />
    </Tooltip>
  )
}

export function InvoiceStatusChip({ status }: { status: InvoiceStatus }) {
  const meta = invoiceStatusMeta(status)
  const Icon = invoiceIcons[status]
  const iconColor = status === 'Partial' ? 'var(--mui-palette-info-main)' : 'var(--mui-palette-text-secondary)'
  return (
    <Tooltip title={`${meta.description} (${status})`}>
      <Chip size="small" variant="outlined" icon={<Icon />} label={meta.label} sx={chipSx(iconColor)} />
    </Tooltip>
  )
}
