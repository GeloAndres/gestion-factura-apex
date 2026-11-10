# Gestión de Facturas

Aplicación web para gestionar facturas: carga un archivo JSON, lo guarda en SQLite y permite buscar facturas, agregar notas de crédito y ver reportes. El backend es una API REST en .NET 8 y el frontend está hecho en React.

## El patrón detrás del proyecto

El proyecto está armado con **Repository + Unit of Work**.

La idea es que la lógica del negocio no hable directamente con la base de datos. Cuando un servicio necesita una factura, se la pide a un **repositorio** (`IInvoiceRepository`), sin saber si detrás hay SQLite u otro motor. Cuando termina su trabajo, el **Unit of Work** (`IUnitOfWork`) guarda todos los cambios juntos: si algo falla a mitad de camino, no queda nada a medias.

```
React  →  API  →  servicio  →  repositorio  →  SQLite
                     └── Unit of Work: confirma todos los cambios juntos
```
Otros dos patrones ayudan en el camino:

- **Strategy**, en la importación: cada validación del JSON (totales, RUN, fechas, estados) es una clase aparte. Para agregar una nueva, basta con crear otra clase.
- **Result**, para los errores esperados: si algo no se puede hacer, el servicio no lanza una excepción, sino que devuelve el error. La API lo traduce al código HTTP que corresponde (400, 404 o 422).

## Cómo correrlo recién descargado

**1. Instala lo necesario** (una sola vez):

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 22.12 o superior (incluye npm)

No hace falta instalar una base de datos: SQLite se crea sola.

**2. Descarga el proyecto:**

```bash
git clone https://github.com/GeloAndres/gestion-factura-apex.git
cd gestion-factura-apex
```

**3. Levántalo.** En macOS o Linux basta con un comando:

```bash
./iniciar.sh
```

El script inicia la API, instala las dependencias del frontend la primera vez y abre <http://localhost:5173> en el navegador. Con `Ctrl+C` se detiene todo.

En Windows, o si prefieres hacerlo a mano, usa dos terminales. En la primera, la API:

```bash
cd backend
dotnet run --project src/Invoices.Api
```

En la segunda, el frontend:

```bash
cd frontend
npm install
npm run dev
```

Luego abre <http://localhost:5173>.

**4. Ingresa** con este usuario:

| Usuario | Contraseña |
|---|---|
| `admin` | `Admin123!` |

La primera vez, la API crea la base y carga las 50 facturas de `data/bd_exam.json`. La documentación de la API queda en <http://localhost:5080/swagger>.

---

## Más detalles

### Opciones al ejecutar

- `iniciar.sh` usa los puertos 5080 (API) y 5173 (web). Se cambian con `API_PORT` y `WEB_PORT`, y con `API_URL` se usa una API que ya esté corriendo.
- La base queda en `backend/src/Invoices.Api/App_Data/invoices.db`. Para empezar de cero, detén la API y borra esa carpeta. Si solo la reinicias, las facturas no se duplican.
- La contraseña de `admin` está guardada como hash PBKDF2 en `appsettings.Development.json`. La clave JWT de ese archivo es solo para desarrollo; en otro ambiente se define con `Auth__SigningKey`.
- En Swagger, ejecuta `POST /api/auth/login`, copia el `accessToken` y pégalo en **Authorize**.
- Para publicar el frontend en otro dominio, define `VITE_API_URL` (ver `frontend/.env.example`) y agrega ese origen en `Cors:AllowedOrigins` de la API.

### Recorrido sugerido

1. En **Facturas**, filtra por estado de pago **Pendiente**: hoy no hay resultados, porque los datos vencen en 2024 y 2025. La pantalla ofrece verlos al **01-03-2025**.
2. Con esa fecha, abre la factura **#13** y agrega una nota de crédito. El formulario no deja superar el saldo y, al guardar, el saldo y el estado se recalculan.
3. En **Reportes**, revisa las tres vistas. En *Inconsistentes*, despliega la #41 para ver la línea que no cuadra.
4. En **Importar**, sube de nuevo `data/bd_exam.json`: las 50 facturas se omiten porque ya existen.

### Tests

```bash
cd backend
dotnet test
```

```bash
cd frontend
npm test
```

- **Backend (55 unitarias + 48 de integración):** las unitarias prueban las reglas del dominio, el RUN y la importación. Las de integración levantan la API completa con una base SQLite temporal y fijan "hoy" en el 01-03-2025.
- **Frontend (21):** formatos y validación del formulario de notas de crédito. `npm run lint` y `npm run build` revisan el estilo y los tipos.

### Arquitectura

```
backend/src/Invoices.Domain          Entidades y reglas de negocio, sin dependencias
backend/src/Invoices.Application     Servicios, interfaces de repositorio y reglas de importación
backend/src/Invoices.Infrastructure  EF Core + SQLite: repositorios, migraciones y vistas
backend/src/Invoices.Api             Controllers, Swagger, JWT y carga inicial
backend/tests/                       Pruebas unitarias y de integración
frontend/src/                        React: api/, auth/, features/ (pantallas) y components/
database/schema.sql                  Script SQL con tablas, índices y vistas
data/bd_exam.json                    Datos del ejercicio
```

Las dependencias van hacia adentro (`Api → Infrastructure → Application → Domain`), así que el dominio no conoce EF Core ni ASP.NET.

### Reglas de negocio

