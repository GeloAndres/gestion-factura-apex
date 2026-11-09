import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
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
import { PaymentStatusIcon } from '../../components/StatusChips'
import { SAMPLE_AS_OF, useAsOf } from '../../context/asOfContext'
import { formatDate, formatInteger, formatPercent } from '../../lib/format'
import { paymentStatusColor, paymentStatusMeta } from '../../lib/status'

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
    <Card>
      {query.isFetching && <LinearProgress aria-label="Cargando resumen" />}
      <CardContent>
        <Stack sx={{ gap: 3 }}>
          <div>
            <Typography variant="h2">Facturas por estado de pago</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {summary
                ? `${formatInteger(summary.totalInvoices)} facturas activas al ${formatDate(summary.asOf)}.`
                : 'Total y porcentaje de facturas activas por estado de pago.'}
            </Typography>
          </div>

          {query.isError && <Alert severity="error">{errorMessage(query.error)}</Alert>}
          {query.isPending && <Skeleton variant="rounded" height={140} />}

          {summary && (
            <>
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
                  Hoy ninguna factura está pendiente: los vencimientos de los datos de ejemplo son de 2024 y 2025.
                </Alert>
              )}
            </>
          )}
        </Stack>
      </CardContent>
    </Card>
  )
}

function SegmentedBar({ items }: { items: SummaryItem[] }) {
  const parts = items.filter((item) => item.count > 0)

  return (
    <Box
      role="img"
      aria-label={`Distribución por estado de pago. ${items.map(describe).join('; ')}.`}
      sx={{ display: 'flex', gap: '2px' }}
    >
      {parts.map((item) => (
        <Tooltip key={item.status} title={describe(item)} placement="top" arrow>
          <Box
            tabIndex={0}
            aria-label={describe(item)}
            sx={{
              flexGrow: item.count,
              flexBasis: 0,
              py: 1,
              cursor: 'default',
              outlineOffset: 2,
              '&:focus-visible': { outline: '2px solid', outlineColor: 'primary.main', borderRadius: 1 },
            }}
          >
            <Box sx={{ height: 8, borderRadius: 999, bgcolor: paymentStatusColor[item.status] }} />
          </Box>
        </Tooltip>
      ))}
    </Box>
  )
}

function StatTile({ item }: { item: SummaryItem }) {
  const meta = paymentStatusMeta(item.status)

  return (
    <Box sx={{ border: 1, borderColor: 'divider', borderRadius: 2, p: 2, height: '100%' }}>
      <Stack direction="row" sx={{ alignItems: 'center', gap: 1 }}>
        <PaymentStatusIcon status={item.status} fontSize="small" />
        <Typography variant="body2" sx={{ fontWeight: 600 }} color="text.secondary">
          {meta.plural} ({item.status})
        </Typography>
      </Stack>
      <Typography sx={{ fontSize: '2rem', fontWeight: 650, lineHeight: 1.2, mt: 1 }}>{formatInteger(item.count)}</Typography>
      <Typography variant="body2" color="text.secondary">
        {formatPercent(item.percentage)} del total · {meta.description.toLowerCase()}
      </Typography>
    </Box>
  )
}
