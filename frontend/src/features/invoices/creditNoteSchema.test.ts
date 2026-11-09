import { describe, expect, it } from 'vitest'
import { creditNoteSchema } from './creditNoteSchema'

// Factura #2: saldo pendiente de $31.867.
const schema = creditNoteSchema(31867)

describe('creditNoteSchema', () => {
  it.each([
    ['31867', 31867],
    ['31.867', 31867],
    ['$ 1.000', 1000],
  ])('acepta "%s"', (input, expected) => {
    expect(schema.parse({ amount: input })).toEqual({ amount: expected })
  })

  it.each([
    ['', 'Ingresa el monto'],
    ['   ', 'Ingresa el monto'],
    ['10,5', 'entero'],
    ['-5', 'entero'],
    ['0', 'mayor a $0'],
    ['31.868', 'saldo pendiente'],
  ])('rechaza "%s" con un mensaje claro', (input, message) => {
    const result = schema.safeParse({ amount: input })

    expect(result.success).toBe(false)
    expect(result.error?.issues).toHaveLength(1)
    expect(result.error?.issues[0]?.message).toContain(message)
  })
})
