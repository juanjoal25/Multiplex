using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Auth;
using Multiplex.Web.Models;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Controllers;

[Route("reserva")]
public sealed class ReservaController(
    IProgramacionClient programacion,
    IInfraestructuraClient infra,
    IVentasClient ventas,
    IFinancieroClient financiero,
    IClientesClient clientes,
    ILogger<ReservaController> log) : Controller
{
    private const decimal PrecioBoletaBase = 20_000m;

    [HttpGet("funcion/{idFuncion:guid}/sillas")]
    public async Task<IActionResult> Sillas(Guid idFuncion, CancellationToken ct)
    {
        ViewData["NavActive"] = "cartelera";
        var funcion = await programacion.GetFuncionAsync(idFuncion, ct);
        if (funcion is null) return NotFound();

        var disp = await infra.ConsultarDisponibilidadAsync(funcion.IdSala, ct);
        if (disp is null) return NotFound("Sala no disponible.");

        var precio = PrecioBoletaBase + funcion.PrecioExtraFormato;
        return View(new SillasVm(funcion, disp, precio));
    }

    [HttpPost("sillas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ElegirSillas(Guid idFuncion, Guid[] sillas, CancellationToken ct)
    {
        if (sillas is null || sillas.Length == 0)
        {
            TempData["Flash"] = "Selecciona al menos una silla.";
            return RedirectToAction(nameof(Sillas), new { idFuncion });
        }

        var funcion = await programacion.GetFuncionAsync(idFuncion, ct);
        if (funcion is null) return NotFound();
        var disp = await infra.ConsultarDisponibilidadAsync(funcion.IdSala, ct);
        if (disp is null) return NotFound();

        var precio = PrecioBoletaBase + funcion.PrecioExtraFormato;
        var cart = HttpContext.Session.GetCart();
        cart.IdFuncion = idFuncion;
        cart.SalaNombre = $"Sala {funcion.IdSala.ToString()[..6]}";
        cart.Horario = funcion.Inicio;
        cart.Formato = funcion.Formato;
        cart.Sillas = sillas
            .Distinct()
            .Select(id => disp.Sillas.FirstOrDefault(s => s.IdSilla == id))
            .Where(s => s is not null)
            .Select(s => new CartItemSilla
            {
                IdFuncion = idFuncion,
                IdSilla = s!.IdSilla,
                Label = $"{s.Fila}{s.Columna}",
                TipoSilla = s.Tipo,
                PrecioBase = precio
            }).ToList();
        HttpContext.Session.SaveCart(cart);
        return RedirectToAction(nameof(Carrito));
    }

    [HttpGet("carrito")]
    public IActionResult Carrito()
    {
        ViewData["NavActive"] = "cartelera";
        var cart = HttpContext.Session.GetCart();
        return View(cart);
    }

    [HttpPost("carrito/vaciar")]
    [ValidateAntiForgeryToken]
    public IActionResult Vaciar()
    {
        HttpContext.Session.ClearCart();
        TempData["Flash"] = "Carrito vacío.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("confiteria")]
    public IActionResult Confiteria()
    {
        ViewData["NavActive"] = "cartelera";
        var cart = HttpContext.Session.GetCart();
        var productos = ventas.ListarConfiteriaStub();
        return View(new ConfiteriaVm(productos, cart));
    }

    [HttpPost("confiteria")]
    [ValidateAntiForgeryToken]
    public IActionResult GuardarConfiteria([FromForm] Dictionary<Guid, int> qty)
    {
        var cart = HttpContext.Session.GetCart();
        cart.Confiteria.Clear();
        if (qty is not null)
        {
            foreach (var (id, cantidad) in qty)
            {
                if (cantidad <= 0) continue;
                var prod = ventas.GetProductoStub(id);
                if (prod is null) continue;
                cart.Confiteria.Add(new CartItemConfiteria
                {
                    IdProducto = id,
                    Nombre = prod.Nombre,
                    Cantidad = cantidad,
                    Precio = prod.Precio
                });
            }
        }
        HttpContext.Session.SaveCart(cart);
        return RedirectToAction(nameof(Checkout));
    }

    [HttpGet("checkout")]
    [Authorize(Policy = "RequireCliente")]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        ViewData["NavActive"] = "cartelera";
        var cart = HttpContext.Session.GetCart();
        if (cart.IsEmpty || cart.IdFuncion is null)
        {
            TempData["Flash"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Home");
        }

        decimal descuentoPct = 0;
        string nivel = "Normal";
        if (User.GetEspectadorId() is { } idEsp)
        {
            try
            {
                var d = await clientes.ConsultarDescuentoAsync(idEsp, ct);
                if (d is not null) { descuentoPct = d.Porcentaje; nivel = d.Nivel; }
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Clientes microservice unreachable for descuento.");
            }
        }
        return View(new CheckoutVm(cart, descuentoPct, nivel));
    }

    [HttpPost("checkout")]
    [Authorize(Policy = "RequireCliente")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutInput input, CancellationToken ct)
    {
        var cart = HttpContext.Session.GetCart();
        if (cart.IsEmpty || cart.IdFuncion is null)
        {
            TempData["Flash"] = "Carrito vacío.";
            return RedirectToAction("Index", "Home");
        }
        if (User.GetEspectadorId() is not { } idEspectador)
        {
            ModelState.AddModelError(string.Empty, "Sesión inválida.");
            return RedirectToAction("Login", "Auth");
        }

        decimal descuentoPct = 0;
        try
        {
            var d = await clientes.ConsultarDescuentoAsync(idEspectador, ct);
            if (d is not null) descuentoPct = d.Porcentaje;
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Could not fetch discount; defaulting to 0.");
        }

        var descuento = Math.Round(cart.Subtotal * descuentoPct / 100m, 0);
        var total = cart.Subtotal - descuento;

        // 1. Create the order (Ventas)
        var ordenReq = new CrearOrdenRequest(
            idEspectador,
            cart.Sillas.Select(s => new BoletaDto(s.IdFuncion, s.IdSilla)).ToList(),
            cart.Confiteria.Select(c => new ConfiteriaItemDto(c.IdProducto, c.Cantidad)).ToList(),
            MinutosExpiracion: 15,
            EsEventoCorporativo: false,
            TerceroCorporativo: null);

        CrearOrdenResult ordenResult;
        try
        {
            ordenResult = await ventas.CrearOrdenAsync(ordenReq, ct);
        }
        catch (HttpRequestException ex)
        {
            log.LogError(ex, "Ventas microservice rejected the order.");
            TempData["Flash"] = "No fue posible crear la orden. Verifica disponibilidad.";
            return RedirectToAction(nameof(Carrito));
        }

        // 2. Reserve seats (Infraestructura)
        var expiracion = ordenResult.Expiracion;
        foreach (var s in cart.Sillas)
        {
            try
            {
                await infra.ReservarSillaAsync(s.IdSilla,
                    new ReservarSillaRequest(s.IdFuncion, ordenResult.IdOrden, expiracion), ct);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Failed to reserve silla {IdSilla}; cancelling order.", s.IdSilla);
                try { await ventas.CancelarOrdenAsync(ordenResult.IdOrden, "Reserva de silla falló", ct); } catch { /* swallow */ }
                TempData["Flash"] = "Alguna silla ya no está disponible. Intenta de nuevo.";
                return RedirectToAction(nameof(Carrito));
            }
        }

        // 3. Register transaction (Financiero)
        var conceptos = new List<ConceptoDto>();
        conceptos.AddRange(cart.Sillas.Select(s => new ConceptoDto($"Boleta {s.Label}", s.PrecioBase)));
        conceptos.AddRange(cart.Confiteria.Select(c => new ConceptoDto($"{c.Nombre} x{c.Cantidad}", c.Precio * c.Cantidad)));
        var descuentos = descuento > 0 ? new[] { descuento } : Array.Empty<decimal>();
        var txReq = new RegistrarTransaccionRequest(
            ordenResult.IdOrden,
            conceptos,
            descuentos,
            total,
            "COP",
            input.MetodoPago);

        Guid txId;
        try
        {
            txId = await financiero.RegistrarTransaccionAsync(txReq, ct);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Financiero microservice rejected the transaction.");
            TempData["Flash"] = "Pago rechazado. La orden quedó pendiente.";
            return RedirectToAction(nameof(Carrito));
        }

        // 4. Confirm order (Ventas)
        try
        {
            await ventas.ConfirmarOrdenAsync(ordenResult.IdOrden, ct);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Order confirmation failed (transaction {Tx} was registered).", txId);
        }

        HttpContext.Session.ClearCart();
        return RedirectToAction(nameof(Confirmacion), new { idOrden = ordenResult.IdOrden, idTx = txId });
    }

    [HttpGet("confirmacion/{idOrden:guid}")]
    [Authorize(Policy = "RequireCliente")]
    public async Task<IActionResult> Confirmacion(Guid idOrden, Guid? idTx, CancellationToken ct)
    {
        ViewData["NavActive"] = "cartelera";
        TransaccionDetail? tx = null;
        if (idTx is { } t)
        {
            try { tx = await financiero.GetTransaccionAsync(t, ct); }
            catch (Exception ex) { log.LogWarning(ex, "Could not fetch transaction {Tx}.", t); }
        }
        return View(new ConfirmacionVm(idOrden, tx));
    }
}

public sealed record SillasVm(FuncionDetail Funcion, DisponibilidadResult Disponibilidad, decimal PrecioBoleta);
public sealed record ConfiteriaVm(IReadOnlyCollection<ProductoConfiteria> Productos, Cart Cart);
public sealed record CheckoutVm(Cart Cart, decimal DescuentoPct, string Nivel);
public sealed record ConfirmacionVm(Guid IdOrden, TransaccionDetail? Transaccion);

public sealed class CheckoutInput
{
    public MetodoPago MetodoPago { get; set; } = MetodoPago.TarjetaCredito;
    public string? NombreTarjeta { get; set; }
    public string? NumeroTarjeta { get; set; }
    public string? Vencimiento { get; set; }
    public string? Cvv { get; set; }
}
