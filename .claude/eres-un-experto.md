# Plan de Ejecución — Multiplex (Spec Driven Development, .NET 10)

## Contexto

El proyecto **Multiplex** es un sistema de gestión de complejos cinematográficos diseñado bajo **Domain-Driven Design** con **6 microservicios** (uno por Bounded Context): `Clientes`, `Programacion`, `Infraestructura`, `Ventas`, `Financiero`, `Cadena`. El documento [ArquitecturaDDD-Multiplex.md](ArquitecturaDDD-Multiplex.md) define exhaustivamente: subdominios, lenguaje ubicuo, agregados/entidades/VOs, eventos de dominio, reglas de negocio, invariantes, Diseño por Contrato (DbC) y servicios de dominio.

El **esqueleto de la solución ya está creado** ([Multiplex.slnx](Multiplex.slnx)) con 4 capas por microservicio (`*.Api`, `*.Application`, `*.Domain`, `*.Infrastructure`) más dos proyectos compartidos (`Shared.Kernel`, `Messaging.Contracts`). Todos los proyectos contienen únicamente `Class1.cs` (template). Falta materializar el modelo de dominio, los casos de uso, los repositorios, la API REST, la mensajería asíncrona y la persistencia.

**Decisiones tomadas** (de la sesión con el usuario):
- **Estrategia de iteración**: Walking skeleton transversal — completar **capa de Dominio en los 6 BCs primero**, luego subir.
- **Persistencia**: EF Core 10 + PostgreSQL en Docker (DB-per-Service).
- **Mensajería**: MassTransit + RabbitMQ en Docker (contratos en `Messaging.Contracts`).
- **API**: ASP.NET Core Controllers (siguiendo literalmente el documento).

**Resultado esperado**: una solución ejecutable con `docker compose up` que arranque los 6 microservicios, Postgres y RabbitMQ, exponga los endpoints REST descritos en el documento y propague eventos de dominio entre BCs.

---

## Principios transversales (aplican a todos los BCs)

- **Aislamiento de Dominio**: `*.Domain` no referencia frameworks (sin EF, sin ASP.NET, sin MassTransit). Solo C# puro + `Shared.Kernel`.
- **Dependencias permitidas**:
  - `Domain` → `Shared.Kernel`
  - `Application` → `Domain` + `Messaging.Contracts`
  - `Infrastructure` → `Application` + EF Core + MassTransit + HTTP clients
  - `Api` → `Application` + `Infrastructure` (solo composition root)
- **Referencias cruzadas entre BCs**: SOLO vía `*Ref` (UUID) en VOs. **Nunca** se referencia un proyecto Domain de otro BC.
- **Eventos de integración** viven en `Messaging.Contracts` (DTOs serializables). Los **eventos de dominio** viven en cada `*.Domain/Events`.
- **Outbox Pattern**: cada microservicio publica eventos vía outbox transaccional (MassTransit + EF Core) para garantizar consistencia eventual.

---

## Fase 0 — Cimientos compartidos (bloqueante para todo lo demás)

### `Shared.Kernel`
Bloques base para todos los Dominios:
- `Entity<TId>` — base con `Id`, igualdad por identidad.
- `AggregateRoot<TId>` — extiende Entity, expone `IReadOnlyCollection<IDomainEvent> DomainEvents` + `Raise(IDomainEvent)` + `ClearEvents()`.
- `ValueObject` — base con igualdad estructural (sobrescribe `Equals`/`GetHashCode` con `GetEqualityComponents()`).
- `IDomainEvent` — marker + `Guid EventId`, `DateTime OccurredOn`.
- `Result` / `Result<T>` — manejo funcional de errores de invariantes.
- `IRepository<TAggregate, TId>` — contrato base.
- Excepciones de dominio: `DomainException`, `InvariantViolationException`.
- `Money` (VO global con moneda COP) — usado en Ventas y Financiero.

