import KeyboardArrowDownRounded from '@mui/icons-material/KeyboardArrowDownRounded'
import KeyboardArrowUpRounded from '@mui/icons-material/KeyboardArrowUpRounded'
import WarningAmberRounded from '@mui/icons-material/WarningAmberRounded'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
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
import { formatCLP, formatDate, formatInteger, formatSignedCLP } from '../../lib/format'

const numeric = { textAlign: 'right', fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const

export function InconsistentReport() {
  const query = useQuery({
    queryKey: queryKeys.inconsistent,
    queryFn: ({ signal }) => invoicesApi.inconsistentInvoices(signal),
  })

  return (
    <Card>
      {query.isFetching && <LinearProgress aria-label="Cargando reporte" />}
      <CardContent>
        <Typography variant="h2">Facturas inconsistentes</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          El total declarado no coincide con la suma de los subtotales de sus productos. Se conservan en la base de
          datos para este reporte, pero no forman parte del sistema activo (no se buscan ni admiten notas de crédito).
        </Typography>
      </CardContent>

      {query.isError && (
        <Alert severity="error" sx={{ m: 2 }}>
          {errorMessage(query.error)}
        </Alert>
      )}
      {query.isPending && <Skeleton variant="rounded" height={120} sx={{ m: 2 }} />}
      {query.data?.length === 0 && (
        <CardContent sx={{ pt: 0 }}>
          <Typography color="text.secondary">No hay facturas inconsistentes.</Typography>
        </CardContent>
      )}

      {query.data && query.data.length > 0 && (
        <TableContainer>
          <Table size="small" aria-label="Facturas inconsistentes">
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
      <TableRow hover sx={{ '& > td': { borderBottom: open ? 'none' : undefined } }}>
        <TableCell padding="checkbox">
          <IconButton
            size="small"
            aria-label={open ? 'Ocultar productos' : 'Ver productos'}
            aria-expanded={open}
            aria-controls={panelId}
            onClick={() => setOpen((value) => !value)}
          >
            {open ? <KeyboardArrowUpRounded /> : <KeyboardArrowDownRounded />}
          </IconButton>
        </TableCell>
        <TableCell sx={{ fontWeight: 700 }}>{invoice.invoiceNumber}</TableCell>
        <TableCell>
          <Typography variant="body2">{invoice.customerName}</Typography>
          <Typography variant="caption" color="text.secondary">
            RUN {invoice.customerRun}
          </Typography>
        </TableCell>
        <TableCell sx={{ whiteSpace: 'nowrap' }}>{formatDate(invoice.invoiceDate)}</TableCell>
        <TableCell sx={numeric}>{formatCLP(invoice.declaredTotal)}</TableCell>
        <TableCell sx={numeric}>{formatCLP(invoice.linesTotal)}</TableCell>
        <TableCell sx={{ ...numeric, fontWeight: 700 }}>{formatSignedCLP(invoice.difference)}</TableCell>
      </TableRow>
      <TableRow>
        <TableCell colSpan={7} sx={{ py: 0 }}>
          <Collapse in={open} unmountOnExit>
            <Box id={panelId} sx={{ py: 2, pl: { md: 7 } }}>
              <Table size="small" aria-label={`Productos de la factura ${invoice.invoiceNumber}`}>
                <TableHead>
                  <TableRow>
                    <TableCell>Producto</TableCell>
                    <TableCell sx={numeric}>Precio unitario</TableCell>
                    <TableCell sx={numeric}>Cantidad</TableCell>
                    <TableCell sx={numeric}>Precio × cantidad</TableCell>
                    <TableCell sx={numeric}>Subtotal declarado</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {invoice.lines.map((line, index) => (
                    <TableRow key={`${line.productName}-${index}`}>
                      <TableCell>{line.productName}</TableCell>
                      <TableCell sx={numeric}>{formatCLP(line.unitPrice)}</TableCell>
                      <TableCell sx={numeric}>{formatInteger(line.quantity)}</TableCell>
                      <TableCell sx={numeric}>{formatCLP(line.expectedSubtotal)}</TableCell>
                      <TableCell sx={numeric}>
                        {line.subtotalMatches ? (
                          formatCLP(line.subtotal)
                        ) : (
                          <Stack direction="row" sx={{ gap: 0.5, alignItems: 'center', justifyContent: 'flex-end' }}>
                            <WarningAmberRounded fontSize="small" sx={{ color: '#d03b3b' }} />
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
