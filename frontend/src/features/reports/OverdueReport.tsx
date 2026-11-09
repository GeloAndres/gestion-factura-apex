import Alert from '@mui/material/Alert'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
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
import { Metric } from '../../components/Metric'
import { useAsOf } from '../../context/asOfContext'
import { formatCLP, formatDate, formatInteger } from '../../lib/format'

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
      {query.isFetching && <LinearProgress aria-label="Cargando reporte" />}
      <CardContent>
        <Typography variant="h2">Vencidas hace más de {OVERDUE_REPORT_DAYS} días, sin pago ni notas de crédito</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Facturas consistentes sin pago registrado ni notas de crédito, cuyo vencimiento pasó hace más de{' '}
          {OVERDUE_REPORT_DAYS} días{report ? ` al ${formatDate(report.asOf)}` : ''}.
        </Typography>
        {report && (
          <Stack direction="row" sx={{ gap: 4, mt: 2 }}>
            <Metric label="Facturas" value={formatInteger(report.count)} emphasis />
            <Metric label="Monto por cobrar" value={formatCLP(report.totalAmount)} emphasis />
          </Stack>
        )}
      </CardContent>

      {query.isError && (
        <Alert severity="error" sx={{ m: 2 }}>
          {errorMessage(query.error)}
        </Alert>
      )}
      {query.isPending && <Skeleton variant="rounded" height={200} sx={{ m: 2 }} />}

      {report && report.items.length === 0 && (
        <CardContent sx={{ pt: 0 }}>
          <Typography color="text.secondary">No hay facturas en esta situación a la fecha indicada.</Typography>
        </CardContent>
      )}

      {report && report.items.length > 0 && (
        <TableContainer>
          <Table size="small" aria-label="Facturas vencidas hace más de 30 días">
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
                <TableRow key={item.invoiceNumber} hover>
                  <TableCell>
                    <Link component={RouterLink} to={`/facturas/${item.invoiceNumber}`} sx={{ fontWeight: 700 }}>
                      {item.invoiceNumber}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{item.customerName}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      RUN {item.customerRun}
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>{formatDate(item.paymentDueDate)}</TableCell>
                  <TableCell sx={numeric}>{formatInteger(item.daysOverdue)}</TableCell>
                  <TableCell sx={numeric}>{formatCLP(item.totalAmount)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Card>
  )
}