### `Messaging.Contracts`
Records inmutables con todos los eventos de integración listados en el documento, agrupados por BC emisor:
- `Clientes/`: `EspectadorRegistrado`, `MembresiaActivada`, `NivelAscendido`, `NivelDescendido`, `MembresiaCancelada`.
- `Programacion/`: `PeliculaRegistrada`, `FuncionProgramada`, `FuncionIniciada`, `FuncionFinalizada`, `FuncionCancelada`, `CarteleraActualizada`.
- `Infraestructura/`: `SillaReservada`, `ReservaRechazada`, `SillaOcupada`, `SillaLiberada`, `ReservaExpirada`, `SalaEnMantenimiento`, `SalaReactivada`.
- `Ventas/`: `OrdenCreada`, `OrdenConfirmada`, `OrdenExpirada`, `OrdenCancelada`, `StockAgotado`.
- `Financiero/`: `TransaccionRegistrada`, `PagoAprobado`, `PagoRechazado`, `TransaccionRevertida`.
- `Cadena/`: `ConfiguracionActualizada`, `ContratoCorporativoRegistrado`, `ContratoCorporativoVencido`, `ContratoCorporativoCancelado`.

### Infraestructura local
- `docker-compose.yml` en raíz con servicios: `postgres` (puerto 5432, una BD por microservicio creada por script init), `rabbitmq` (15672 management, 5672 AMQP), `pgadmin` (opcional).
- `Directory.Build.props` en `src/` para fijar `LangVersion=preview`, `TreatWarningsAsErrors=true`, `Nullable=enable`.

---

## Fase 1 — Capa de Dominio (los 6 BCs en paralelo)

Esta fase **materializa todo el documento DDD**. Para cada BC se crean:

### Estructura de carpetas estándar en `*.Domain/`
```
Aggregates/<NombreAgregado>/
  <Agregado>.cs                  ← AggregateRoot, métodos = casos del DbC
  <Entidades anidadas>.cs
Events/                          ← eventos de dominio (puros, no integración)
ValueObjects/                    ← VOs con invariantes en constructor
Services/                        ← Servicios de Dominio (Svc*)
Repositories/                    ← interfaces I*Repository
Exceptions/                      ← excepciones específicas del BC
Strategies/                      ← interfaces de patrones Strategy
States/                          ← interfaces de patrones State
```

### Mapeo BC → contenido (resumido del documento)

| BC | Agregados | Patrones | Servicios de Dominio |
|---|---|---|---|
| **Clientes** | `Espectador` (con `Suscripcion`) | Strategy `INivel`, State `IEstadoSuscripcion` | `SvcCalculoDescuento`, `SvcReactivarSuscripcion` |
| **Programacion** | `Funcion`, `Pelicula`, `Cartelera`, `Alquiler` | Strategy `IFormatoProyeccion`, State `IEstadoFuncion`, Observer `IObserverCartelera` | `SvcValidacionHorario`, `SvcGestionCartelera` |
| **Infraestructura** | `Sala` (con `Silla`) | State para Silla (DISPONIBLE→RESERVADA→OCUPADA), State para Sala | `SvcGestionAforo`, `SvcLiberacionReservas`, `SvcCambioEstadoSala` |
| **Ventas** | `Orden`, `ProductoConfiteria`, `DefCombo` | State `EstadoOrden` | `SvcCalculoPrecio`, `SvcCreacionOrden`, `SvcConfirmacionOrden`, `SvcValidacionEventoCorporativo` |
| **Financiero** | `Transaccion` (con `TransaccionReversion`, inmutable) | — | `SvcProcesoPago`, `SvcRegistroContable` |
| **Cadena** | `Sucursal` (con `ContratoCorporativo`, `ConfiguracionGlobal`) | — | `SvcPropagacionConfiguracion`, `SvcGestionContratos` |

### Reglas de implementación

