import AddRounded from '@mui/icons-material/AddRounded'
import ArrowBackRounded from '@mui/icons-material/ArrowBackRounded'
import Alert from '@mui/material/Alert'
import AlertTitle from '@mui/material/AlertTitle'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Grid from '@mui/material/Grid'
import Link from '@mui/material/Link'
import Skeleton from '@mui/material/Skeleton'
import Snackbar from '@mui/material/Snackbar'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableFooter from '@mui/material/TableFooter'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState, type ReactNode } from 'react'
import { Link as RouterLink, useLocation, useParams } from 'react-router'
import { ApiError, errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { InvoiceDetail } from '../../api/types'
import { AsOfNotice } from '../../components/AsOfNotice'
import { Metric } from '../../components/Metric'
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
        <Button component={RouterLink} to={backTo} startIcon={<ArrowBackRounded />} size="small">
          Volver al listado
        </Button>
      </Box>
      <AsOfNotice />

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
            actions={
              <Stack direction="row" sx={{ gap: 1 }}>
                <InvoiceStatusChip status={invoice.invoiceStatus} />
                <PaymentStatusChip status={invoice.paymentStatus} />
              </Stack>
            }
          />

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 4 }}>
              <InfoCard title="Montos">
                <Metric label="Total" value={formatCLP(invoice.totalAmount)} />
                <Metric label="Notas de crédito" value={formatCLP(invoice.creditNotesTotal)} />
                <Metric label="Saldo pendiente" value={formatCLP(invoice.pendingBalance)} emphasis />
              </InfoCard>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <InfoCard title="Fechas">
                <Metric label="Emisión" value={formatDate(invoice.invoiceDate)} />
                <Metric label="Plazo" value={`${formatInteger(invoice.daysToDue)} días`} />
                <Metric
                  label="Vencimiento"
                  value={
                    <>
                      {formatDate(invoice.paymentDueDate)}
                      {invoice.daysOverdue !== null && (
                        <Typography component="span" variant="body2" color="text.secondary">
                          {' '}
                          · vencida hace {formatInteger(invoice.daysOverdue)} días
                        </Typography>
                      )}
                    </>
                  }
                />
              </InfoCard>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <InfoCard title="Cliente y pago">
                <Metric label="Cliente" value={invoice.customer.name} />
                <Metric label="RUN · correo" value={`${invoice.customer.run} · ${invoice.customer.email ?? 'sin correo'}`} />
                <Metric
                  label="Pago"
                  value={
                    invoice.payment
                      ? `${invoice.payment.method ?? 'Método no informado'} · ${formatDate(invoice.payment.paymentDate)}`
                      : 'Sin pago registrado'
                  }
                />
              </InfoCard>
            </Grid>
          </Grid>

          <LinesCard invoice={invoice} />

          <Card>
            <CardHeader
              disableTypography
              title={<Typography variant="h2">Notas de crédito</Typography>}
              subheader={
                <Typography variant="body2" color="text.secondary">
                  {invoice.pendingBalance === 0
                    ? 'La factura no tiene saldo pendiente: no admite más notas de crédito.'
                    : `Se pueden emitir notas por hasta ${formatCLP(invoice.pendingBalance)}.`}
                </Typography>
              }
              action={
                <Button
                  variant="contained"
                  startIcon={<AddRounded />}
                  onClick={() => setDialogOpen(true)}
                  disabled={invoice.pendingBalance === 0}
                >
                  Agregar nota de crédito
                </Button>
              }
              sx={{ flexWrap: 'wrap', gap: 1 }}
            />
            {invoice.creditNotes.length === 0 ? (
              <CardContent sx={{ pt: 0 }}>
                <Typography color="text.secondary">Esta factura no tiene notas de crédito.</Typography>
              </CardContent>
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
                      <TableRow key={note.number}>
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

          {invoice.importIssues.length > 0 && (
            <Alert severity="warning">
              <AlertTitle>Observaciones de la importación</AlertTitle>
              <Box component="ul" sx={{ m: 0, pl: 2.5 }}>
                {invoice.importIssues.map((issue) => (
                  <li key={issue.code + issue.message}>{issue.message}</li>
                ))}
              </Box>
            </Alert>
          )}

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

function InfoCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Typography variant="h3" component="h2" sx={{ mb: 1.5 }}>
          {title}
        </Typography>
        <Stack sx={{ gap: 1.25 }}>{children}</Stack>
      </CardContent>
    </Card>
  )
}

function LinesCard({ invoice }: { invoice: InvoiceDetail }) {
  const linesTotal = invoice.lines.reduce((sum, line) => sum + line.subtotal, 0)

  return (
    <Card>
      <CardHeader disableTypography title={<Typography variant="h2">Productos</Typography>} />
      <TableContainer>
        <Table size="small" aria-label="Productos de la factura">
          <TableHead>
            <TableRow>
              <TableCell>Producto</TableCell>
              <TableCell sx={numeric}>Precio unitario</TableCell>
              <TableCell sx={numeric}>Cantidad</TableCell>
              <TableCell sx={numeric}>Subtotal</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {invoice.lines.map((line, index) => (
              <TableRow key={`${line.productName}-${index}`}>
                <TableCell>{line.productName}</TableCell>
                <TableCell sx={numeric}>{formatCLP(line.unitPrice)}</TableCell>
                <TableCell sx={numeric}>{formatInteger(line.quantity)}</TableCell>
                <TableCell sx={numeric}>{formatCLP(line.subtotal)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
          <TableFooter>
            <TableRow>
              <TableCell colSpan={3} sx={{ fontWeight: 600, fontSize: '0.875rem', color: 'text.primary' }}>
                Suma de productos
              </TableCell>
              <TableCell sx={{ ...numeric, fontWeight: 600, fontSize: '0.875rem', color: 'text.primary' }}>
                {formatCLP(linesTotal)}
              </TableCell>
            </TableRow>
          </TableFooter>
        </Table>
      </TableContainer>
    </Card>
  )
}

function DetailSkeleton() {
  return (
    <Stack sx={{ gap: 2 }} aria-label="Cargando factura">
      <Skeleton variant="text" width="40%" height={48} />
      <Grid container spacing={2}>
        {[0, 1, 2].map((key) => (
          <Grid key={key} size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rounded" height={170} />
          </Grid>
        ))}
      </Grid>
      <Skeleton variant="rounded" height={160} />
    </Stack>
  )
}
