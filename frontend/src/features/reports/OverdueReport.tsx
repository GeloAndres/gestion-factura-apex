import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import LinearProgress from '@mui/material/LinearProgress'
import Link from '@mui/material/Link'
import Skeleton from '@mui/material/Skeleton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { Link as RouterLink } from 'react-router'
import { errorMessage } from '../../api/client'
import { OVERDUE_REPORT_DAYS, invoicesApi, queryKeys } from '../../api/invoicesApi'
import { CustomerCell } from '../../components/CustomerCell'
import { useAsOf } from '../../context/asOfContext'
import { formatCLP, formatDate, formatInteger } from '../../lib/format'
import { paymentTones } from '../../lib/status'

const numeric = { textAlign: 'right', fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const

export function OverdueReport() {
  const { asOf } = useAsOf()
  const query = useQuery({
    queryKey: queryKeys.overdue(asOf),
    queryFn: ({ signal }) => invoicesApi.overdueReport(asOf, signal),
  })
  const report = query.data

  return (
    <Card>
      <Box sx={{ height: 2 }}>{query.isFetching && <LinearProgress aria-label="Cargando reporte" sx={{ height: 2 }} />}</Box>
      <Box sx={{ p: { xs: 2.5, sm: 3 } }}>
        <Typography variant="h2">Vencidas hace más de {OVERDUE_REPORT_DAYS} días</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Facturas consistentes sin pago ni notas de crédito{report ? `, al ${formatDate(report.asOf)}` : ''}.
        </Typography>
        {report && (
          <Stack direction="row" sx={{ gap: { xs: 4, sm: 6 }, mt: 3, flexWrap: 'wrap' }}>
            <BigNumber label="Facturas" value={formatInteger(report.count)} />
            <BigNumber label="Monto por cobrar" value={formatCLP(report.totalAmount)} />
          </Stack>
        )}
      </Box>

      {query.isError && (
        <Alert severity="error" sx={{ mx: 3, mb: 3 }}>
          {errorMessage(query.error)}
        </Alert>
      )}
      {query.isPending && <Skeleton variant="rounded" height={200} sx={{ mx: 3, mb: 3 }} />}

      {report && report.items.length === 0 && (
        <Typography color="text.secondary" sx={{ px: 3, pb: 3 }}>
          No hay facturas en esta situación a la fecha indicada.
        </Typography>
      )}

      {report && report.items.length > 0 && (
        <TableContainer sx={{ borderTop: 1, borderColor: 'divider' }}>
          <Table aria-label={`Facturas vencidas hace más de ${OVERDUE_REPORT_DAYS} días`} sx={{ minWidth: 640 }}>
            <TableHead>
              <TableRow>
                <TableCell>N°</TableCell>
                <TableCell>Cliente</TableCell>
                <TableCell>Vencimiento</TableCell>
                <TableCell sx={numeric}>Días vencida</TableCell>
                <TableCell sx={numeric}>Total</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {report.items.map((item) => (
                <TableRow key={item.invoiceNumber} hover sx={{ '&:last-child td': { borderBottom: 0 } }}>
                  <TableCell>
                    <Link component={RouterLink} to={`/facturas/${item.invoiceNumber}`} underline="hover" sx={{ fontWeight: 700 }}>
                      {item.invoiceNumber}
                    </Link>
                  </TableCell>
                  <TableCell sx={{ maxWidth: 280 }}>
                    <CustomerCell name={item.customerName} run={item.customerRun} />
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap', color: 'text.secondary' }}>{formatDate(item.paymentDueDate)}</TableCell>
                  <TableCell
                    sx={(theme) => ({
                      ...numeric,
                      fontWeight: 600,
                      color: paymentTones.Overdue.text,
                      ...theme.applyStyles('dark', { color: paymentTones.Overdue.textDark }),
                    })}
                  >
                    {formatInteger(item.daysOverdue)}
                  </TableCell>
                  <TableCell sx={{ ...numeric, fontWeight: 600 }}>{formatCLP(item.totalAmount)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Card>
  )
}

function BigNumber({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <Typography sx={{ fontSize: '2rem', fontWeight: 700, lineHeight: 1.15, letterSpacing: '-0.02em' }}>{value}</Typography>
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
    </div>
  )
}