1. **Invariantes en constructores y métodos** — cada método del documento "Diseño por Contrato" se traduce literal: `precondiciones` → guards al inicio (lanzando `DomainException`); `postcondiciones` → estado post + `Raise(evento)`; `invariantes` → assertions reforzadas en factory + cada transición.
2. **VOs cerrados** — todos los Value Objects son `sealed record` con `private` constructor y factory `Create(...)` que valida.
3. **Enums cerrados** — `Clasificacion`, `TipoSala`, `TipoSilla`, `EstadoOrden`, `EstadoContrato`, `EstadoPago`, `PropositoAlquiler` como `enum`.
4. **Strategies/States** como interfaces + clases concretas; el agregado **nunca** evalúa el tipo concreto (LSP).
5. **Eventos de dominio**: cada método de agregado que muta estado debe `Raise(...)` el evento listado en la sección "EVENTOS DE DOMINIO".

### Pruebas unitarias por BC (proyecto `*.Domain.Tests` con xUnit + FluentAssertions)
Una prueba por cada cláusula del DbC:
- Una prueba por **precondición** (caso negativo → excepción).
- Una prueba por **postcondición** (caso positivo → estado + evento generado).
- Una prueba por **invariante** (transición ilegal rechazada).

Esto es el corazón del **Spec Driven Development**: el documento DbC es el spec; cada cláusula = una prueba.

**Hito de Fase 1**: `dotnet test` verde en los 6 proyectos `*.Domain.Tests`.

---

## Fase 2 — Capa de Aplicación (casos de uso por BC)

Para cada BC, en `*.Application/`:

```
UseCases/<NombreCasoDeUso>/
  <CasoDeUso>Command.cs   ← record DTO de entrada
  <CasoDeUso>Handler.cs   ← orquesta agregado + repos + svc dominio
  <CasoDeUso>Result.cs    ← DTO de salida
Abstractions/
  I<BC>Client.cs          ← contratos HTTP a otros BCs
  IEventPublisher.cs
IntegrationEventHandlers/ ← reaccionan a eventos de otros BCs
DTOs/                     ← Read models para queries
```

Casos de uso por BC (extraídos del documento, sección "Capa de Aplicación"):
- **Clientes**: `RegistrarEspectador`, `ConsultarDescuento`, `AscenderNivel`, `DescenderNivel`.
- **Programacion**: `ProgramarFuncion`, `CancelarFuncion`, `ConsultarCartelera`, `RegistrarPelicula`.
- **Infraestructura**: `ReservarSilla`, `LiberarSilla`, `ConsultarDisponibilidad`, `EnviarMantenimiento`, `ReactivarSala`.
- **Ventas**: `CrearOrden`, `ConfirmarOrden`, `CancelarOrden`, `ConsultarProductos`.
- **Financiero**: `RegistrarTransaccion`, `RevertirTransaccion`, `ConsultarHistorial`.
- **Cadena**: `ConsultarConfiguracion`, `ActualizarConfiguracion`, `RegistrarContrato`, `CancelarContrato`.

**Integraciones críticas** (handlers de eventos entre BCs):
- `Ventas` consume `EspectadorRegistrado`, `NivelAscendido` (para descuentos), `SillaReservada/Rechazada` (para confirmar/rechazar orden), `PagoAprobado/Rechazado`, `FuncionCancelada`.
- `Infraestructura` consume `OrdenCreada` (reservar sillas), `OrdenExpirada/Cancelada` (liberar), `FuncionIniciada/Finalizada/Cancelada`.
- `Financiero` consume `OrdenConfirmada` (registrar transacción).
- `Programacion` consume `SalaEnMantenimiento/Reactivada`.

**MediatR** para dispatch de comandos/queries dentro del proceso.

---

## Fase 3 — Capa de Infraestructura

Por BC, en `*.Infrastructure/`:

```
Persistence/
  <BC>DbContext.cs              ← DbSet por agregado
  Configurations/               ← IEntityTypeConfiguration por agregado
  Repositories/                 ← implementación de I*Repository
  Migrations/                   ← migraciones EF
Messaging/
  Consumers/                    ← MassTransit consumers para integration events
  Publishers/                   ← IEventPublisher concreto (outbox + MassTransit)
HttpClients/                    ← implementaciones de I<BC>Client (Refit o HttpClient tipado)
Schedulers/                     ← BackgroundService para LiberacionReservas, OrdenExpiration, ContratoVigencia
DependencyInjection.cs          ← AddInfrastructure()
```

