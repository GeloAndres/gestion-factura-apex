import Avatar from '@mui/material/Avatar'
import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import { alpha } from '@mui/material/styles'
import Typography from '@mui/material/Typography'

const avatarColors = ['#2563eb', '#16a34a', '#d97706', '#db2777', '#7c3aed', '#0891b2', '#ea580c', '#4f46e5']

function initials(name: string): string {
  const [first = '', second = ''] = name.trim().split(/\s+/)
  return (first.charAt(0) + second.charAt(0)).toUpperCase() || '?'
}

function colorFor(name: string): string {
  const hash = [...name].reduce((sum, char) => sum + char.charCodeAt(0), 0)
  return avatarColors[hash % avatarColors.length]
}

export function CustomerCell({ name, run }: { name: string; run: string }) {
  const color = colorFor(name)
  return (
    <Stack direction="row" sx={{ alignItems: 'center', gap: 1.5, minWidth: 0 }}>
      <Avatar
        aria-hidden
        sx={{ width: 32, height: 32, fontSize: '0.75rem', fontWeight: 700, bgcolor: alpha(color, 0.14), color }}
      >
        {initials(name)}
      </Avatar>
      <Box sx={{ minWidth: 0 }}>
        <Typography variant="body2" noWrap sx={{ fontWeight: 500 }}>
          {name}
        </Typography>
        <Typography variant="caption" color="text.secondary" component="div" noWrap>
          RUN {run}
        </Typography>
      </Box>
    </Stack>
  )
}
