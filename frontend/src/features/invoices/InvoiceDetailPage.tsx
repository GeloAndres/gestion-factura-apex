import AddRounded from '@mui/icons-material/AddRounded'
import ArrowBackRounded from '@mui/icons-material/ArrowBackRounded'
import Alert from '@mui/material/Alert'
import AlertTitle from '@mui/material/AlertTitle'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import Divider from '@mui/material/Divider'
import Grid from '@mui/material/Grid'
import Link from '@mui/material/Link'
import Skeleton from '@mui/material/Skeleton'
import Snackbar from '@mui/material/Snackbar'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState, type ReactNode } from 'react'
import { Link as RouterLink, useLocation, useParams } from 'react-router'
import { ApiError, errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { InvoiceDetail } from '../../api/types'
import { PageHeader } from '../../components/PageHeader'
import { InvoiceStatusChip, PaymentStatusChip } from '../../components/StatusChips'
import { useAsOf } from '../../context/asOfContext'
import { formatCLP, formatDate, formatInteger } from '../../lib/format'
import { invoiceStatusMeta } from '../../lib/status'
import { AddCreditNoteDialog } from './AddCreditNoteDialog'

const numeric = { textAlign: 'right', fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const

export function InvoiceDetailPage() {
  const params = useParams()
  const location = useLocation()
  const { asOf } = useAsOf()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)

  const invoiceNumber = Number(params.invoiceNumber)
  const isValidNumber = Number.isInteger(invoiceNumber) && invoiceNumber > 0
  const backTo = `/facturas${(location.state as { search?: string } | null)?.search ?? ''}`

  const query = useQuery({
    queryKey: queryKeys.detail(invoiceNumber, asOf),
    queryFn: ({ signal }) => invoicesApi.getByNumber(invoiceNumber, asOf, signal),
    enabled: isValidNumber,
  })

  const invoice = query.data
  const notFound = !isValidNumber || (query.error instanceof ApiError && query.error.status === 404)

  return (
    <Stack sx={{ gap: 3 }}>
      <Box>
        <Button component={RouterLink} to={backTo} startIcon={<ArrowBackRounded />} size="small" color="inherit" sx={{ color: 'text.secondary' }}>
          Facturas
        </Button>
      </Box>

      {notFound && (
        <Alert severity="warning">
          <AlertTitle>No encontramos la factura N° {params.invoiceNumber} en el sistema activo</AlertTitle>
          Si su total no cuadra con la suma de sus productos, es inconsistente y aparece en{' '}
          <Link component={RouterLink} to="/reportes?vista=inconsistentes">
            Reportes → Inconsistentes
          </Link>
          .
        </Alert>
      )}
      {query.isError && !notFound && <Alert severity="error">{errorMessage(query.error)}</Alert>}
      {isValidNumber && query.isPending && <DetailSkeleton />}

      {invoice && (
        <>
          <PageHeader
            title={`Factura N° ${invoice.invoiceNumber}`}
            subtitle={`${invoice.customer.name} · emitida el ${formatDate(invoice.invoiceDate)}`}
          />
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 8 }} sx={{ order: { xs: 2, md: 1 } }}>
              <Stack sx={{ gap: 3 }}>
                <InvoiceDocument invoice={invoice} />
                {invoice.importIssues.length > 0 && (
                  <Alert severity="warning" variant="outlined" sx={{ bgcolor: 'background.paper' }}>
                    <AlertTitle>Observaciones de la importación</AlertTitle>
                    <Box component="ul" sx={{ m: 0, pl: 2.5 }}>
                      {invoice.importIssues.map((issue) => (
                        <li key={issue.code + issue.message}>{issue.message}</li>
                      ))}
                    </Box>
                  </Alert>
                )}
              </Stack>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }} sx={{ order: { xs: 1, md: 2 } }}>
              <SummaryPanel invoice={invoice} onAddCreditNote={() => setDialogOpen(true)} />
            </Grid>
          </Grid>

          <AddCreditNoteDialog
            open={dialogOpen}
            invoice={invoice}
            onClose={() => setDialogOpen(false)}
            onCreated={(created) => {
              setDialogOpen(false)
              setNotice(
                `Nota de crédito N° ${created.number} por ${formatCLP(created.amount)} creada. ` +
                  `Estado de la factura: ${invoiceStatusMeta(created.invoiceStatus).label}.`,
              )
            }}
          />
        </>
      )}

      <Snackbar
        open={notice !== null}
        autoHideDuration={6000}
        onClose={() => setNotice(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" variant="filled" onClose={() => setNotice(null)}>
          {notice}
        </Alert>
      </Snackbar>
    </Stack>
  )
}

