import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import Grid from '@mui/material/Grid'
import LinearProgress from '@mui/material/LinearProgress'
import Skeleton from '@mui/material/Skeleton'
import Stack from '@mui/material/Stack'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { PaymentStatusSummary } from '../../api/types'
import { SAMPLE_AS_OF, useAsOf } from '../../context/asOfContext'
import { formatDate, formatInteger, formatPercent } from '../../lib/format'
import { paymentStatusMeta, paymentTones } from '../../lib/status'

type SummaryItem = PaymentStatusSummary['items'][number]

const describe = (item: SummaryItem) =>
  `${paymentStatusMeta(item.status).plural}: ${formatInteger(item.count)} (${formatPercent(item.percentage)})`

export function PaymentSummaryReport() {
  const { asOf, setAsOf } = useAsOf()
  const query = useQuery({
    queryKey: queryKeys.summary(asOf),
    queryFn: ({ signal }) => invoicesApi.paymentStatusSummary(asOf, signal),
  })
  const summary = query.data
  const pending = summary?.items.find((item) => item.status === 'Pending')?.count ?? 0

  return (
    <Card sx={{ p: { xs: 2.5, sm: 3 } }}>
      <Box sx={{ height: 2, mx: { xs: -2.5, sm: -3 }, mt: { xs: -2.5, sm: -3 }, mb: { xs: 2.5, sm: 3 } }}>
        {query.isFetching && <LinearProgress aria-label="Cargando resumen" sx={{ height: 2 }} />}
      </Box>
      <Typography variant="h2">Facturas por estado de pago</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
        Total y porcentaje de las facturas activas{summary ? ` al ${formatDate(summary.asOf)}` : ''}.
      </Typography>

      {query.isError && <Alert severity="error" sx={{ mt: 2 }}>{errorMessage(query.error)}</Alert>}
      {query.isPending && <Skeleton variant="rounded" height={180} sx={{ mt: 3 }} />}

      {summary && (
        <Stack sx={{ gap: 3, mt: 3 }}>
          <div>
            <Typography sx={{ fontSize: '2.5rem', fontWeight: 700, lineHeight: 1.1, letterSpacing: '-0.02em' }}>
              {formatInteger(summary.totalInvoices)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              facturas activas
            </Typography>
          </div>

          <SegmentedBar items={summary.items} />

          <Grid container spacing={2}>
            {summary.items.map((item) => (
              <Grid key={item.status} size={{ xs: 12, sm: 4 }}>
                <StatTile item={item} />
              </Grid>
            ))}
          </Grid>

          {pending === 0 && !asOf && (
            <Alert
              severity="info"
              action={
                <Button color="inherit" size="small" onClick={() => setAsOf(SAMPLE_AS_OF)}>
                  Ver al {formatDate(SAMPLE_AS_OF)}
                </Button>
              }
            >
              Hoy ninguna factura está pendiente: los datos de ejemplo vencen en 2024 y 2025.
            </Alert>
          )}
        </Stack>
      )}
    </Card>
  )
}

function SegmentedBar({ items }: { items: SummaryItem[] }) {
  const parts = items.filter((item) => item.count > 0)

  return (
    <Box
      role="img"
      aria-label={`Distribución por estado de pago. ${items.map(describe).join('; ')}.`}
      sx={{ display: 'flex', gap: '3px' }}
    >
      {parts.map((item) => (
        <Tooltip key={item.status} title={describe(item)} placement="top" arrow>
          <Box
            tabIndex={0}
            aria-label={describe(item)}
            sx={{
              flexGrow: item.count,
              flexBasis: 0,
              height: 12,
              borderRadius: 999,
              bgcolor: paymentTones[item.status].main,
              outlineOffset: 2,
              '&:focus-visible': { outline: '2px solid', outlineColor: 'primary.main' },
            }}
          />
        </Tooltip>
      ))}
    </Box>
  )
}

function StatTile({ item }: { item: SummaryItem }) {
  const meta = paymentStatusMeta(item.status)

  return (
    <Box sx={{ borderRadius: 3, p: 2, height: '100%', bgcolor: 'action.hover' }}>
      <Stack direction="row" sx={{ alignItems: 'center', gap: 1 }}>
        <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: paymentTones[item.status].main }} />
        <Typography variant="body2" sx={{ fontWeight: 600 }}>
          {meta.plural}
        </Typography>
      </Stack>
      <Stack direction="row" sx={{ alignItems: 'baseline', gap: 1, mt: 1 }}>
        <Typography sx={{ fontSize: '1.75rem', fontWeight: 700, lineHeight: 1.2 }}>{formatInteger(item.count)}</Typography>
        <Typography variant="body2" color="text.secondary">
          {formatPercent(item.percentage)}
        </Typography>
      </Stack>
      <Typography variant="caption" color="text.secondary">
        {meta.description}
      </Typography>
    </Box>
  )
}