| Regla | Cómo se cumple |
|---|---|
| `invoice_number` único | Índice `UNIQUE`. La importación omite los números repetidos o que ya existen. |
| Consistencia | Si la suma de subtotales no es igual a `total_amount`, la factura se guarda, pero queda fuera del sistema activo y aparece en el reporte de inconsistentes. |
| Estado de factura | Se calcula: **Issued** sin notas de crédito, **Partial** si suman menos que el total y **Cancelled** si suman el total. |
| Estado de pago | Se calcula con la fecha de hoy en Chile: **Paid** si tiene pago, **Overdue** si venció sin pago y **Pending** si sigue en plazo. El día del vencimiento todavía cuenta como plazo. |
| Notas de crédito | Monto entero mayor a 0 que no supere el saldo (total − notas de crédito). El servidor asigna la fecha y el número dentro de una transacción. |
| Reportes | Vencidas hace más de 30 días sin pago ni notas de crédito, total y porcentaje por estado de pago, e inconsistentes con la línea que no cuadra. |

### Base de datos y vistas

- Tablas: `Customers`, `Invoices`, `InvoiceLines`, `CreditNotes`, `Payments` e `InvoiceImportIssues`.
- `vw_invoice_summary`: facturas activas con sus notas de crédito, saldo y estado. La usan la búsqueda, el detalle y los reportes.
- `vw_inconsistent_invoices`: total declarado contra suma de subtotales, para el reporte de inconsistentes.
- Los estados no se guardan, se calculan al consultar, así que cambian solos al agregar una nota de crédito. El estado de pago depende de "hoy", por eso lo calcula la consulta con la fecha de `TimeProvider` o con `asOf`. Un test verifica que la vista y el dominio den el mismo resultado.
- Los montos son `long`, porque el peso chileno no tiene decimales.
- `database/schema.sql` se genera desde las migraciones:

```bash
cd backend
dotnet tool restore
dotnet ef migrations script --project src/Invoices.Infrastructure --startup-project src/Invoices.Api --output ../database/schema.sql
```

### Endpoints

Todos, salvo el login, piden `Authorization: Bearer <token>`. Los errores llegan como `application/problem+json`, en español. `asOf` (yyyy-MM-dd) es opcional y por defecto es la fecha de hoy en Chile.

| Método y ruta | Qué hace | Respuestas |
|---|---|---|
| `POST /api/auth/login` | Entrega un token JWT | 200 · 400 · 401 |
| `GET /api/invoices?number=&invoiceStatus=&paymentStatus=&asOf=&page=&pageSize=` | Busca facturas activas, con paginación | 200 · 400 · 401 |
| `GET /api/invoices/{number}?asOf=` | Detalle con productos, notas de crédito, pago y observaciones | 200 · 401 · 404 |
| `POST /api/invoices/{number}/credit-notes` | Agrega una nota de crédito `{ "amount": 10000 }` | 201 · 400 · 401 · 404 · 422 |
| `GET /api/reports/overdue-invoices?minDaysOverdue=30&asOf=` | Vencidas hace más de N días sin pago ni notas de crédito | 200 · 400 · 401 |
| `GET /api/reports/payment-status-summary?asOf=` | Total y porcentaje por estado de pago | 200 · 401 |
| `GET /api/reports/inconsistent-invoices` | Facturas inconsistentes | 200 · 401 |
| `POST /api/imports` | Carga un JSON (`multipart/form-data`, campo `file`, máximo 5 MB) | 200 · 400 · 401 |

### Decisiones ante ambigüedades del enunciado

1. **"Utilizar vistas"** se tomó como vistas SQL en la base, usadas por la búsqueda y los reportes.
2. **Saldo pendiente = total − notas de crédito.** El pago no lo reduce: las facturas #2 y #32 están pagadas y tienen notas de crédito posteriores.
3. **"Sin pago o nota de crédito"**, en el reporte de vencidas, se tomó como *sin pago y sin ninguna nota de crédito*.
4. **Factura anulada sin pago:** la regla de pago se aplica igual, así que la #22 puede salir Overdue y Cancelled a la vez.
5. **"Hoy"** es la fecha en `America/Santiago`, no en UTC.
6. **Nombre del archivo:** el enunciado dice `bd_exam.json` y el entregado se llama `bd_exam_invoices.json`. Se incluye como `data/bd_exam.json` y se carga al iniciar la API; también se puede subir otro desde **Importar**.
7. **Datos del JSON:** los estados declarados no se usan para calcular, y el vencimiento se toma de `payment_due_date`. Si algo no cuadra, se guarda una observación.
8. **Fecha de referencia (`asOf`):** se agregó porque hoy ninguna factura de ejemplo sigue en plazo, y sin ella no se vería el estado Pending.
9. **Factura inconsistente:** la búsqueda no la muestra, y su detalle y sus notas de crédito responden 404.

### Hallazgos en `bd_exam.json`

| Hallazgo | Facturas | Tratamiento |
|---|---|---|
| Suma de subtotales ≠ total | #41 (la línea "Lamb" trae el subtotal de la #39) y #48 (diferencia de $6) | Inconsistentes |
| Estado declarado incorrecto | #30 dice `canceled`, pero no tiene notas de crédito | Se calcula **Issued** |
| `invoice_date + days_to_due ≠ payment_due_date` | #1, 3, 6, 8, 23, 25, 30 | Se usa `payment_due_date` |
| RUN inválido | #4 | Se guarda una observación |
| Pagadas con nota de crédito posterior | #2, #32 | Respaldan la definición de saldo |

Para verificar: al **01-03-2025** hay 13 Paid (27,1 %), 25 Pending (52,1 %) y 10 Overdue (20,8 %), y 3 facturas vencidas hace más de 30 días (#39, #45 y #36).

### Mejoras posibles

Usuarios y roles en la base de datos, `docker-compose` para no instalar .NET ni Node, pruebas end-to-end del frontend y exportación de reportes a CSV.
