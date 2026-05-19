// BFF-owned DTOs that mirror the wire shape of each microservice.
// They are intentionally simple records — no references to domain types.
// Enum values are int-mapped to match the microservices' default JSON
// serialization (System.Text.Json serializes enums as numbers by default).

namespace Multiplex.Web.Models.Dtos;

// ── Clientes ──
public enum TipoDocumento { CC = 0, CE = 1, PAS = 2 }
public sealed record RegistroRequest(string Nombre, string Apellido, string Correo, TipoDocumento TipoDocumento, string NumeroDocumento);
public sealed record RegistrarEspectadorResult(Guid IdEspectador);
public sealed record ConsultarDescuentoResult(decimal Porcentaje, string Nivel, string Estado);
public sealed record AscenderRequest(Guid IdOrdenPago);

// ── Programacion ──
public enum Clasificacion { G = 0, PG = 1, PG13 = 2, R = 3 }
public enum TipoFormato { Formato2D = 0, Formato3D = 1, FormatoIMAX = 2, Formato4DX = 3 }
public sealed record CrearPeliculaRequest(string Titulo, Clasificacion Clasificacion, string Genero, int DuracionMinutos, TipoFormato Formato);
public sealed record ConsultarCarteleraResult(Guid? IdCartelera, IReadOnlyCollection<Guid> Funciones);
public sealed record FuncionDetail(Guid Id, Guid IdSala, string TipoSala, string Formato, decimal PrecioExtraFormato, DateTime Inicio, DateTime Fin);
public sealed record CrearFuncionRequest(Guid IdPelicula, Guid IdSala, DateTime Inicio, DateTime Fin, TipoFormato Formato);

// ── Infraestructura ──
public sealed record SalaDetail(Guid Id, string Nombre, string Tipo, string Estado, int Aforo);
public sealed record SillaDto(Guid IdSilla, string Fila, int Columna, string Tipo, string Estado);
public sealed record DisponibilidadResult(Guid IdSala, int Aforo, int Disponibles, int Ocupacion, IReadOnlyCollection<SillaDto> Sillas);
public sealed record ReservarSillaRequest(Guid IdFuncion, Guid IdOrden, DateTime Expiracion);
public sealed record LiberarSillaRequest(string Motivo);
public sealed record SillaDetail(Guid Id, string Tipo, string Estado);

// ── Ventas ──
public sealed record BoletaDto(Guid IdFuncion, Guid IdSilla);
public sealed record ConfiteriaItemDto(Guid IdProducto, int Cantidad);
public sealed record CrearOrdenRequest(
    Guid IdEspectador,
    IReadOnlyCollection<BoletaDto> Boletas,
    IReadOnlyCollection<ConfiteriaItemDto> Confiterias,
    int MinutosExpiracion,
    bool EsEventoCorporativo,
    string? TerceroCorporativo);
public sealed record CrearOrdenResult(Guid IdOrden, decimal Total, DateTime Expiracion);

// ── Financiero ──
public enum MetodoPago { TarjetaCredito = 0, TarjetaDebito = 1, EfectivoTaquilla = 2, BilleteraDigital = 3 }
public sealed record ConceptoDto(string Descripcion, decimal Valor);
public sealed record RegistrarTransaccionRequest(
    Guid IdOrden,
    IReadOnlyCollection<ConceptoDto> Conceptos,
    IReadOnlyCollection<decimal> Descuentos,
    decimal ValorTotal,
    string Moneda,
    MetodoPago MetodoPago);
public sealed record TransaccionIdResult(Guid Id);
public sealed record TransaccionDetail(Guid Id, Guid IdOrden, decimal ValorTotal, string Moneda, string Estado, string? Referencia);
public sealed record RevertirTransaccionRequest(string Motivo);
public sealed record RegistroHistorialDto(Guid IdTransaccion, Guid IdOrden, decimal ValorTotal, string Moneda, string EstadoPago, DateTime Fecha);

// ── Cadena ──
public enum TipoParametro { String = 0, Entero = 1, Booleano = 2, Decimal = 3 }
public sealed record ParametroDto(string Clave, string Valor, TipoParametro Tipo);
public sealed record ConfiguracionResult(Guid IdSucursal, string ZonaHoraria, string Moneda, IReadOnlyCollection<ParametroDto> Parametros);
public sealed record ActualizarParametroDto(string Clave, string Valor, TipoParametro Tipo);
public sealed record ActualizarConfiguracionRequest(IReadOnlyCollection<ActualizarParametroDto> Parametros);
public sealed record CrearContratoRequest(Guid IdSucursal, string Tercero, DateTime VigenciaInicio, DateTime VigenciaFin, string Condiciones);
public sealed record ContratoIdResult(Guid Id);

// ── BFF-only view models for screens that need data not yet exposed by services ──
// These are stubbed in BFF until microservice endpoints exist (documented as API gaps).
public sealed record PeliculaSummary(
    Guid Id,
    string Titulo,
    string Genero,
    int DuracionMinutos,
    string Clasificacion,
    string Formato,
    decimal Rating,
    string Sinopsis,
    string Estado);

public sealed record ProductoConfiteria(
    Guid Id,
    string Nombre,
    string Categoria,
    string Descripcion,
    decimal Precio);

public sealed record SalaSummary(
    Guid Id,
    string Nombre,
    string Tipo,
    string Estado,
    int Aforo);

public sealed record ContratoSummary(
    Guid Id,
    Guid IdSucursal,
    string Tercero,
    DateTime VigenciaInicio,
    DateTime VigenciaFin,
    string Condiciones,
    string Estado);