- **Persistencia**: una BD Postgres por servicio (`multiplex_clientes`, `multiplex_ventas`, etc.). Configuración EF Core con `OwnsOne`/`OwnsMany` para Value Objects y entidades anidadas.
- **Mappings de Strategy/State**: persistir un discriminador `string` (e.g. `"NORMAL"`, `"ORO"`, `"PLATINO"`) y reconstruir la instancia concreta en el repositorio.
- **Outbox**: `MassTransit.EntityFrameworkCoreIntegration` con tabla `OutboxMessages` por BD.
- **Schedulers** (`IHostedService`):
  - Infraestructura: `ReservaExpirationScheduler` (cada 30s).
  - Ventas: `OrdenExpirationScheduler` (cada 60s).
  - Cadena: `ContratoVigenciaScheduler` (cada hora).

---

## Fase 4 — Capa API (Controllers)

Por BC, en `*.Api/`:

```
Controllers/                    ← un controller por agregado/recurso
Filters/                        ← ExceptionFilter → DomainException a 4xx
Middleware/                     ← Correlation-Id, logging
Program.cs                      ← AddApplication + AddInfrastructure + MapControllers
appsettings.json                ← ConnectionString, RabbitMq, urls de otros BCs
```

Endpoints (literal del documento):
- **Clientes**: `POST /v1/clientes/registro`, `GET /v1/clientes/{id}/descuento`, `POST /v1/clientes/{id}/ascender`, `POST /v1/clientes/{id}/descender`.
- **Programacion**: `GET /v1/programacion/cartelera`, `GET /v1/programacion/funcion/{id}`, `POST /v1/programacion/funciones`, `DELETE /v1/programacion/funciones/{id}`, `POST /v1/programacion/peliculas`.
- **Infraestructura**: `GET /v1/infraestructura/salas/{id}/disponibilidad`, `PUT /v1/infraestructura/sillas/{id}/reservar`, `POST /v1/infraestructura/salas/{id}/mantenimiento`, `POST /v1/infraestructura/salas/{id}/reactivar`.
- **Ventas**: `POST /v1/ventas/orden`, `POST /v1/ventas/orden/{id}/confirmar`, `DELETE /v1/ventas/orden/{id}`, `GET /v1/ventas/productos`, `GET /v1/ventas/combos`.
- **Financiero**: `POST /v1/financiera/transacciones`, `GET /v1/financiera/transacciones/{id}`, `GET /v1/financiera/historial`.
- **Cadena**: `GET /v1/cadena/configuracion/{idSucursal}`, `POST /v1/cadena/contratos`, `DELETE /v1/cadena/contratos/{id}`.

OpenAPI (Swashbuckle o Microsoft.AspNetCore.OpenApi ya incluido) habilitado en todos los servicios.

---

## Archivos críticos a crear/modificar

**Cimientos**:
- `src/Shared/Shared.Kernel/` → `Entity.cs`, `AggregateRoot.cs`, `ValueObject.cs`, `IDomainEvent.cs`, `Result.cs`, `Money.cs`, `IRepository.cs`, `Exceptions/DomainException.cs`.
- `src/Shared/Messaging.Contracts/` → un archivo por evento de integración (~30 archivos).
- Raíz: `docker-compose.yml`, `Directory.Build.props`.

**Por cada uno de los 6 BCs** (ejemplo Clientes):
- `src/Clientes/Clientes.Domain/Aggregates/Espectador/Espectador.cs`, `Suscripcion.cs`.
- `src/Clientes/Clientes.Domain/ValueObjects/` (7 archivos).
- `src/Clientes/Clientes.Domain/Strategies/INivel.cs` + `NivelNormal.cs`, `NivelOro.cs`, `NivelPlatino.cs`.
- `src/Clientes/Clientes.Domain/States/IEstadoSuscripcion.cs` + 3 implementaciones.
- `src/Clientes/Clientes.Domain/Events/` (5 archivos).
- `src/Clientes/Clientes.Domain/Services/SvcCalculoDescuento.cs`.
- `src/Clientes/Clientes.Domain/Repositories/IEspectadorRepository.cs`.
- `src/Clientes/Clientes.Application/UseCases/RegistrarEspectador/...` (4 casos de uso × 3 archivos).
- `src/Clientes/Clientes.Infrastructure/Persistence/ClientesDbContext.cs` + configurations + repos.
- `src/Clientes/Clientes.Api/Controllers/ClientesController.cs`, `SuscripcionController.cs`, `Program.cs`.
- `src/Clientes/Clientes.Domain.Tests/` (proyecto nuevo) — pruebas DbC.

