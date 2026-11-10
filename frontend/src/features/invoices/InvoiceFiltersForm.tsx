import { zodResolver } from '@hookform/resolvers/zod'
import CloseRounded from '@mui/icons-material/CloseRounded'
import FilterAltOffRounded from '@mui/icons-material/FilterAltOffRounded'
import SearchRounded from '@mui/icons-material/SearchRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import InputAdornment from '@mui/material/InputAdornment'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import { useEffect, useEffectEvent } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import type { InvoiceStatus } from '../../api/types'
import { invoiceStatuses, isInvoiceStatus } from '../../lib/status'

const SEARCH_DELAY_MS = 400

const searchSchema = z.object({
  number: z
    .string()
    .trim()
    .regex(/^\d*$/, 'Ingresa solo números.')
    .refine((value) => value === '' || Number(value) > 0, 'El número debe ser mayor a 0.'),
})

type SearchFormValues = z.infer<typeof searchSchema>

interface InvoiceFiltersFormProps {
  number?: number
  invoiceStatus?: InvoiceStatus
  hasFilters: boolean
  onChange: (filters: { number?: number; invoiceStatus?: InvoiceStatus }) => void
  onClear: () => void
}

export function InvoiceFiltersForm({ number, invoiceStatus, hasFilters, onChange, onClear }: InvoiceFiltersFormProps) {
  const applied = number?.toString() ?? ''
  const { control, handleSubmit, setValue } = useForm<SearchFormValues>({
    resolver: zodResolver(searchSchema),
    defaultValues: { number: applied },
  })
  const typed = useWatch({ control, name: 'number' })

  useEffect(() => {
    setValue('number', applied)
  }, [applied, setValue])

  const search = handleSubmit((values) => onChange({ number: values.number ? Number(values.number) : undefined }))
  const searchAfterPause = useEffectEvent(() => void search())

  useEffect(() => {
    if (typed.trim() === applied) return
    const timer = setTimeout(searchAfterPause, SEARCH_DELAY_MS)
    return () => clearTimeout(timer)
  }, [typed, applied])

  return (
    <Stack
      component="form"
      noValidate
      onSubmit={search}
      direction={{ xs: 'column', sm: 'row' }}
      sx={{ gap: 1.5, p: 2, alignItems: { sm: 'flex-start' } }}
    >
      <Controller
        name="number"
        control={control}
        render={({ field: { ref, ...field }, fieldState }) => (
          <TextField
            {...field}
            inputRef={ref}
            size="small"
            placeholder="Buscar por N° de factura"
            error={Boolean(fieldState.error)}
            helperText={fieldState.error?.message}
            sx={{ width: { sm: 260 } }}
            slotProps={{
              htmlInput: { inputMode: 'numeric', 'aria-label': 'Buscar por número de factura' },
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchRounded fontSize="small" />
                  </InputAdornment>
                ),
                endAdornment: field.value ? (
                  <InputAdornment position="end">
                    <IconButton
                      size="small"
                      edge="end"
                      aria-label="Borrar búsqueda"
                      onClick={() => {
                        setValue('number', '', { shouldValidate: true })
                        onChange({ number: undefined })
                      }}
                    >
                      <CloseRounded fontSize="small" />
                    </IconButton>
                  </InputAdornment>
                ) : undefined,
              },
            }}
          />
        )}
      />
      <TextField
        select
        size="small"
        value={invoiceStatus ?? ''}
        onChange={(event) => onChange({ invoiceStatus: isInvoiceStatus(event.target.value) ? event.target.value : undefined })}
        sx={{ minWidth: { sm: 210 } }}
        slotProps={{
          select: {
            displayEmpty: true,
            renderValue: (value) =>
              isInvoiceStatus(String(value))
                ? `Factura: ${invoiceStatuses.find((status) => status.value === value)?.plural.toLowerCase()}`
                : 'Estado de factura: todos',
          },
          htmlInput: { 'aria-label': 'Estado de factura' },
        }}
      >
        <MenuItem value="">Todos</MenuItem>
        {invoiceStatuses.map((status) => (
          <MenuItem key={status.value} value={status.value}>
            {status.plural}
          </MenuItem>
        ))}
      </TextField>
      <Box sx={{ flexGrow: 1 }} />
      {hasFilters && (
        <Button onClick={onClear} startIcon={<FilterAltOffRounded />} sx={{ alignSelf: { xs: 'flex-start', sm: 'center' } }}>
          Limpiar filtros
        </Button>
      )}
    </Stack>
  )
}
