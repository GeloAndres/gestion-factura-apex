import { zodResolver } from '@hookform/resolvers/zod'
import SearchRounded from '@mui/icons-material/SearchRounded'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { invoiceStatuses, paymentStatuses } from '../../lib/status'
import type { InvoiceFilters } from './searchParams'

const filtersSchema = z.object({
  number: z
    .string()
    .trim()
    .regex(/^\d*$/, 'Ingresa solo números.')
    .refine((value) => value === '' || Number(value) > 0, 'El número debe ser mayor a 0.'),
  invoiceStatus: z.enum(['', 'Issued', 'Partial', 'Cancelled']),
  paymentStatus: z.enum(['', 'Pending', 'Overdue', 'Paid']),
})

type FiltersFormValues = z.infer<typeof filtersSchema>

interface InvoiceFiltersFormProps {
  initial: InvoiceFilters
  onSearch: (filters: InvoiceFilters) => void
  onClear: () => void
}

export function InvoiceFiltersForm({ initial, onSearch, onClear }: InvoiceFiltersFormProps) {
  const { control, handleSubmit, reset } = useForm<FiltersFormValues>({
    resolver: zodResolver(filtersSchema),
    defaultValues: {
      number: initial.number?.toString() ?? '',
      invoiceStatus: initial.invoiceStatus ?? '',
      paymentStatus: initial.paymentStatus ?? '',
    },
  })

  const submit = handleSubmit((values) =>
    onSearch({
      number: values.number ? Number(values.number) : undefined,
      invoiceStatus: values.invoiceStatus || undefined,
      paymentStatus: values.paymentStatus || undefined,
    }),
  )

  const clear = () => {
    reset({ number: '', invoiceStatus: '', paymentStatus: '' })
    onClear()
  }

  return (
    <Card component="section" aria-label="Filtros de búsqueda">
      <CardContent>
        <Stack
          component="form"
          noValidate
          onSubmit={submit}
          direction={{ xs: 'column', md: 'row' }}
          sx={{ gap: 2, alignItems: { md: 'flex-start' } }}
        >
          <Controller
            name="number"
            control={control}
            render={({ field: { ref, ...field }, fieldState }) => (
              <TextField
                {...field}
                inputRef={ref}
                size="small"
                label="N° de factura"
                placeholder="Ej: 30"
                error={Boolean(fieldState.error)}
                helperText={fieldState.error?.message}
                slotProps={{ htmlInput: { inputMode: 'numeric' } }}
                sx={{ minWidth: 160 }}
              />
            )}
          />
          <Controller
            name="invoiceStatus"
            control={control}
            render={({ field: { ref, ...field } }) => (
              <TextField {...field} inputRef={ref} select size="small" label="Estado de factura" sx={{ minWidth: 210 }}>
                <MenuItem value="">Todos</MenuItem>
                {invoiceStatuses.map((status) => (
                  <MenuItem key={status.value} value={status.value}>
                    {status.label} ({status.value})
                  </MenuItem>
                ))}
              </TextField>
            )}
          />
          <Controller
            name="paymentStatus"
            control={control}
            render={({ field: { ref, ...field } }) => (
              <TextField {...field} inputRef={ref} select size="small" label="Estado de pago" sx={{ minWidth: 210 }}>
                <MenuItem value="">Todos</MenuItem>
                {paymentStatuses.map((status) => (
                  <MenuItem key={status.value} value={status.value}>
                    {status.label} ({status.value})
                  </MenuItem>
                ))}
              </TextField>
            )}
          />
          <Stack direction="row" sx={{ gap: 1 }}>
            <Button type="submit" variant="contained" startIcon={<SearchRounded />} sx={{ height: 40 }}>
              Buscar
            </Button>
            <Button type="button" onClick={clear} sx={{ height: 40 }}>
              Limpiar
            </Button>
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  )
}