Patrón análogo para los otros 5 BCs.

**Referencias de proyecto a añadir** en `.slnx`:
- 6 nuevos proyectos `*.Domain.Tests`.

---

## Especificaciones derivadas del documento (Spec Driven)

Cada elemento del documento se traduce a artefactos de código verificables:

| Elemento del documento | Artefacto de código | Verificación |
|---|---|---|
| **Lenguaje Ubicuo** | Nombres de clases/métodos | Code review |
| **Agregados / Entidades / VOs** | `AggregateRoot`, `Entity`, `record sealed` | Compilación + tests |
| **Eventos de Dominio** | Clases en `Events/` + records en `Messaging.Contracts` | Test `event raised on operation` |
| **Reglas de Negocio / Invariantes** | Guards + constructor validations | Test por cada regla |
| **Diseño por Contrato** | Métodos de agregado/servicio | Test por cada precondición/postcondición |
| **Servicios de Dominio** | Clases `Svc*` en `Services/` | Tests unitarios |
| **Capas de Arquitectura** | Estructura de proyectos | `dotnet build` |
| **Endpoints REST** | Controllers + acciones | Tests de integración o llamadas manuales |

---

## Verificación end-to-end

Al completar las 4 fases:

1. **Compilación**: `dotnet build Multiplex.slnx` sin errores ni warnings (`TreatWarningsAsErrors`).
2. **Tests de Dominio**: `dotnet test` verde en los 6 proyectos `*.Domain.Tests` (cobertura ≥ cada cláusula DbC del documento).
3. **Levantar infraestructura**: `docker compose up -d postgres rabbitmq`.
4. **Migraciones**: `dotnet ef database update --project <BC>.Infrastructure` para los 6 BCs.
5. **Arranque de microservicios**: ejecutar los 6 `*.Api` (perfiles de launchSettings) y verificar Swagger en cada uno.
6. **Flujo de humo manual** (escenario `Comprar boleta`):
   1. `POST /v1/clientes/registro` → genera `EspectadorRegistrado`.
   2. `POST /v1/programacion/peliculas` → `PeliculaRegistrada`.
   3. `POST /v1/programacion/funciones` → `FuncionProgramada`.
   4. `POST /v1/ventas/orden` → reserva silla (`SillaReservada`) y crea `OrdenCreada`.
   5. `POST /v1/ventas/orden/{id}/confirmar` → `OrdenConfirmada` → `TransaccionRegistrada` → `PagoAprobado`.
   6. Verificar en RabbitMQ management UI que todos los eventos fueron publicados y consumidos.
7. **Verificar invariante crítica de Infraestructura**: lanzar dos peticiones concurrentes de reserva sobre la misma silla; solo una debe quedar `RESERVADA`, la otra debe recibir `ReservaRechazada`. **Tasa de doble venta = 0%**.

---

## Orden de ejecución sugerido

1. **Fase 0** completa (Shared.Kernel + Messaging.Contracts + docker-compose).
2. **Fase 1** en paralelo (los 6 dominios + sus tests). Tiempo estimado: 60-70% del esfuerzo total — aquí vive todo el valor DDD.
3. **Fase 2** por BC, comenzando por **Clientes** (no tiene dependencias salientes), luego **Programacion** e **Infraestructura** (independientes), luego **Cadena** (independiente), luego **Ventas** (consumidor de todos), por último **Financiero**.
4. **Fase 3** y **Fase 4** por BC en el mismo orden, para tener servicios funcionales incrementalmente.

El usuario tendrá un sistema demostrable extremo-a-extremo al finalizar las 4 fases.
