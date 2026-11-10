import Stack from '@mui/material/Stack'
import Tab from '@mui/material/Tab'
import Tabs from '@mui/material/Tabs'
import { useSearchParams } from 'react-router'
import { PageHeader } from '../../components/PageHeader'
import { InconsistentReport } from './InconsistentReport'
import { OverdueReport } from './OverdueReport'
import { PaymentSummaryReport } from './PaymentSummaryReport'

const views = [
  { value: 'resumen', label: 'Resumen de pagos', content: <PaymentSummaryReport /> },
  { value: 'vencidas', label: 'Vencidas +30 días', content: <OverdueReport /> },
  { value: 'inconsistentes', label: 'Inconsistentes', content: <InconsistentReport /> },
] as const

export function ReportsPage() {
  const [params, setParams] = useSearchParams()
  const current = views.find((view) => view.value === params.get('vista')) ?? views[0]

  return (
    <Stack sx={{ gap: 3 }}>
      <PageHeader title="Reportes" subtitle="Cobranza y calidad de los datos importados." />
      <Tabs
        value={current.value}
        onChange={(_, value: string) => setParams({ vista: value }, { replace: true })}
        variant="scrollable"
        allowScrollButtonsMobile
        aria-label="Reportes"
        sx={{
          minHeight: 0,
          '& .MuiTabs-indicator': { display: 'none' },
          '& .MuiTabs-list': { gap: 1 },
        }}
      >
        {views.map((view) => (
          <Tab
            key={view.value}
            value={view.value}
            label={view.label}
            id={`tab-${view.value}`}
            aria-controls={`panel-${view.value}`}
            sx={{
              minHeight: 0,
              px: 2,
              py: 1,
              borderRadius: 2,
              border: 1,
              borderColor: 'transparent',
              color: 'text.secondary',
              bgcolor: 'action.hover',
              '&.Mui-selected': { color: 'text.primary', bgcolor: 'background.paper', borderColor: 'divider' },
            }}
          />
        ))}
      </Tabs>
      <div role="tabpanel" id={`panel-${current.value}`} aria-labelledby={`tab-${current.value}`}>
        {current.content}
      </div>
    </Stack>
  )
}
