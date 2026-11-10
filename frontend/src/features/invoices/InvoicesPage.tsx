import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import LinearProgress from '@mui/material/LinearProgress'
import Link from '@mui/material/Link'
import Skeleton from '@mui/material/Skeleton'
import Stack from '@mui/material/Stack'
import Tab from '@mui/material/Tab'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TablePagination from '@mui/material/TablePagination'
import TableRow from '@mui/material/TableRow'
import Tabs from '@mui/material/Tabs'
import Typography from '@mui/material/Typography'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { Link as RouterLink, useLocation, useNavigate, useSearchParams } from 'react-router'
import { errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { PaymentStatus } from '../../api/types'
import { CustomerCell } from '../../components/CustomerCell'
import { PageHeader } from '../../components/PageHeader'
import { InvoiceStatusChip, PaymentStatusChip } from '../../components/StatusChips'
import { SAMPLE_AS_OF, useAsOf } from '../../context/asOfContext'
import { formatCLP, formatDate, formatInteger } from '../../lib/format'
import { isPaymentStatus } from '../../lib/status'
import { InvoiceFiltersForm } from './InvoiceFiltersForm'
import { PAGE_SIZES, readSearchParams, toSearchParams } from './searchParams'

const COLUMNS = 8
const numeric = { textAlign: 'right', fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const

const paymentTabs: { value: PaymentStatus | 'all'; label: string }[] = [
  { value: 'all', label: 'Todas' },
  { value: 'Pending', label: 'Pendientes' },
  { value: 'Overdue', label: 'Vencidas' },
  { value: 'Paid', label: 'Pagadas' },
]

export function InvoicesPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const search = readSearchParams(searchParams)
  const { asOf, setAsOf } = useAsOf()
  const navigate = useNavigate()
  const location = useLocation()

  const query = useQuery({
    queryKey: queryKeys.search(search, asOf),
    queryFn: ({ signal }) => invoicesApi.search(search, asOf, signal),
    placeholderData: keepPreviousData,
  })

  const update = (next: Partial<typeof search>) => setSearchParams(toSearchParams({ ...search, ...next }))
  const hasFilters = Boolean(search.number || search.invoiceStatus || search.paymentStatus)
  const detailState = { search: location.search }

  const page = query.data
  const rows = page?.items ?? []
  const lastPageIndex = Math.max(0, (page?.totalPages ?? 1) - 1)
  const dateText = asOf ? `al ${formatDate(asOf)}` : 'a la fecha de hoy'

  return (
    <Stack sx={{ gap: 3 }}>
      <PageHeader
        title="Facturas"
        subtitle={
          page
            ? `${formatInteger(page.totalItems)} ${page.totalItems === 1 ? 'factura activa' : 'facturas activas'} · estado de pago ${dateText}`
            : `Facturas activas · estado de pago ${dateText}`
        }
      />

      <Card>
        <Box sx={{ px: 2, borderBottom: 1, borderColor: 'divider' }}>
          <Tabs
            value={search.paymentStatus ?? 'all'}
            onChange={(_, value: string) => update({ paymentStatus: isPaymentStatus(value) ? value : undefined, page: 1 })}
            variant="scrollable"
            allowScrollButtonsMobile
            aria-label="Filtrar por estado de pago"
          >
            {paymentTabs.map((tab) => (
              <Tab key={tab.value} value={tab.value} label={tab.label} />
            ))}
          </Tabs>
        </Box>

        <InvoiceFiltersForm
          number={search.number}
          invoiceStatus={search.invoiceStatus}
          hasFilters={hasFilters}
          onChange={(filters) => update({ ...filters, page: 1 })}
          onClear={() => setSearchParams(new URLSearchParams())}
        />

        <Box sx={{ height: 2 }}>{query.isFetching && <LinearProgress aria-label="Cargando facturas" sx={{ height: 2 }} />}</Box>

        {query.isError ? (
          <Alert severity="error" sx={{ m: 2 }}>
            {errorMessage(query.error)}
          </Alert>
        ) : (
          <TableContainer>
            <Table aria-label="Listado de facturas" sx={{ minWidth: 880 }}>
              <TableHead>
                <TableRow>
                  <TableCell>N°</TableCell>
                  <TableCell>Cliente</TableCell>
                  <TableCell>Emisión</TableCell>
                  <TableCell>Vencimiento</TableCell>
                  <TableCell sx={numeric}>Total</TableCell>
                  <TableCell sx={numeric}>Saldo</TableCell>
                  <TableCell>Factura</TableCell>
                  <TableCell>Pago</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {query.isPending &&
                  Array.from({ length: 6 }, (_, index) => (
                    <TableRow key={index}>
                      <TableCell colSpan={COLUMNS}>
                        <Skeleton height={32} />
                      </TableCell>
                    </TableRow>
                  ))}

                {rows.map((row) => (
                  <TableRow
                    key={row.invoiceNumber}
                    hover
                    onClick={() => navigate(`/facturas/${row.invoiceNumber}`, { state: detailState })}
                    sx={{ cursor: 'pointer', '&:last-child td': { borderBottom: 0 } }}
                  >
                    <TableCell>
                      <Link
                        component={RouterLink}
                        to={`/facturas/${row.invoiceNumber}`}
                        state={detailState}
                        onClick={(event) => event.stopPropagation()}
                        underline="hover"
                        sx={{ fontWeight: 700 }}
                      >
                        {row.invoiceNumber}
                      </Link>
                    </TableCell>
                    <TableCell sx={{ maxWidth: 260 }}>
                      <CustomerCell name={row.customerName} run={row.customerRun} />
                    </TableCell>
                    <TableCell sx={{ whiteSpace: 'nowrap', color: 'text.secondary' }}>{formatDate(row.invoiceDate)}</TableCell>
                    <TableCell sx={{ whiteSpace: 'nowrap', color: 'text.secondary' }}>{formatDate(row.paymentDueDate)}</TableCell>
                    <TableCell sx={numeric}>{formatCLP(row.totalAmount)}</TableCell>
                    <TableCell sx={{ ...numeric, fontWeight: 600 }}>{formatCLP(row.pendingBalance)}</TableCell>
                    <TableCell>
                      <InvoiceStatusChip status={row.invoiceStatus} />
                    </TableCell>
                    <TableCell>
                      <PaymentStatusChip status={row.paymentStatus} />
                    </TableCell>
                  </TableRow>
                ))}

                {query.isSuccess && rows.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={COLUMNS} sx={{ py: 6, borderBottom: 0 }}>
                      <Stack sx={{ gap: 1.5, alignItems: 'center', textAlign: 'center' }}>
                        <Typography sx={{ fontWeight: 600 }}>No hay facturas que coincidan con los filtros</Typography>
                        {search.paymentStatus === 'Pending' && !asOf ? (
                          <>
                            <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 520 }}>
                              A la fecha de hoy ninguna factura sigue dentro del plazo: los datos de ejemplo vencen en 2024 y
                              2025. Prueba con una fecha de cálculo anterior.
                            </Typography>
                            <Button variant="outlined" size="small" onClick={() => setAsOf(SAMPLE_AS_OF)}>
                              Ver al {formatDate(SAMPLE_AS_OF)}
                            </Button>
                          </>
                        ) : (
                          hasFilters && (
                            <Button size="small" onClick={() => setSearchParams(new URLSearchParams())}>
                              Limpiar filtros
                            </Button>
                          )
                        )}
                      </Stack>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        )}
        <TablePagination
          component="div"
          count={page?.totalItems ?? 0}
          page={page ? Math.min(search.page - 1, lastPageIndex) : 0}
          rowsPerPage={search.pageSize}
          rowsPerPageOptions={[...PAGE_SIZES]}
          onPageChange={(_, nextPage) => update({ page: nextPage + 1 })}
          onRowsPerPageChange={(event) => update({ page: 1, pageSize: Number(event.target.value) })}
          labelRowsPerPage="Por página"
          labelDisplayedRows={({ from, to, count }) => `${from}–${to} de ${count}`}
          getItemAriaLabel={(type) =>
            type === 'next' ? 'Página siguiente' : type === 'previous' ? 'Página anterior' : type === 'first' ? 'Primera página' : 'Última página'
          }
          sx={{ borderTop: 1, borderColor: 'divider' }}
        />
      </Card>
    </Stack>
  )
}