function InvoiceDocument({ invoice }: { invoice: InvoiceDetail }) {
  return (
    <Card component="article" aria-label={`Detalle de la factura N° ${invoice.invoiceNumber}`} sx={{ p: { xs: 2.5, sm: 4 } }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} sx={{ gap: 3, justifyContent: 'space-between' }}>
        <div>
          <Typography variant="overline" color="text.secondary">
            Cliente
          </Typography>
          <Typography sx={{ fontWeight: 600 }}>{invoice.customer.name}</Typography>
          <Typography variant="body2" color="text.secondary">
            RUN {invoice.customer.run}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {invoice.customer.email ?? 'Sin correo'}
          </Typography>
        </div>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'auto auto', columnGap: 3, rowGap: 0.75, alignSelf: 'flex-start' }}>
          <DateRow label="Emisión" value={formatDate(invoice.invoiceDate)} />
          <DateRow label="Vencimiento" value={formatDate(invoice.paymentDueDate)} />
          <DateRow label="Plazo" value={`${formatInteger(invoice.daysToDue)} días`} />
        </Box>
      </Stack>

      <TableContainer sx={{ mt: 3 }}>
        <Table
          aria-label="Productos de la factura"
          sx={{
            minWidth: 480,
            borderCollapse: 'separate',
            borderSpacing: '0 6px',
            '& td, & th': { border: 0 },
            '& tbody td': { bgcolor: 'action.hover' },
            '& tbody td:first-of-type': { borderTopLeftRadius: 8, borderBottomLeftRadius: 8 },
            '& tbody td:last-of-type': { borderTopRightRadius: 8, borderBottomRightRadius: 8 },
          }}
        >
          <TableHead>
            <TableRow>
              <TableCell>Descripción</TableCell>
              <TableCell sx={numeric}>Cantidad</TableCell>
              <TableCell sx={numeric}>Precio</TableCell>
              <TableCell sx={numeric}>Total</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {invoice.lines.map((line, index) => (
              <TableRow key={`${line.productName}-${index}`}>
                <TableCell sx={{ fontWeight: 500 }}>{line.productName}</TableCell>
                <TableCell sx={numeric}>{formatInteger(line.quantity)}</TableCell>
                <TableCell sx={numeric}>{formatCLP(line.unitPrice)}</TableCell>
                <TableCell sx={numeric}>{formatCLP(line.subtotal)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Stack sx={{ gap: 1, mt: 2, ml: 'auto', maxWidth: 320 }}>
        <AmountRow label="Total" value={formatCLP(invoice.totalAmount)} />
        <AmountRow label="Notas de crédito" value={invoice.creditNotesTotal ? `−${formatCLP(invoice.creditNotesTotal)}` : formatCLP(0)} />
        <Divider />
        <AmountRow label="Saldo pendiente" value={formatCLP(invoice.pendingBalance)} strong />
      </Stack>

      <Divider sx={{ my: 3 }} />

      <Typography variant="h2" component="h2" sx={{ mb: 1.5 }}>
        Notas de crédito
      </Typography>
      {invoice.creditNotes.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          Esta factura no tiene notas de crédito.
        </Typography>
      ) : (
        <TableContainer>
          <Table size="small" aria-label="Notas de crédito">
            <TableHead>
              <TableRow>
                <TableCell>N°</TableCell>
                <TableCell>Fecha</TableCell>
                <TableCell sx={numeric}>Monto</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {invoice.creditNotes.map((note) => (
                <TableRow key={note.number} sx={{ '&:last-child td': { borderBottom: 0 } }}>
                  <TableCell>{note.number}</TableCell>
                  <TableCell>{formatDate(note.date)}</TableCell>
                  <TableCell sx={numeric}>{formatCLP(note.amount)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Card>
  )
}

function SummaryPanel({ invoice, onAddCreditNote }: { invoice: InvoiceDetail; onAddCreditNote: () => void }) {
  const hasBalance = invoice.pendingBalance > 0

  return (
    <Card sx={{ p: 3, position: { md: 'sticky' }, top: { md: 24 } }}>
      <Typography variant="overline" color="text.secondary">
        Saldo pendiente
      </Typography>
      <Typography sx={{ fontSize: '1.75rem', fontWeight: 700, lineHeight: 1.2, letterSpacing: '-0.02em' }}>
        {formatCLP(invoice.pendingBalance)}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
        de {formatCLP(invoice.totalAmount)} facturados
      </Typography>

      <Divider sx={{ my: 2.5 }} />

      <Stack sx={{ gap: 1.75 }}>
        <PanelRow label="Estado de factura" value={<InvoiceStatusChip status={invoice.invoiceStatus} />} />
        <PanelRow
          label="Estado de pago"
          value={<PaymentStatusChip status={invoice.paymentStatus} />}
          hint={
            invoice.daysOverdue !== null
              ? `Hace ${formatInteger(invoice.daysOverdue)} días · al ${formatDate(invoice.asOf)}`
              : `Al ${formatDate(invoice.asOf)}`
          }
        />
        <PanelRow
          label="Pago"
          value={invoice.payment ? formatDate(invoice.payment.paymentDate) : 'Sin pago'}
          hint={invoice.payment ? (invoice.payment.method ?? 'Método no informado') : undefined}
        />
      </Stack>

      <Divider sx={{ my: 2.5 }} />

      <Button variant="contained" size="large" fullWidth startIcon={<AddRounded />} onClick={onAddCreditNote} disabled={!hasBalance}>
        Agregar nota de crédito
      </Button>
      <Typography variant="caption" color="text.secondary" component="p" sx={{ mt: 1, textAlign: 'center' }}>
        {hasBalance
          ? `Hasta ${formatCLP(invoice.pendingBalance)}. La fecha se asigna sola.`
          : 'La factura no tiene saldo pendiente.'}
      </Typography>
    </Card>
  )
}

function PanelRow({ label, value, hint }: { label: string; value: ReactNode; hint?: string }) {
  return (
    <Stack direction="row" sx={{ gap: 2, justifyContent: 'space-between', alignItems: 'flex-start' }}>
      <Typography variant="body2" color="text.secondary" sx={{ pt: 0.25 }}>
        {label}
      </Typography>
      <Box sx={{ textAlign: 'right', minWidth: 0 }}>
        <Typography component="div" variant="body2" sx={{ fontWeight: 500 }}>
          {value}
        </Typography>
        {hint && (
          <Typography variant="caption" color="text.secondary" component="div">
            {hint}
          </Typography>
        )}
      </Box>
    </Stack>
  )
}

function DateRow({ label, value }: { label: string; value: string }) {
  return (
    <>
      <Typography variant="overline" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ textAlign: 'right', lineHeight: 1.85 }}>
        {value}
      </Typography>
    </>
  )
}

function AmountRow({ label, value, strong = false }: { label: string; value: string; strong?: boolean }) {
  return (
    <Stack direction="row" sx={{ justifyContent: 'space-between', gap: 2 }}>
      <Typography variant="body2" color={strong ? 'text.primary' : 'text.secondary'} sx={{ fontWeight: strong ? 700 : 400 }}>
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: strong ? 700 : 500, fontVariantNumeric: 'tabular-nums' }}>
        {value}
      </Typography>
    </Stack>
  )
}

function DetailSkeleton() {
  return (
    <Grid container spacing={3} aria-label="Cargando factura">
      <Grid size={{ xs: 12, md: 8 }}>
        <Skeleton variant="rounded" height={480} />
      </Grid>
      <Grid size={{ xs: 12, md: 4 }}>
        <Skeleton variant="rounded" height={360} />
      </Grid>
    </Grid>
  )
}
