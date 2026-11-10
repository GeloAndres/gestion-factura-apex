import { zodResolver } from '@hookform/resolvers/zod'
import CloseRounded from '@mui/icons-material/CloseRounded'
import DescriptionOutlined from '@mui/icons-material/DescriptionOutlined'
import UploadFileRounded from '@mui/icons-material/UploadFileRounded'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import FormHelperText from '@mui/material/FormHelperText'
import Grid from '@mui/material/Grid'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, type CSSProperties, type DragEvent } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { errorMessage } from '../../api/client'
import { invoicesApi, queryKeys } from '../../api/invoicesApi'
import type { ImportResult } from '../../api/types'
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
  const [dragging, setDragging] = useState(false)
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
      <PageHeader title="Importar facturas" subtitle="Carga un archivo JSON con el mismo formato que bd_exam.json." />

      <Card sx={{ p: { xs: 2.5, sm: 3 } }}>
        <Stack component="form" noValidate onSubmit={handleSubmit((values) => mutation.mutate(values.file))} sx={{ gap: 2.5 }}>
          <Controller
            name="file"
            control={control}
            render={({ field, fieldState }) => {
              const pick = (file: File | null) => {
                field.onChange(file)
                mutation.reset()
              }
              const drop = (event: DragEvent) => {
                event.preventDefault()
                setDragging(false)
                pick(event.dataTransfer.files[0] ?? null)
              }

              return (
                <div>
                  <Box
                    component="label"
                    onDragOver={(event: DragEvent) => {
                      event.preventDefault()
                      setDragging(true)
                    }}
                    onDragLeave={() => setDragging(false)}
                    onDrop={drop}
                    sx={{
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: 'center',
                      gap: 1,
                      px: 3,
                      py: 5,
                      borderRadius: 3,
                      border: '2px dashed',
                      borderColor: fieldState.error ? 'error.main' : dragging ? 'primary.main' : 'divider',
                      bgcolor: dragging ? 'action.selected' : 'action.hover',
                      textAlign: 'center',
                      cursor: 'pointer',
                      transition: 'border-color 120ms, background-color 120ms',
                      '&:hover': { borderColor: 'primary.main' },
                      '&:focus-within': { outline: '2px solid', outlineColor: 'primary.main', outlineOffset: 2 },
                    }}
                  >
                    <input
                      type="file"
                      accept=".json,application/json"
                      style={visuallyHidden}
                      onChange={(event) => {
                        pick(event.target.files?.[0] ?? null)
                        event.target.value = ''
                      }}
                    />
                    <UploadFileRounded sx={{ fontSize: 40, color: 'primary.main' }} />
                    <Typography sx={{ fontWeight: 600 }}>Arrastra tu archivo .json aquí</Typography>
                    <Typography variant="body2" color="text.secondary">
                      o haz clic para buscarlo · máximo 5 MB
                    </Typography>
                  </Box>

                  {field.value && (
                    <Stack
                      direction="row"
                      sx={{ alignItems: 'center', gap: 1.5, mt: 1.5, p: 1.5, borderRadius: 2, border: 1, borderColor: 'divider' }}
                    >
                      <DescriptionOutlined color="action" />
                      <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                        <Typography variant="body2" noWrap sx={{ fontWeight: 600 }}>
                          {field.value.name}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {formatInteger(Math.ceil(field.value.size / 1024))} KB
                        </Typography>
                      </Box>
                      <IconButton size="small" aria-label="Quitar archivo" onClick={() => pick(null)}>
                        <CloseRounded fontSize="small" />
                      </IconButton>
                    </Stack>
                  )}
                  {fieldState.error && <FormHelperText error>{fieldState.error.message}</FormHelperText>}
                </div>
              )
            }}
          />

          <Typography variant="body2" color="text.secondary">
            Las facturas cuyo número ya existe se omiten, así que puedes cargar el mismo archivo varias veces sin duplicar
            datos. Las inconsistentes se guardan, pero quedan fuera del sistema activo.
          </Typography>

          {mutation.isError && <Alert severity="error">{errorMessage(mutation.error)}</Alert>}

          <Box>
            <Button type="submit" variant="contained" size="large" loading={mutation.isPending}>
              Importar
            </Button>
          </Box>
        </Stack>
      </Card>

      {mutation.data && <ImportSummary result={mutation.data} />}
    </Stack>
  )
}

function ImportSummary({ result }: { result: ImportResult }) {
  const stats = [
    { label: 'Registros leídos', value: result.totalRecords },
    { label: 'Importadas', value: result.imported },
    { label: 'Inconsistentes', value: result.importedInconsistent },
    { label: 'Omitidas', value: result.skipped.length },
  ]

  return (
    <Card sx={{ p: { xs: 2.5, sm: 3 } }}>
      <Typography variant="h2">Resultado de la importación</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
        Archivo {result.source}
      </Typography>

      <Grid container spacing={2} sx={{ mt: 2 }}>
        {stats.map((stat) => (
          <Grid key={stat.label} size={{ xs: 6, md: 3 }}>
            <Box sx={{ p: 2, borderRadius: 3, bgcolor: 'action.hover' }}>
              <Typography sx={{ fontSize: '1.75rem', fontWeight: 700, lineHeight: 1.2 }}>{formatInteger(stat.value)}</Typography>
              <Typography variant="body2" color="text.secondary">
                {stat.label}
              </Typography>
            </Box>
          </Grid>
        ))}
      </Grid>

      <Stack sx={{ gap: 3, mt: 3 }}>
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
              <TableRow key={index} sx={{ '&:last-child td': { borderBottom: 0 } }}>
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
