import { zodResolver } from '@hookform/resolvers/zod'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import InputAdornment from '@mui/material/InputAdornment'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useMemo } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { CreditNoteCreated, InvoiceDetail } from '../../api/types'
import { formatCLP, formatInteger } from '../../lib/format'
import { creditNoteSchema, type CreditNoteFormInput, type CreditNoteFormOutput } from './creditNoteSchema'

interface AddCreditNoteDialogProps {
  open: boolean
  invoice: InvoiceDetail
  onClose: () => void
  onCreated: (created: CreditNoteCreated) => void
}

export function AddCreditNoteDialog({ open, invoice, onClose, onCreated }: AddCreditNoteDialogProps) {
  const queryClient = useQueryClient()
  const schema = useMemo(() => creditNoteSchema(invoice.pendingBalance), [invoice.pendingBalance])

  const { control, handleSubmit, reset, setError, setValue } = useForm<CreditNoteFormInput, unknown, CreditNoteFormOutput>({
    resolver: zodResolver(schema),
    defaultValues: { amount: '' },
  })

  const mutation = useMutation({
    mutationFn: (amount: number) => invoicesApi.addCreditNote(invoice.invoiceNumber, amount),
    onSuccess: async (created) => {
      // El estado y el saldo cambian: se refrescan el detalle, el listado y los reportes.
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.invoices }),
        queryClient.invalidateQueries({ queryKey: queryKeys.reports }),
      ])
      reset({ amount: '' })
      onCreated(created)
    },
    // La API valida de nuevo (por ejemplo, si otra persona agregó una NC entre medio).
    onError: (error) => setError('amount', { message: errorMessage(error) }),
  })

  const close = () => {
    if (mutation.isPending) return
    reset({ amount: '' })
    mutation.reset()
    onClose()
  }

  return (
    <Dialog open={open} onClose={close} fullWidth maxWidth="xs" aria-labelledby="credit-note-title">
      <form noValidate onSubmit={handleSubmit((values) => mutation.mutate(values.amount))}>
        <DialogTitle id="credit-note-title">Agregar nota de crédito</DialogTitle>
        <DialogContent>
          <Stack sx={{ gap: 2, pt: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Factura N° {invoice.invoiceNumber}. Saldo pendiente: <strong>{formatCLP(invoice.pendingBalance)}</strong>.
              La fecha de la nota se asigna automáticamente (hoy).
            </Typography>
            <Controller
              name="amount"
              control={control}
              render={({ field: { ref, ...field }, fieldState }) => (
                <TextField
                  {...field}
                  inputRef={ref}
                  label="Monto"
                  placeholder="Ej: 10.000"
                  required
                  autoFocus
                  fullWidth
                  error={Boolean(fieldState.error)}
                  helperText={fieldState.error?.message ?? 'Entero en pesos, sin decimales.'}
                  slotProps={{
                    input: { startAdornment: <InputAdornment position="start">$</InputAdornment> },
                    htmlInput: { inputMode: 'numeric', 'aria-describedby': 'credit-note-title' },
                  }}
                />
              )}
            />
            <Button
              size="small"
              sx={{ alignSelf: 'flex-start' }}
              onClick={() => setValue('amount', formatInteger(invoice.pendingBalance), { shouldValidate: true })}
            >
              Usar el saldo completo ({formatCLP(invoice.pendingBalance)})
            </Button>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={close} disabled={mutation.isPending}>
            Cancelar
          </Button>
          <Button type="submit" variant="contained" loading={mutation.isPending}>
            Agregar
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}
