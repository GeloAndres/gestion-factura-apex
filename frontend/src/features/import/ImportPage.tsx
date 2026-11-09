import { zodResolver } from '@hookform/resolvers/zod'
import UploadFileRounded from '@mui/icons-material/UploadFileRounded'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import FormHelperText from '@mui/material/FormHelperText'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { CSSProperties } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { ImportResult } from '../../api/types'
import { Metric } from '../../components/Metric'
import { PageHeader } from '../../components/PageHeader'
import { formatInteger } from '../../lib/format'

const MAX_FILE_BYTES = 5 * 1024 * 1024

const importSchema = z.object({
  file: z
    .custom<File | null>((value) => value === null || value instanceof File)
    .refine((file) => file !== null, 'Selecciona un archivo .json.')
    .refine((file) => !file || file.name.toLowerCase().endsWith('.json'), 'El archivo debe tener extensión .json.')
    .refine((file) => !file || file.size > 0, 'El archivo está vacío.')
    .refine((file) => !file || file.size <= MAX_FILE_BYTES, 'El archivo no puede superar 5 MB.'),
})

type ImportFormInput = z.input<typeof importSchema>
type ImportFormOutput = z.output<typeof importSchema>

const visuallyHidden: CSSProperties = {
  clip: 'rect(0 0 0 0)',
  clipPath: 'inset(50%)',
  height: 1,
  overflow: 'hidden',
  position: 'absolute',
  whiteSpace: 'nowrap',
  width: 1,
}

export function ImportPage() {
  const queryClient = useQueryClient()
  const { control, handleSubmit, reset } = useForm<ImportFormInput, unknown, ImportFormOutput>({
    resolver: zodResolver(importSchema),
    defaultValues: { file: null },
  })

  const mutation = useMutation({
    mutationFn: (file: File) => invoicesApi.importFile(file),
    onSuccess: async () => {
      reset({ file: null })
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.invoices }),
        queryClient.invalidateQueries({ queryKey: queryKeys.reports }),
      ])
    },
  })

  return (
    <Stack sx={{ gap: 3 }}>
      <PageHeader
        title="Importar facturas"
        subtitle="Carga un archivo JSON con el mismo formato que bd_exam.json."
      />

      <Card>
        <CardContent>
          <Stack
            component="form"
            noValidate
            onSubmit={handleSubmit((values) => mutation.mutate(values.file))}
            sx={{ gap: 2 }}
          >
            <Typography variant="body2" color="text.secondary">
              Las facturas cuyo número ya existe se omiten, así que el mismo archivo se puede cargar varias veces sin
              duplicar datos. Las inconsistentes (total distinto de la suma de sus productos) se guardan, pero quedan
              fuera del sistema activo. Al iniciar, la API ya cargó <code>data/bd_exam.json</code>.
            </Typography>

            <Controller
              name="file"
              control={control}
              render={({ field, fieldState }) => (
                <Stack sx={{ gap: 0.5 }}>
                  <Stack direction={{ xs: 'column', sm: 'row' }} sx={{ gap: 2, alignItems: { sm: 'center' } }}>
                    <Button component="label" variant="outlined" startIcon={<UploadFileRounded />} sx={{ alignSelf: 'flex-start' }}>
                      Seleccionar archivo
                      <input
                        type="file"
                        accept=".json,application/json"
                        style={visuallyHidden}
                        onChange={(event) => {
                          field.onChange(event.target.files?.[0] ?? null)
                          event.target.value = ''
                        }}
                      />
                    </Button>
                    <Typography variant="body2" color={field.value ? 'text.primary' : 'text.secondary'}>
                      {field.value
                        ? `${field.value.name} (${formatInteger(Math.ceil(field.value.size / 1024))} KB)`
                        : 'Ningún archivo seleccionado'}
                    </Typography>
                  </Stack>
                  {fieldState.error && <FormHelperText error>{fieldState.error.message}</FormHelperText>}
                </Stack>
              )}
            />

            {mutation.isError && <Alert severity="error">{errorMessage(mutation.error)}</Alert>}

            <Box>
              <Button type="submit" variant="contained" loading={mutation.isPending}>
                Importar
              </Button>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {mutation.data && <ImportSummary result={mutation.data} />}
    </Stack>
  )
}

function ImportSummary({ result }: { result: ImportResult }) {
  return (
    <Card>
      <CardContent>
        <Stack sx={{ gap: 2.5 }}>
          <div>
            <Typography variant="h2">Resultado de la importación</Typography>
            <Typography variant="body2" color="text.secondary">
              Archivo {result.source}
            </Typography>
          </div>

          <Stack direction="row" sx={{ gap: 4, flexWrap: 'wrap' }}>
            <Metric label="Registros leídos" value={formatInteger(result.totalRecords)} emphasis />
            <Metric label="Importadas" value={formatInteger(result.imported)} emphasis />
            <Metric label="Inconsistentes (guardadas aparte)" value={formatInteger(result.importedInconsistent)} emphasis />
            <Metric label="Omitidas" value={formatInteger(result.skipped.length)} emphasis />
          </Stack>

          {result.skipped.length > 0 && (
            <ResultTable
              title="Registros omitidos"
              headers={['Posición', 'N° factura', 'Motivo']}
              rows={result.skipped.map((item) => [item.position, item.invoiceNumber ?? '—', item.reason])}
            />
          )}

          {result.issues.length > 0 && (
            <ResultTable
              title="Observaciones de las facturas importadas"
              headers={['N° factura', 'Tipo', 'Detalle']}
              rows={result.issues.map((issue) => [
                issue.invoiceNumber,
                issue.severity === 'Inconsistency' ? 'Inconsistencia' : 'Advertencia',
                issue.message,
              ])}
            />
          )}
        </Stack>
      </CardContent>
    </Card>
  )
}

function ResultTable({ title, headers, rows }: { title: string; headers: string[]; rows: (string | number)[][] }) {
  return (
    <div>
      <Typography variant="h3" component="h3" sx={{ mb: 1 }}>
        {title} ({formatInteger(rows.length)})
      </Typography>
      <TableContainer sx={{ maxHeight: 320, border: 1, borderColor: 'divider', borderRadius: 2 }}>
        <Table size="small" stickyHeader aria-label={title}>
          <TableHead>
            <TableRow>
              {headers.map((header) => (
                <TableCell key={header}>{header}</TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((row, index) => (
              <TableRow key={index}>
                {row.map((cell, cellIndex) => (
                  <TableCell key={cellIndex}>{cell}</TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </div>
  )
}
