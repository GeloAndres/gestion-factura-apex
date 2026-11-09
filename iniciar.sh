#!/usr/bin/env bash
set -eo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
WEB_PORT="${WEB_PORT:-5174}"
API_PORT="${API_PORT:-5081}"
REFERENCE_API="http://localhost:5080"
API_PROJECT="$ROOT/backend/src/Invoices.Api/Invoices.Api.csproj"
WEB_URL="http://localhost:$WEB_PORT"

export PATH="$HOME/.dotnet:$PATH"

responde() {
  [ "$(curl -s -o /dev/null -m 2 -w '%{http_code}' "$1")" != "000" ]
}

esperar() {
  local url="$1" segundos="$2"
  for _ in $(seq 1 "$segundos"); do
    responde "$url" && return 0
    sleep 1
  done
  return 1
}

api_pid=""
detener() {
  if [ -n "$api_pid" ]; then
    pkill -P "$api_pid" 2>/dev/null || true
    kill "$api_pid" 2>/dev/null || true
  fi
}
trap detener EXIT INT TERM

if responde "$WEB_URL"; then
  echo "La web ya está corriendo en $WEB_URL. Abriéndola..."
  open "$WEB_URL"
  exit 0
fi

if [ -n "${API_URL:-}" ]; then
  echo "Se usa la API indicada en API_URL: $API_URL"
elif [ -f "$API_PROJECT" ]; then
  API_URL="http://localhost:$API_PORT"
  if responde "$API_URL/api/auth/login"; then
    echo "Tu API ya está corriendo en $API_URL."
  else
    echo "Iniciando tu API en $API_URL ..."
    dotnet run --project "$API_PROJECT" --urls "$API_URL" &
    api_pid=$!
  fi
else
  API_URL="$REFERENCE_API"
  echo "Tu API todavía no existe (llega en el paso 8): se usa la API de referencia en $API_URL."
fi

if esperar "$API_URL/api/auth/login" 60; then
  echo "API lista en $API_URL."
else
  echo "Aviso: la API no responde en $API_URL. La web abrirá, pero no podrás iniciar sesión."
fi

cd "$ROOT/frontend"
if [ ! -d node_modules ]; then
  echo "Instalando dependencias de la web..."
  npm ci --no-audit --no-fund
fi

( esperar "$WEB_URL" 30 && echo "Abriendo $WEB_URL (usuario admin / Admin123!)" && open "$WEB_URL" ) &

echo "Iniciando la web en $WEB_URL (Ctrl+C para detener)..."
VITE_API_PROXY_TARGET="$API_URL" npm run dev -- --port "$WEB_PORT"
