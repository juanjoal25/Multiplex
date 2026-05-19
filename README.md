# Multiplex

Sistema de gestión de complejos cinematográficos diseñado bajo **Domain-Driven Design** con arquitectura de **microservicios** en **.NET 10**.

Proyecto académico para **Arquitectura de Software** — Universidad Pontificia Bolivariana (Medellín, 2026).

**Autores:** Hugo Alejandro Hernández Mercado · Juan José Espinosa Alzate · Santiago Viana Ayala
**Docente:** Cesar Augusto López Gallego

---

## Tabla de contenidos

- [Visión general](#visión-general)
- [Bounded Contexts](#bounded-contexts)
- [Arquitectura por microservicio](#arquitectura-por-microservicio)
- [Stack tecnológico](#stack-tecnológico)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Requisitos](#requisitos)
- [Puesta en marcha](#puesta-en-marcha)
- [Endpoints REST](#endpoints-rest)
- [Flujo de humo extremo a extremo](#flujo-de-humo-extremo-a-extremo)
- [Invariantes críticas codificadas](#invariantes-críticas-codificadas)
- [Patrones DDD aplicados](#patrones-ddd-aplicados)
- [Mensajería y eventos](#mensajería-y-eventos)
- [Documentación del diseño](#documentación-del-diseño)

---

## Visión general

Multiplex es una plataforma para operar una cadena de cines: programación de cartelera, gestión de salas y sillas, venta de boletas y confitería, fidelización de clientes, procesamiento de pagos y configuración corporativa.

El sistema está particionado en **6 microservicios**, uno por Bounded Context, comunicados mediante **eventos asíncronos** (RabbitMQ) con **patrón Outbox** transaccional, y vía **HTTP** cuando se requiere consulta sincrónica entre contextos.

Diseñado siguiendo **Spec Driven Development**: cada cláusula del documento de arquitectura ([ArquitecturaDDD-Multiplex.md](ArquitecturaDDD-Multiplex.md)) — agregados, invariantes, Diseño por Contrato, eventos de dominio — está traducida directamente a código verificable.

---

## Bounded Contexts

| Subdominio | Tipo | Microservicio | Responsabilidad |
|---|---|---|---|
| **Ventas** | Core | `Ventas` | Órdenes, boletas, confitería, descuentos |
| **Clientes y Fidelización** | Core | `Clientes` | Espectadores, niveles (Normal/Oro/Platino), descuentos |
| **Programación** | Supporting | `Programacion` | Películas, funciones, cartelera, alquileres |
| **Infraestructura Física** | Supporting | `Infraestructura` | Salas, sillas, estados, reservas |
| **Financiero** | Generic | `Financiero` | Transacciones, pagos, registro contable |
| **Cadena** | Generic | `Cadena` | Sucursales, contratos corporativos, configuración global |

---

## Arquitectura por microservicio

Cada microservicio sigue **arquitectura por capas** con estricta inversión de dependencias:

```
*.Api              ← Controllers REST, OpenAPI, ExceptionFilter, Program.cs
  └─ *.Application   ← Use Cases (MediatR), Abstractions, IntegrationEventHandlers, DTOs
       └─ *.Domain      ← Agregados, Value Objects, Eventos de Dominio, Servicios de Dominio (núcleo puro)
  └─ *.Infrastructure ← EF Core DbContext, Repositorios, MassTransit, Outbox, HTTP Clients, Schedulers
```

Reglas:
- **Domain** no referencia frameworks. Solo C# puro + `Shared.Kernel`.
- **Application** depende de Domain y de `Messaging.Contracts`.
- **Infrastructure** implementa las interfaces definidas en Domain/Application.
- **API** es composition root y enrutamiento; sin lógica de negocio.
- Las referencias **entre BCs** son únicamente por **UUID** (Value Objects `*Ref`), nunca por proyecto.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Runtime | **.NET 10** (C# preview, nullable enabled) |
| API | ASP.NET Core Controllers + Microsoft.AspNetCore.OpenApi 10.0 |
| CQRS in-process | **MediatR 12.4** (Commands, Queries, Notifications) |
| Persistencia | **EF Core 9.0** + **Npgsql** (PostgreSQL) — DB-per-Service |
| Mensajería | **MassTransit 8.3** + **RabbitMQ** + Outbox transaccional (EF Core) |
| HTTP entre BCs | `HttpClient` tipado registrado con `AddHttpClient<T>()` |
| Background jobs | `IHostedService` (`BackgroundService`) |
| Infraestructura local | Docker Compose (Postgres, RabbitMQ, pgAdmin) |

---

## Estructura del repositorio

```
Multiplex/
├── ArquitecturaDDD-Multiplex.md      ← Documento maestro de diseño DDD
├── Multiplex.slnx                    ← Solución (.NET 10)
├── docker-compose.yml                ← Postgres + RabbitMQ + pgAdmin
├── docker/
│   └── postgres-init.sql             ← Crea las 6 BDs (multiplex_<bc>)
└── src/
    ├── Directory.Build.props         ← Settings comunes (LangVersion, Nullable)
    ├── Shared/
    │   ├── Shared.Kernel/            ← Entity, AggregateRoot, ValueObject, Result, Money, IDomainEvent
    │   └── Messaging.Contracts/      ← Eventos de integración (records inmutables) por BC
    ├── Cadena/
    │   ├── Cadena.Domain/            ← Sucursal, ContratoCorporativo, ConfiguracionGlobal
    │   ├── Cadena.Application/       ← ConsultarConfiguracion, RegistrarContrato, …
    │   ├── Cadena.Infrastructure/    ← DbContext, Repos, ContratoVigenciaScheduler
    │   └── Cadena.Api/               ← SucursalController, ContratoController
    ├── Clientes/                     ← idem (Espectador + Suscripcion)
    ├── Financiero/                   ← idem (Transaccion inmutable + StubPasarelaClient)
    ├── Infraestructura/              ← idem (Sala + Silla + ReservaExpirationScheduler)
    ├── Programacion/                 ← idem (Pelicula, Funcion, Cartelera, Alquiler)
    └── Ventas/                       ← idem (Orden, ProductoConfiteria, DefCombo + OrdenExpirationScheduler)
```

---

## Requisitos

### Obligatorios
- **.NET 10 SDK** (versión 10.0 o superior)  
  Descargar desde: https://dotnet.microsoft.com/download/dotnet/10.0
- **Docker Desktop** (con Docker Engine y Docker Compose)  
  Descargar desde: https://www.docker.com/products/docker-desktop
- **Git** (para clonar/actualizar el repositorio)

### Opcionales pero recomendados
- **pgAdmin** (UI para PostgreSQL) — incluido en `docker-compose.yml`
- **RabbitMQ Management UI** (monitoreo de colas) — incluido en `docker-compose.yml`

### Verificar requisitos instalados

```bash
dotnet --version                    # Debe ser 10.x.x
docker --version                    # Debe existir
docker compose version              # Debe existir (no docker-compose)
git --version                        # Debe existir
```

---

## Puesta en marcha (Guía paso a paso)

### Paso 1: Clonar o actualizar el repositorio

```bash
# Si es primera vez:
git clone <repo-url> && cd Multiplex

# Si ya está clonado:
git pull origin main
```

### Paso 2: Verificar estructura del proyecto

```bash
# Desde la raíz del repositorio
ls -la Multiplex.slnx docker-compose.yml docker/postgres-init.sql src/
```

Deberías ver:
- `Multiplex.slnx` — archivo solución
- `docker-compose.yml` — orquestación de servicios
- `docker/postgres-init.sql` — script de inicialización de BDs
- `src/` — directorio con los 6 microservicios

### Paso 3: Levantar infraestructura local (Docker)

En una terminal, desde la **raíz del repositorio**:

```bash
docker compose up -d
```

Esto inicia:
- **PostgreSQL 16** (`multiplex-postgres` en `localhost:5432`)
- **RabbitMQ 3.13** (`multiplex-rabbitmq` en `localhost:5672`)
- **pgAdmin** (`multiplex-pgadmin` en `http://localhost:5050`)

**Credenciales por defecto:**
```
PostgreSQL:
  Usuario: multiplex
  Contraseña: multiplex
  Base datos principal: postgres
  
RabbitMQ:
  Usuario: multiplex
  Contraseña: multiplex
  
pgAdmin:
  Usuario: admin@multiplex.local
  Contraseña: admin
```

**Bases de datos creadas automáticamente:**
- `multiplex_clientes`
- `multiplex_programacion`
- `multiplex_infraestructura`
- `multiplex_ventas`
- `multiplex_financiero`
- `multiplex_cadena`

**Verificar que los servicios estén sanos:**

```bash
docker compose ps
```

Esperar a que los `healthcheck` pasen (status=healthy). Puede tardar 20-30 segundos.

### Paso 4: Restaurar dependencias y compilar

```bash
# Desde la raíz del repositorio
dotnet restore Multiplex.slnx
dotnet build Multiplex.slnx
```

Si todo es correcto, deberías ver `Build succeeded`.

### Paso 5: Ejecutar los 6 microservicios

Necesitas **6 terminales separadas**. Desde la **raíz del repositorio**, en cada una ejecuta:

**Terminal 1 — Clientes**
```bash
dotnet run --project src/Clientes/Clientes.Api
# Esperado: listening on http://localhost:5001
```

**Terminal 2 — Programación**
```bash
dotnet run --project src/Programacion/Programacion.Api
# Esperado: listening on http://localhost:5002
```

**Terminal 3 — Infraestructura**
```bash
dotnet run --project src/Infraestructura/Infraestructura.Api
# Esperado: listening on http://localhost:5003
```

**Terminal 4 — Ventas**
```bash
dotnet run --project src/Ventas/Ventas.Api
# Esperado: listening on http://localhost:5004
```

**Terminal 5 — Financiero**
```bash
dotnet run --project src/Financiero/Financiero.Api
# Esperado: listening on http://localhost:5005
```

**Terminal 6 — Cadena**
```bash
dotnet run --project src/Cadena/Cadena.Api
# Esperado: listening on http://localhost:5006
```

### Paso 6: Verificar que todo funciona

**OpenAPI de cada servicio:**
- Clientes: http://localhost:5001/openapi/v1.json
- Programación: http://localhost:5002/openapi/v1.json
- Infraestructura: http://localhost:5003/openapi/v1.json
- Ventas: http://localhost:5004/openapi/v1.json
- Financiero: http://localhost:5005/openapi/v1.json
- Cadena: http://localhost:5006/openapi/v1.json

**Monitoreo:**
- PostgreSQL (pgAdmin): http://localhost:5050
- RabbitMQ (Management UI): http://localhost:15672

**Test rápido (Curl o Postman):**
```bash
# Registrar un cliente
curl -X POST http://localhost:5001/v1/clientes/registro \
  -H "Content-Type: application/json" \
  -d '{"documento":"12345678","email":"test@example.com","nombre":"Test User"}'

# Consultar cartelera
curl http://localhost:5002/v1/programacion/cartelera

# Consultar configuración
curl http://localhost:5006/v1/cadena/configuracion/00000000-0000-0000-0000-000000000000
```

### Paso 7: Detener los servicios

Para parar todo:

```bash
# Parar los contenedores Docker
docker compose down

# Parar los microservicios: Ctrl+C en cada terminal
```

Para limpiar volúmenes de datos (restaurar estado limpio):

```bash
docker compose down -v
docker compose up -d  # Reiniciar con BDs vacías
```

---

## Comportamiento automático de las APIs

Cada API al arrancar:
- Ejecuta `EnsureCreated()` (crea schema + tablas si no existen)
- Registra `DomainExceptionFilter` (mapea excepciones a HTTP semánticos)
- Expone OpenAPI en `/openapi/v1.json`
- Conecta con RabbitMQ y suscribe a eventos asíncronos
- Inicia `BackgroundService` (schedulers de expiración, vigencia, etc.)

---

## Endpoints REST

### Clientes (`:5001`)
- `POST /v1/clientes/registro`
- `GET  /v1/clientes/{id}/descuento`
- `POST /v1/clientes/{id}/ascender`
- `POST /v1/clientes/{id}/descender`

### Programacion (`:5002`)
- `POST /v1/programacion/peliculas`
- `GET  /v1/programacion/cartelera`
- `GET  /v1/programacion/funcion/{id}`
- `POST /v1/programacion/funciones`
- `DELETE /v1/programacion/funciones/{id}`

### Infraestructura (`:5003`)
- `GET  /v1/infraestructura/salas/{id}`
- `GET  /v1/infraestructura/salas/{id}/disponibilidad`
- `POST /v1/infraestructura/salas/{id}/mantenimiento`
- `POST /v1/infraestructura/salas/{id}/reactivar`
- `PUT  /v1/infraestructura/sillas/{id}/reservar`
- `POST /v1/infraestructura/sillas/{id}/liberar`
- `GET  /v1/infraestructura/sillas/{id}`

### Ventas (`:5004`)
- `POST   /v1/ventas/orden`
- `POST   /v1/ventas/orden/{id}/confirmar`
- `DELETE /v1/ventas/orden/{id}?motivo=...`

### Financiero (`:5005`)
- `POST /v1/financiera/transacciones`
- `GET  /v1/financiera/transacciones/{id}`
- `POST /v1/financiera/transacciones/{id}/revertir`
- `GET  /v1/financiera/historial?inicio=...&fin=...`

### Cadena (`:5006`)
- `GET  /v1/cadena/configuracion/{idSucursal}`
- `PUT  /v1/cadena/configuracion/{idSucursal}`
- `POST /v1/cadena/contratos`
- `DELETE /v1/cadena/contratos/{id}?motivo=...`
- `GET  /v1/cadena/contratos?tercero=...`

### Mapeo de errores

| Excepción del dominio | HTTP |
|---|---|
| `ConflictException` | **409** |
| `PreconditionFailedException` | **400** |
| `InvariantViolationException` | **422** |
| `DomainException` (base) | **400** |

Todas devuelven `ProblemDetails` con `title=code`, `detail=message`, `status`.

---

## Flujo de humo extremo a extremo

Comprar una boleta confirmada:

1. `POST /v1/clientes/registro` → genera evento `EspectadorRegistrado`.
2. `POST /v1/programacion/peliculas` → `PeliculaRegistrada`.
3. *(seed manual de Sala/Silla en `Infraestructura`)*
4. `POST /v1/programacion/funciones` → `FuncionProgramada`.
5. `POST /v1/ventas/orden` → `OrdenCreada` → Infraestructura consume y reserva sillas (`SillaReservada`).
6. `POST /v1/ventas/orden/{id}/confirmar` → `OrdenConfirmada` → Financiero registra transacción → `PagoAprobado`.
7. Verificar en RabbitMQ management UI que todos los eventos fueron publicados y consumidos.

**Prueba de invariante crítica (tasa doble venta = 0%):**
Lanzar dos peticiones concurrentes `PUT /v1/infraestructura/sillas/{id}/reservar` para la misma silla. Una recibe 204; la otra **409 Conflict** con `ReservaRechazada` publicado.

---

## Invariantes críticas codificadas

- **Silla** en estado `RESERVADA` que recibe otra reserva → `ConflictException` (tasa doble-venta = 0%).
- **Función IMAX** solo admite formato IMAX o 3D.
- **Sala VIP** solo admite sillas VIP o ACOMPAÑANTE.
- **Orden** requiere al menos un item; no permite dos `ItemBoleta` con misma `FuncionRef` + `SillaRef`.
- **Transaccion** es inmutable; correcciones → `TransaccionReversion`.
- **Idempotency key** en `Transaccion` por `OrdenId` (índice único en BD).
- **OrdenDepurada** viaja a Financiero sin objetos del dominio cinematográfico (aislamiento BC).
- **Suscripcion CANCELADA** es irreversible; PLATINO no asciende, NORMAL no desciende.

---

## Patrones DDD aplicados

| Patrón | Uso |
|---|---|
| **Strategy** | `INivel` (Clientes), `IFormatoProyeccion` (Programacion) |
| **State** | `IEstadoSuscripcion`, `IEstadoFuncion`, `IEstadoSilla`, `IEstadoSala` |
| **Observer** | `IObserverCartelera` (Programacion) |
| **Aggregate Root** | `Espectador`, `Funcion`, `Pelicula`, `Cartelera`, `Alquiler`, `Sala`, `Orden`, `ProductoConfiteria`, `DefCombo`, `Transaccion`, `Sucursal` |
| **Value Object** | `Money`, `EspectadorId`, `Email`, `Documento`, `RangoHorario`, `Aforo`, `Descuento`, `Expiracion`, `Vigencia`, `OrdenDepurada`, `ParametroGlobal`, `ConceptoFacturable`, … |
| **Domain Service** | `SvcCalculoDescuento`, `SvcValidacionHorario`, `SvcGestionCartelera`, `SvcGestionAforo`, `SvcLiberacionReservas`, `SvcCalculoPrecio`, `SvcCreacionOrden`, `SvcConfirmacionOrden`, `SvcValidacionEventoCorporativo`, `SvcProcesoPago`, `SvcRegistroContable`, `SvcPropagacionConfiguracion`, `SvcGestionContratos` |
| **Diseño por Contrato (DbC)** | Cada método de agregado tiene precondiciones (guards), postcondiciones (mutación + `Raise(evento)`), invariantes (assertions) |

---

## Mensajería y eventos

- **Outbox transaccional** (MassTransit + EF Core): garantiza que un evento de integración solo se publica si la transacción de BD commitea exitosamente.
- **Schedulers** (`BackgroundService`):
  - `ReservaExpirationScheduler` cada 30 s → libera sillas con expiración vencida.
  - `OrdenExpirationScheduler` cada 60 s → expira órdenes PENDIENTES vencidas → libera sillas.
  - `ContratoVigenciaScheduler` cada hora → vence contratos corporativos.

### Flujos de integración entre BCs

| Disparador | Consumidor | Reacción |
|---|---|---|
| `OrdenCreada` | Infraestructura | Reservar sillas |
| `OrdenExpirada` / `OrdenCancelada` | Infraestructura | Liberar sillas |
| `OrdenConfirmada` | Financiero | Registrar transacción |
| `PagoRechazado` | Ventas | Cancelar orden |
| `FuncionCancelada` | Ventas | Cancelar órdenes pendientes con esa función |
| `SalaEnMantenimiento` / `SalaReactivada` | Programación | Marcar disponibilidad |
| `ConfiguracionActualizada` | Todos | Refrescar parámetros globales |

---

## Documentación del diseño

El documento maestro de diseño DDD se encuentra en [ArquitecturaDDD-Multiplex.md](ArquitecturaDDD-Multiplex.md). Incluye:

- Identificación del dominio y subdominios
- Bounded Contexts y lenguaje ubicuo
- Agregados, entidades y objetos de valor por BC
- Eventos de dominio y consumidores
- Reglas de negocio, invariantes y Diseño por Contrato
- Servicios de dominio
- Capas de arquitectura por microservicio

---

## Licencia

Proyecto académico. Sin licencia comercial explícita.
