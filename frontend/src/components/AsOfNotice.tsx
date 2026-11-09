import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import { useAsOf } from '../context/asOfContext'
import { formatDate } from '../lib/format'

export function AsOfNotice() {
  const { asOf, setAsOf } = useAsOf()
  if (!asOf) return null

  return (
    <Alert
      severity="info"
      action={
        <Button color="inherit" size="small" onClick={() => setAsOf(null)}>
          Volver a hoy
        </Button>
      }
    >
      Los estados de pago se calculan al <strong>{formatDate(asOf)}</strong> (fecha de referencia), no a la fecha de hoy.
    </Alert>
  )
}
