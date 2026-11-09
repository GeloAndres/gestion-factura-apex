import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
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
import TablePagination from '@mui/material/TablePagination'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { Link as RouterLink, useLocation, useNavigate, useSearchParams } from 'react-router'
import { errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import { AsOfNotice } from '../../components/AsOfNotice'
import { PageHeader } from '../../components/PageHeader'
import { InvoiceStatusChip, PaymentStatusChip } from '../../components/StatusChips'
import { SAMPLE_AS_OF, useAsOf } from '../../context/asOfContext'
import { formatCLP, formatDate } from '../../lib/format'
import { InvoiceFiltersForm } from './InvoiceFiltersForm'
import { PAGE_SIZES, readSearchParams, toSearchParams, type InvoiceFilters } from './searchParams'

const COLUMNS = 8
const numeric = { textAlign: 'right', fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const

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
  const applyFilters = (filters: InvoiceFilters) => update({ ...filters, page: 1 })
  const filtersKey = `${search.number ?? ''}|${search.invoiceStatus ?? ''}|${search.paymentStatus ?? ''}`
  const detailState = { search: location.search }

  const page = query.data
  const rows = page?.items ?? []
  const lastPageIndex = Math.max(0, (page?.totalPages ?? 1) - 1)

  return (
    <Stack sx={{ gap: 3 }}>
      <PageHeader
        title="Facturas"
        subtitle={`Facturas activas (consistentes). Estado de pago calculado ${asOf ? `al ${formatDate(asOf)}` : 'a la fecha de hoy'}.`}
      />
      <AsOfNotice />

      <InvoiceFiltersForm
        key={filtersKey}
        initial={search}
        onSearch={applyFilters}
        onClear={() => setSearchParams(new URLSearchParams())}
      />

      <Card>
        {query.isFetching && <LinearProgress aria-label="Cargando facturas" />}
        {query.isError ? (
          <Alert severity="error" sx={{ m: 2 }}>
            {errorMessage(query.error)}
          </Alert>
        ) : (
          <TableContainer>
            <Table size="small" aria-label="Listado de facturas">
              <TableHead>
                <TableRow>
                  <TableCell>N°</TableCell>
                  <TableCell>Emisión</TableCell>
                  <TableCell>Vencimiento</TableCell>
                  <TableCell>Cliente</TableCell>
                  <TableCell sx={numeric}>Total</TableCell>
                  <TableCell sx={numeric}>Saldo</TableCell>
                  <TableCell>Estado factura</TableCell>
                  <TableCell>Estado pago</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {query.isPending &&
                  Array.from({ length: 5 }, (_, index) => (
                    <TableRow key={index}>
                      <TableCell colSpan={COLUMNS}>
                        <Skeleton />
                      </TableCell>
                    </TableRow>
                  ))}

                {rows.map((row) => (
                  <TableRow
                    key={row.invoiceNumber}
                    hover
                    onClick={() => navigate(`/facturas/${row.invoiceNumber}`, { state: detailState })}
                    sx={{ cursor: 'pointer' }}
                  >
                    <TableCell>
                      <Link
                        component={RouterLink}
                        to={`/facturas/${row.invoiceNumber}`}
                        state={detailState}
                        onClick={(event) => event.stopPropagation()}
                        sx={{ fontWeight: 700 }}
                      >
                        {row.invoiceNumber}
                      </Link>
                    </TableCell>
                    <TableCell sx={{ whiteSpace: 'nowrap' }}>{formatDate(row.invoiceDate)}</TableCell>
                    <TableCell sx={{ whiteSpace: 'nowrap' }}>{formatDate(row.paymentDueDate)}</TableCell>
                    <TableCell>
                      <Typography variant="body2">{row.customerName}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        RUN {row.customerRun}
                      </Typography>
                    </TableCell>
                    <TableCell sx={numeric}>{formatCLP(row.totalAmount)}</TableCell>
                    <TableCell sx={numeric}>{formatCLP(row.pendingBalance)}</TableCell>
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
                    <TableCell colSpan={COLUMNS} sx={{ py: 4 }}>
                      <Stack sx={{ gap: 1.5, alignItems: 'center', textAlign: 'center' }}>
                        <Typography>No hay facturas que coincidan con los filtros.</Typography>
                        {search.paymentStatus === 'Pending' && !asOf && (
                          <>
                            <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 560 }}>
                              A la fecha de hoy ninguna factura sigue dentro del plazo: los vencimientos de los datos de
                              ejemplo son de 2024 y 2025. Prueba con una fecha de referencia anterior.
                            </Typography>
                            <Button variant="outlined" size="small" onClick={() => setAsOf(SAMPLE_AS_OF)}>
                              Ver al {formatDate(SAMPLE_AS_OF)}
                            </Button>
                          </>
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
          getItemAriaLabel={(type) => (type === 'next' ? 'Página siguiente' : type === 'previous' ? 'Página anterior' : type === 'first' ? 'Primera página' : 'Última página')}
        />
      </Card>
    </Stack>
  )
}
