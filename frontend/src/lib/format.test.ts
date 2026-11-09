import { describe, expect, it } from 'vitest'
import { formatCLP, formatDate, formatPercent, formatSignedCLP, parseAmount } from './format'

describe('formatCLP', () => {
  it('usa punto como separador de miles y no muestra decimales', () => {
    expect(formatCLP(265019)).toBe('$265.019')
    expect(formatCLP(0)).toBe('$0')
  })

  it('formatSignedCLP antepone el signo al símbolo', () => {
    expect(formatSignedCLP(-137165)).toBe('−$137.165')
    expect(formatSignedCLP(1000)).toBe('+$1.000')
    expect(formatSignedCLP(0)).toBe('$0')
  })
})

describe('formatPercent', () => {
  it('usa coma decimal y un decimal como máximo', () => {
    expect(formatPercent(27.1).replace(/\s/g, '')).toBe('27,1%')
    expect(formatPercent(0).replace(/\s/g, '')).toBe('0%')
  })
})

describe('formatDate', () => {
  it('formatea la fecha ISO sin desplazarla por zona horaria', () => {
    expect(formatDate('2025-03-01')).toBe('01-03-2025')
  })
})

describe('parseAmount', () => {
  it.each([
    ['12000', 12000],
    ['12.000', 12000],
    ['$ 31.867', 31867],
  ])('acepta "%s"', (input, expected) => {
    expect(parseAmount(input)).toBe(expected)
  })

  it.each(['', '12,5', '-100', 'abc', '1e5'])('rechaza "%s"', (input) => {
    expect(parseAmount(input)).toBeNull()
  })
})
