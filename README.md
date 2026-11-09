# Gestión de Facturas

Sistema web para gestionar facturas: carga desde JSON, almacenamiento en SQLite, búsqueda, notas de crédito y reportes.

- **Backend:** .NET 8, API REST con Swagger, EF Core + SQLite, autenticación JWT.
- **Frontend:** React 19 + TypeScript (Vite), MUI, TanStack Query, React Hook Form + Zod.

---

## Requisitos

| Herramienta | Versión |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0 |
| [Node.js](https://nodejs.org/) | LTS 22 (22.12 o superior) o LTS 24, con npm 10+ |

No hace falta instalar una base de datos: SQLite se crea sola al iniciar la API.

## Instalación y ejecución

### Con un solo comando (macOS o Linux)

Desde la raíz del repositorio:

```bash
./iniciar.sh
```

Levanta la API (<http://localhost:5080>) y el frontend (<http://localhost:5173>), instala las dependencias del frontend si faltan y abre la aplicación en el navegador. Con `Ctrl+C` se detienen ambos.

- Si la API ya está corriendo en ese puerto, la usa en vez de iniciar otra.
- Los puertos se cambian con las variables `API_PORT` y `WEB_PORT`, por ejemplo `API_PORT=5081 WEB_PORT=5174 ./iniciar.sh`.
- Con `API_URL` se usa una API que ya está corriendo en otra dirección, sin iniciar la propia.

### Paso a paso

**1. API** (terminal 1):

```bash
cd backend
dotnet run --project src/Invoices.Api
```

- Swagger: <http://localhost:5080/swagger>
- Al iniciar, la API aplica las migraciones (tablas y vistas) y carga `data/bd_exam.json`. Si las facturas ya existen, las omite, así que se puede reiniciar sin duplicar datos.
- La base queda en `backend/src/Invoices.Api/App_Data/invoices.db`. Para volver al estado inicial, detén la API y borra esa carpeta.

**2. Frontend** (terminal 2):

```bash
cd frontend
npm install
npm run dev
```

- Aplicación: <http://localhost:5173>. En desarrollo, Vite redirige `/api` a `http://localhost:5080`.
- Para servir el build en otro dominio, define `VITE_API_URL` (ver `frontend/.env.example`) y agrega el origen en `Cors:AllowedOrigins` de la API.

## Credenciales

| Usuario | Contraseña |
|---|---|
| `admin` | `Admin123!` |

El usuario está en `backend/src/Invoices.Api/appsettings.Development.json`, con la contraseña guardada como hash PBKDF2. La clave de firma JWT de ese archivo es solo para desarrollo; en otro ambiente se define con la variable de entorno `Auth__SigningKey`.

En Swagger: ejecuta `POST /api/auth/login`, copia el `accessToken` y pégalo en el botón **Authorize**.

## Recorrido sugerido para revisar

1. Ingresa con `admin` / `Admin123!`.
2. En **Facturas**, filtra por estado de pago **Pendiente**: hoy no hay resultados, porque todos los vencimientos de los datos son de 2024–2025. La pantalla lo explica y ofrece ver los datos al **01-03-2025**.
3. Con esa fecha de referencia, abre la factura **#13** y agrega una nota de crédito: el formulario no deja superar el saldo y, al guardar, el saldo y el estado se recalculan.
4. En **Reportes** revisa las tres vistas. En *Inconsistentes*, despliega la #41 para ver la línea que no cuadra.
5. En **Importar**, sube de nuevo `data/bd_exam.json`: las 50 facturas aparecen como omitidas porque ya existen.

## Tests

Desde la raíz del repositorio, el backend (55 unitarios + 48 de integración):

```bash
cd backend
dotnet test
```

Y el frontend (21 tests de formatos y de la validación del formulario de NC):

```bash
cd frontend
npm test
```

- **Unitarios:** reglas de estado, saldo y notas de crédito del dominio, validación del RUN, reglas de importación y lectura del JSON. Un test fija los hallazgos conocidos del `bd_exam.json` real.
- **Integración:** levantan la API completa en memoria (`WebApplicationFactory`) con una base SQLite temporal, las migraciones, las vistas y la carga real del JSON. "Hoy" se fija en el 01-03-2025 con `FakeTimeProvider`. Cubren búsqueda, detalle, reportes, notas de crédito, importación, autenticación, códigos HTTP y que la vista SQL y el dominio calculen lo mismo.
- **Calidad del frontend:** `npm run lint` (oxlint) y `npm run build` (incluye el chequeo de tipos de TypeScript).

---

## Arquitectura

```
backend/
  src/Invoices.Domain          Entidades y reglas de negocio puras (sin dependencias)
  src/Invoices.Application     Casos de uso (servicios), interfaces de repositorio, reglas de importación, DTOs
  src/Invoices.Infrastructure  EF Core + SQLite: DbContext, configuraciones, migraciones, vistas, repositorios
  src/Invoices.Api             Controllers REST, Swagger, JWT, ProblemDetails, carga inicial
  tests/Invoices.UnitTests
  tests/Invoices.IntegrationTests
frontend/src
  api/        Cliente HTTP tipado y claves de caché
  auth/       Sesión (JWT) y ruta protegida
  features/   Pantallas: facturas, detalle y notas de crédito, reportes, importación, login
  components/ Layout, chips de estado y piezas comunes
database/schema.sql   Script SQL generado desde las migraciones (tablas, índices y vistas)
data/bd_exam.json     Datos de entrada del ejercicio
iniciar.sh            Levanta la API y el frontend con un solo comando
```

Las dependencias van hacia adentro: `Api → Infrastructure → Application → Domain`. El dominio no conoce EF Core ni ASP.NET.

### Patrones de diseño

- **Repository + Unit of Work** (patrón principal). Los repositorios (`IInvoiceRepository`, `ICustomerRepository`, `IInvoiceReadRepository`) encapsulan EF Core, y `IUnitOfWork` confirma los cambios y maneja las transacciones. Los servicios de aplicación no dependen de EF Core, así que se pueden probar y cambiar de motor de base de datos sin tocarlos.
- **Strategy** en la importación. Cada validación es una clase que implementa `IInvoiceImportRule` (consistencia de totales, subtotal por línea, vencimiento, RUN, estados declarados, notas de crédito). El servicio de importación aplica todas las reglas registradas, así que agregar una regla es crear una clase y registrarla.
- **Result.** Los errores de negocio esperados se devuelven como `Result<T>` con un `Error` tipado, no como excepciones. La API traduce el tipo de error a un código HTTP en un solo lugar (`ResultExtensions`).
- Complementan: capa de servicios, inyección de dependencias, Options tipadas y `TimeProvider` para la fecha.

## Reglas de negocio

| Regla | Implementación |
|---|---|
| `invoice_number` único | Índice `UNIQUE` y la importación omite números existentes o repetidos en el archivo. |
| Consistencia | Suma de subtotales = `total_amount`, comparación exacta. Si no cuadra, la factura se guarda completa pero queda fuera del sistema activo (búsqueda, detalle y notas de crédito) y aparece en el reporte de inconsistentes. |
| Estado de factura | Se calcula, no se guarda: **Issued** sin notas de crédito, **Partial** si suman menos que el total, **Cancelled** si suman el total. |
| Estado de pago | Se calcula a la fecha de hoy en Chile: **Paid** si hay pago registrado, **Overdue** si no hay pago y `payment_due_date` ya pasó, **Pending** si no hay pago y está dentro del plazo. El mismo día del vencimiento todavía es Pending. |
| Notas de crédito | Monto entero mayor a 0 que no supere el saldo pendiente (total − notas de crédito). El servidor asigna la fecha (hoy) y un número correlativo. La validación y la inserción ocurren en la misma transacción. |
| Reporte de vencidas | Facturas consistentes, sin pago y sin notas de crédito, con más de 30 días de vencidas. |
| Resumen de pagos | Cantidad y porcentaje de facturas activas por estado de pago. |
| Inconsistentes | Total declarado, suma de productos, diferencia y la línea cuyo subtotal no es precio × cantidad. |

## Base de datos y vistas

Tablas: `Customers`, `Invoices`, `InvoiceLines`, `CreditNotes`, `Payments` y `InvoiceImportIssues` (observaciones de la carga).

Vistas, creadas en la migración `AddReportingViews`:

- `vw_invoice_summary`: facturas activas con cantidad y suma de notas de crédito, saldo pendiente y estado de factura. La búsqueda, el detalle y los reportes de pagos la consultan.
- `vw_inconsistent_invoices`: total declarado vs. suma de subtotales. Alimenta el reporte de inconsistentes.

Los estados no se guardan en tablas, así que al agregar una nota de crédito el estado cambia solo, sin código de recálculo.

El estado de pago depende de "hoy", por eso no está en la vista: lo calcula la consulta (EF Core lo traduce a un `CASE`) con la fecha que entrega `TimeProvider`. Así se puede testear con una fecha fija y consultar a otra fecha de referencia (`asOf`).

Las mismas reglas existen en el dominio (`InvoiceStatusRules` y `PaymentStatusRules`) para validar notas de crédito y la importación. Un test de integración verifica que la vista y el dominio den el mismo resultado para todas las facturas.

Los montos se guardan como `long` (columna `INTEGER`): el peso chileno no tiene decimales, y en SQLite el tipo `decimal` de EF Core se guarda como texto.

`database/schema.sql` se genera desde las migraciones. Para regenerarlo:

```bash
cd backend
dotnet tool restore
dotnet ef migrations script --project src/Invoices.Infrastructure --startup-project src/Invoices.Api --output ../database/schema.sql
```

## Endpoints

Todos, salvo el login, requieren `Authorization: Bearer <token>`. Los errores se devuelven como `application/problem+json` (RFC 7807), con títulos y mensajes en español; los de reglas de negocio traen además un campo `code`. En Swagger, cada endpoint tiene su descripción.

| Método y ruta | Descripción | Respuestas |
|---|---|---|
| `POST /api/auth/login` | Devuelve un token JWT | 200 · 400 · 401 |
| `GET /api/invoices?number=&invoiceStatus=&paymentStatus=&asOf=&page=&pageSize=` | Búsqueda paginada de facturas activas | 200 · 400 · 401 |
| `GET /api/invoices/{number}?asOf=` | Detalle con productos, notas de crédito, pago y observaciones | 200 · 401 · 404 |
| `POST /api/invoices/{number}/credit-notes` | Agrega una nota de crédito `{ "amount": 10000 }` | 201 · 400 · 401 · 404 · 422 |
| `GET /api/reports/overdue-invoices?minDaysOverdue=30&asOf=` | Vencidas por más de N días sin pago ni notas de crédito | 200 · 400 · 401 |
| `GET /api/reports/payment-status-summary?asOf=` | Total y porcentaje por estado de pago | 200 · 401 |
| `GET /api/reports/inconsistent-invoices` | Facturas inconsistentes | 200 · 401 |
| `POST /api/imports` | Carga un JSON (`multipart/form-data`, campo `file`, máx. 5 MB) | 200 · 400 · 401 |

`asOf` (yyyy-MM-dd) es opcional: por defecto se usa la fecha de hoy en Chile.

## Decisiones ante ambigüedades del enunciado

1. **"Utilizar vistas"** se interpretó como vistas SQL de la base de datos, consumidas por la búsqueda y los reportes. Además, el frontend tiene una pantalla por funcionalidad.
2. **Saldo pendiente = total − notas de crédito.** El pago no lo reduce. Los datos lo respaldan: las facturas #2 y #32 están pagadas y tienen notas de crédito posteriores al pago. Por eso una factura pagada todavía acepta notas de crédito hasta su saldo.
3. **"Sin pago o nota de crédito"** (reporte de vencidas) se interpretó como *sin pago y sin ninguna nota de crédito*.
4. **Factura anulada sin pago:** se aplica la regla de pago tal como está. Por ejemplo, la #22 aparece Overdue si su vencimiento ya pasó, y su estado de factura (Cancelled) se muestra al lado.
5. **"Hoy"** es la fecha en `America/Santiago`, no en UTC (se configura en `BusinessCalendar:TimeZone`).
6. **Nombre del archivo:** el enunciado dice `bd_exam.json` y el archivo entregado se llama `bd_exam_invoices.json`. Se incluye como `data/bd_exam.json` y la ruta se configura en `Import:FilePath`. La carga ocurre "en tiempo de ejecución" al iniciar la API, y además se puede subir otro archivo con `POST /api/imports`.
7. **Estados que trae el JSON** (`invoice_status`, `payment_status`): no se usan para calcular. Si contradicen el estado calculado, se guarda una observación.
8. **Vencimiento:** se usa `payment_due_date`, que es el campo que nombra el enunciado. Si `invoice_date + days_to_due` no coincide, se guarda una observación y la factura no se excluye.
9. **Fecha de referencia (`asOf`):** se agregó porque, a la fecha de hoy, ninguna factura de ejemplo está dentro del plazo y el estado Pending no se podría ver.
10. **Factura inconsistente en endpoints activos:** devuelve 404, porque no forma parte del sistema activo.

## Hallazgos en `bd_exam.json`

| Hallazgo | Facturas | Tratamiento |
|---|---|---|
| Suma de subtotales ≠ total | **#41**: la línea "Lamb" trae un subtotal de $254.735 (copiado de la #39), pero 3.919 × 30 = $117.570. **#48**: la diferencia es de solo $6. | Inconsistentes |
| Estado declarado que no corresponde | #30 dice `canceled` y no tiene notas de crédito | Se calcula **Issued** y se guarda una observación |
| `invoice_date + days_to_due ≠ payment_due_date` | #1, 3, 6, 8, 23, 25, 30 | Se usa `payment_due_date` y se guarda una observación |
| RUN inválido | #4 (`9G13WC4RU71`) | Se guarda una observación |
| Pagadas con nota de crédito posterior | #2, #32 | Respaldan la definición de saldo pendiente |

Resultados esperados (sirven para verificar):

| Fecha de referencia | Paid | Pending | Overdue | Vencidas +30 días |
|---|---|---|---|---|
| 01-03-2025 | 13 (27,1 %) | 25 (52,1 %) | 10 (20,8 %) | 3 facturas: #39, #45, #36 |
| 07-11-2026 | 13 (27,1 %) | 0 | 35 (72,9 %) | 30 facturas |

## Mejoras posibles

- Usuarios en la base de datos, roles y refresh tokens.
- `docker-compose` para levantar API y frontend sin instalar .NET ni Node.
- Tests end-to-end del frontend (Playwright).
- Ordenamiento configurable en el listado y exportación de reportes a CSV.
