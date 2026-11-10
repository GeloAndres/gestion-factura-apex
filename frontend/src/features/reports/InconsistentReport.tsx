import KeyboardArrowDownRounded from '@mui/icons-material/KeyboardArrowDownRounded'
import KeyboardArrowUpRounded from '@mui/icons-material/KeyboardArrowUpRounded'
import WarningAmberRounded from '@mui/icons-material/WarningAmberRounded'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import Collapse from '@mui/material/Collapse'
import IconButton from '@mui/material/IconButton'
import LinearProgress from '@mui/material/LinearProgress'
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
import { useState } from 'react'
import { errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { InconsistentInvoice } from '../../api/types'
import { CustomerCell } from '../../components/CustomerCell'
import { formatCLP, formatDate, formatInteger, formatSignedCLP } from '../../lib/format'
import { paymentTones } from '../../lib/status'

const numeric = { textAlign: 'right', fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const
const alertColor = paymentTones.Overdue.main

export function InconsistentReport() {
  const query = useQuery({
    queryKey: queryKeys.inconsistent,
    queryFn: ({ signal }) => invoicesApi.inconsistentInvoices(signal),
  })

  return (
    <Card>
      <Box sx={{ height: 2 }}>{query.isFetching && <LinearProgress aria-label="Cargando reporte" sx={{ height: 2 }} />}</Box>
      <Box sx={{ p: { xs: 2.5, sm: 3 } }}>
        <Typography variant="h2">Facturas inconsistentes</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, maxWidth: 720 }}>
          Su total declarado no coincide con la suma de sus productos. Se guardan para este reporte, pero quedan fuera del
          sistema activo: no aparecen en la búsqueda ni admiten notas de crédito.
        </Typography>
      </Box>

      {query.isError && (
        <Alert severity="error" sx={{ mx: 3, mb: 3 }}>
          {errorMessage(query.error)}
        </Alert>
      )}
      {query.isPending && <Skeleton variant="rounded" height={120} sx={{ mx: 3, mb: 3 }} />}
      {query.data?.length === 0 && (
        <Typography color="text.secondary" sx={{ px: 3, pb: 3 }}>
          No hay facturas inconsistentes.
        </Typography>
      )}

      {query.data && query.data.length > 0 && (
        <TableContainer sx={{ borderTop: 1, borderColor: 'divider' }}>
          <Table aria-label="Facturas inconsistentes" sx={{ minWidth: 760 }}>
            <TableHead>
              <TableRow>
                <TableCell padding="checkbox" />
                <TableCell>N°</TableCell>
                <TableCell>Cliente</TableCell>
                <TableCell>Emisión</TableCell>
                <TableCell sx={numeric}>Total declarado</TableCell>
                <TableCell sx={numeric}>Suma de productos</TableCell>
                <TableCell sx={numeric}>Diferencia</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {query.data.map((invoice) => (
                <InconsistentRow key={invoice.invoiceNumber} invoice={invoice} />
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Card>
  )
}

function InconsistentRow({ invoice }: { invoice: InconsistentInvoice }) {
  const [open, setOpen] = useState(false)
  const panelId = `inconsistent-lines-${invoice.invoiceNumber}`

  return (
    <>
      <TableRow hover onClick={() => setOpen((value) => !value)} sx={{ cursor: 'pointer', '& > td': { borderBottom: open ? 'none' : undefined } }}>
        <TableCell padding="checkbox">
          <IconButton
            size="small"
            aria-label={open ? 'Ocultar productos' : 'Ver productos'}
            aria-expanded={open}
            aria-controls={panelId}
            onClick={(event) => {
              event.stopPropagation()
              setOpen((value) => !value)
            }}
          >
            {open ? <KeyboardArrowUpRounded /> : <KeyboardArrowDownRounded />}
          </IconButton>
        </TableCell>
        <TableCell sx={{ fontWeight: 700 }}>{invoice.invoiceNumber}</TableCell>
        <TableCell sx={{ maxWidth: 260 }}>
          <CustomerCell name={invoice.customerName} run={invoice.customerRun} />
        </TableCell>
        <TableCell sx={{ whiteSpace: 'nowrap', color: 'text.secondary' }}>{formatDate(invoice.invoiceDate)}</TableCell>
        <TableCell sx={numeric}>{formatCLP(invoice.declaredTotal)}</TableCell>
        <TableCell sx={numeric}>{formatCLP(invoice.linesTotal)}</TableCell>
        <TableCell sx={{ ...numeric, fontWeight: 700 }}>{formatSignedCLP(invoice.difference)}</TableCell>
      </TableRow>
      <TableRow>
        <TableCell colSpan={7} sx={{ py: 0 }}>
          <Collapse in={open} unmountOnExit>
            <Box id={panelId} sx={{ my: 2, ml: { md: 7 }, borderRadius: 2, bgcolor: 'action.hover', overflow: 'hidden' }}>
              <Table size="small" aria-label={`Productos de la factura ${invoice.invoiceNumber}`}>
                <TableHead>
                  <TableRow>
                    <TableCell>Producto</TableCell>
                    <TableCell sx={numeric}>Precio</TableCell>
                    <TableCell sx={numeric}>Cantidad</TableCell>
                    <TableCell sx={numeric}>Precio × cantidad</TableCell>
                    <TableCell sx={numeric}>Subtotal declarado</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {invoice.lines.map((line, index) => (
                    <TableRow key={`${line.productName}-${index}`} sx={{ '&:last-child td': { borderBottom: 0 } }}>
                      <TableCell>{line.productName}</TableCell>
                      <TableCell sx={numeric}>{formatCLP(line.unitPrice)}</TableCell>
                      <TableCell sx={numeric}>{formatInteger(line.quantity)}</TableCell>
                      <TableCell sx={numeric}>{formatCLP(line.expectedSubtotal)}</TableCell>
                      <TableCell sx={numeric}>
                        {line.subtotalMatches ? (
                          formatCLP(line.subtotal)
                        ) : (
                          <Stack direction="row" sx={{ gap: 0.5, alignItems: 'center', justifyContent: 'flex-end', fontWeight: 600 }}>
                            <WarningAmberRounded fontSize="small" sx={{ color: alertColor }} />
                            <span>{formatCLP(line.subtotal)} · no cuadra</span>
                          </Stack>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          </Collapse>
        </TableCell>
      </TableRow>
    </>
  )
}
