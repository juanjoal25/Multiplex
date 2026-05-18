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

- **.NET 10 SDK** (preview)
- **Docker Desktop** (Postgres 16 + RabbitMQ 3.13)
- (Opcional) **pgAdmin** está incluido en el `docker-compose` (http://localhost:5050)

---

## Puesta en marcha

### 1. Levantar infraestructura local

```bash
docker compose up -d postgres rabbitmq
```

Esto crea las 6 bases de datos vía `docker/postgres-init.sql`:
- `multiplex_clientes`, `multiplex_programacion`, `multiplex_infraestructura`, `multiplex_ventas`, `multiplex_financiero`, `multiplex_cadena`.

RabbitMQ management UI: http://localhost:15672 (user/pass: `multiplex`/`multiplex`).

### 2. Compilar la solución

```bash
dotnet build Multiplex.slnx
```

### 3. Ejecutar los 6 microservicios

En 6 terminales separadas (o vía `tmux`/`Windows Terminal split panes`):

```bash
dotnet run --project src/Clientes/Clientes.Api               # → http://localhost:5001
dotnet run --project src/Programacion/Programacion.Api       # → http://localhost:5002
dotnet run --project src/Infraestructura/Infraestructura.Api # → http://localhost:5003
dotnet run --project src/Ventas/Ventas.Api                   # → http://localhost:5004
dotnet run --project src/Financiero/Financiero.Api           # → http://localhost:5005
dotnet run --project src/Cadena/Cadena.Api                   # → http://localhost:5006
```

Cada API:
- En desarrollo, ejecuta `EnsureCreated()` al arranque (crea el schema y las tablas).
- Expone OpenAPI en `/openapi/v1.json`.
- Aplica `DomainExceptionFilter` para mapear excepciones del dominio a códigos HTTP semánticos.

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
