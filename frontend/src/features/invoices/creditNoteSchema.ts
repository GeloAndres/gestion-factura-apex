import { z } from 'zod'
import { formatCLP, parseAmount } from '../../lib/format'

export function creditNoteSchema(pendingBalance: number) {
  return z.object({
    amount: z.string().transform((raw, context) => {
      if (raw.trim() === '') {
        context.addIssue({ code: 'custom', message: 'Ingresa el monto de la nota de crédito.' })
        return z.NEVER
      }

      const amount = parseAmount(raw)
      if (amount === null) {
        context.addIssue({ code: 'custom', message: 'Ingresa un monto entero en pesos, sin decimales (ej: 10.000).' })
        return z.NEVER
      }

      if (amount <= 0) {
        context.addIssue({ code: 'custom', message: 'El monto debe ser mayor a $0.' })
        return z.NEVER
      }

      if (amount > pendingBalance) {
        context.addIssue({
          code: 'custom',
          message: `No puede superar el saldo pendiente de la factura (${formatCLP(pendingBalance)}).`,
        })
        return z.NEVER
      }

      return amount
    }),
  })
}

export type CreditNoteFormInput = z.input<ReturnType<typeof creditNoteSchema>>
export type CreditNoteFormOutput = z.output<ReturnType<typeof creditNoteSchema>>
