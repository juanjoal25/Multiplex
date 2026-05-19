using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Multiplex.Web.Models.Dtos;

namespace Multiplex.Web.Models;

public sealed class CartItemSilla
{
    public Guid IdFuncion { get; set; }
    public Guid IdSilla { get; set; }
    public string Label { get; set; } = string.Empty;   // "F-12" etc.
    public string TipoSilla { get; set; } = "Estandar";
    public decimal PrecioBase { get; set; }              // base ticket price + format surcharge
}

public sealed class CartItemConfiteria
{
    public Guid IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
}

public sealed class Cart
{
    public Guid? IdFuncion { get; set; }
    public string? Pelicula { get; set; }
    public string? SalaNombre { get; set; }
    public DateTime? Horario { get; set; }
    public string? Formato { get; set; }
    public List<CartItemSilla> Sillas { get; set; } = new();
    public List<CartItemConfiteria> Confiteria { get; set; } = new();

    public decimal SubtotalBoletas => Sillas.Sum(s => s.PrecioBase);
    public decimal SubtotalConfiteria => Confiteria.Sum(c => c.Precio * c.Cantidad);
    public decimal Subtotal => SubtotalBoletas + SubtotalConfiteria;

    public bool IsEmpty => Sillas.Count == 0 && Confiteria.Count == 0;
}

public static class CartSessionExtensions
{
    private const string Key = "multiplex.cart";
    private static readonly JsonSerializerOptions _opt = new() { PropertyNamingPolicy = null };

    public static Cart GetCart(this ISession s)
    {
        var raw = s.GetString(Key);
        if (string.IsNullOrEmpty(raw)) return new Cart();
        try { return JsonSerializer.Deserialize<Cart>(raw, _opt) ?? new Cart(); }
        catch { return new Cart(); }
    }

    public static void SaveCart(this ISession s, Cart c)
        => s.SetString(Key, JsonSerializer.Serialize(c, _opt));

    public static void ClearCart(this ISession s) => s.Remove(Key);
}
