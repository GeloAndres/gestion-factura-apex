import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Tab from '@mui/material/Tab'
import Tabs from '@mui/material/Tabs'
import { useSearchParams } from 'react-router'
import { AsOfNotice } from '../../components/AsOfNotice'
import { PageHeader } from '../../components/PageHeader'
import { InconsistentReport } from './InconsistentReport'
import { OverdueReport } from './OverdueReport'
import { PaymentSummaryReport } from './PaymentSummaryReport'

const views = [
  { value: 'vencidas', label: 'Vencidas +30 días', content: <OverdueReport /> },
  { value: 'resumen', label: 'Resumen por estado de pago', content: <PaymentSummaryReport /> },
  { value: 'inconsistentes', label: 'Inconsistentes', content: <InconsistentReport /> },
] as const

export function ReportsPage() {
  const [params, setParams] = useSearchParams()
  const current = views.find((view) => view.value === params.get('vista')) ?? views[0]

  return (
    <Stack sx={{ gap: 3 }}>
      <PageHeader title="Reportes" subtitle="Seguimiento de cobranza y calidad de los datos importados." />
      <AsOfNotice />
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs
          value={current.value}
          onChange={(_, value: string) => setParams({ vista: value }, { replace: true })}
          variant="scrollable"
          allowScrollButtonsMobile
          aria-label="Reportes"
        >
          {views.map((view) => (
            <Tab key={view.value} value={view.value} label={view.label} id={`tab-${view.value}`} aria-controls={`panel-${view.value}`} />
          ))}
        </Tabs>
      </Box>
      <div role="tabpanel" id={`panel-${current.value}`} aria-labelledby={`tab-${current.value}`}>
        {current.content}
      </div>
    </Stack>
  )
}
