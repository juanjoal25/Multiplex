using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace Multiplex.Cli;

internal static class Program
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static Dictionary<string, string> _endpoints = new();
    private static readonly Dictionary<string, HttpClient> _clients = new();
    private static readonly Dictionary<string, Guid> _ctx = new();

    private static async Task<int> Main(string[] args)
    {
        AnsiConsole.Write(new FigletText("Multiplex CLI").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Cliente de consola para los 6 microservicios FRAME[/]");

        LoadConfig();
        BuildClients();

        if (args.Length > 0 && args[0] == "ping") { await PingAllAsync(); return 0; }

        while (true)
        {
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("\n[bold]Menu principal[/]")
                .AddChoices(
                    "1. Clientes",
                    "2. Programacion (peliculas / funciones / cartelera)",
                    "3. Infraestructura (salas / sillas)",
                    "4. Ventas (ordenes)",
                    "5. Financiero (transacciones)",
                    "6. Cadena (sucursales / contratos)",
                    "7. Health-check de todos los servicios",
                    "8. Flujo completo demo (compra end-to-end)",
                    "9. Mostrar contexto guardado",
                    "0. Salir"));

            try
            {
                switch (choice[0])
                {
                    case '1': await MenuClientes(); break;
                    case '2': await MenuProgramacion(); break;
                    case '3': await MenuInfraestructura(); break;
                    case '4': await MenuVentas(); break;
                    case '5': await MenuFinanciero(); break;
                    case '6': await MenuCadena(); break;
                    case '7': await PingAllAsync(); break;
                    case '8': await FlujoDemoAsync(); break;
                    case '9': MostrarContexto(); break;
                    case '0': AnsiConsole.MarkupLine("[yellow]Adios.[/]"); return 0;
                }
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[yellow]Operacion cancelada.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            }

            AnsiConsole.MarkupLine("[grey]-- Presiona ENTER para continuar --[/]");
            Console.ReadLine();
        }
    }

    // ---------- Config ----------

    private static void LoadConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) path = "appsettings.json";
        using var fs = File.OpenRead(path);
        using var doc = JsonDocument.Parse(fs);
        _endpoints = doc.RootElement.GetProperty("Services")
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString()!);
    }

    private static void BuildClients()
    {
        foreach (var (name, url) in _endpoints)
        {
            var c = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(20) };
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _clients[name] = c;
        }
    }

    // ---------- Health ----------

    private static async Task PingAllAsync()
    {
        var table = new Table().AddColumns("Servicio", "URL", "Estado");
        foreach (var (name, url) in _endpoints)
        {
            string status;
            try
            {
                using var resp = await _clients[name].GetAsync("/");
                status = $"[green]OK ({(int)resp.StatusCode})[/]";
            }
            catch (Exception ex)
            {
                status = $"[red]DOWN[/] [grey]{Markup.Escape(ex.GetType().Name)}[/]";
            }
            table.AddRow(name, url, status);
        }
        AnsiConsole.Write(table);
    }

    // ---------- Helpers HTTP ----------

    private static async Task<T?> GetAsync<T>(string svc, string path)
    {
        var resp = await _clients[svc].GetAsync(path);
        return await ReadAsync<T>(resp);
    }

    private static async Task<T?> PostAsync<T>(string svc, string path, object? body)
    {
        var resp = await _clients[svc].PostAsJsonAsync(path, body, Json);
        return await ReadAsync<T>(resp);
    }

    private static async Task<HttpResponseMessage> PostRawAsync(string svc, string path, object? body)
    {
        var resp = await _clients[svc].PostAsJsonAsync(path, body, Json);
        await EnsureOk(resp);
        return resp;
    }

    private static async Task<HttpResponseMessage> PutRawAsync(string svc, string path, object? body)
    {
        var resp = await _clients[svc].PutAsJsonAsync(path, body, Json);
        await EnsureOk(resp);
        return resp;
    }

    private static async Task<HttpResponseMessage> DeleteAsync(string svc, string path)
    {
        var resp = await _clients[svc].DeleteAsync(path);
        await EnsureOk(resp);
        return resp;
    }

    private static async Task<T?> ReadAsync<T>(HttpResponseMessage resp)
    {
        await EnsureOk(resp);
        var raw = await resp.Content.ReadAsStringAsync();
        AnsiConsole.MarkupLine($"[grey]HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}[/]");
        if (string.IsNullOrWhiteSpace(raw)) return default;
        try
        {
            using var doc = JsonDocument.Parse(raw);
            AnsiConsole.WriteLine(JsonSerializer.Serialize(doc.RootElement, Json));
            return JsonSerializer.Deserialize<T>(raw, Json);
        }
        catch
        {
            AnsiConsole.WriteLine(raw);
            return default;
        }
    }

    private static bool TryProp(JsonElement? el, string name, out JsonElement value)
    {
        value = default;
        return el.HasValue && el.Value.ValueKind == JsonValueKind.Object && el.Value.TryGetProperty(name, out value);
    }

    private static async Task EnsureOk(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;
        var body = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
    }

    // ---------- Contexto ----------

    private static void GuardarCtx(string clave, Guid valor)
    {
        _ctx[clave] = valor;
        AnsiConsole.MarkupLine($"[blue]ctx[[{Markup.Escape(clave)}]] = {valor}[/]");
    }

    private static Guid PromptGuid(string label)
    {
        while (true)
        {
            var existing = _ctx.TryGetValue(label, out var v) ? v.ToString() : "";
            var hint = existing != "" ? $" [grey][[{existing}]][/]" : " [grey](x = cancelar)[/]";
            var input = AnsiConsole.Prompt(new TextPrompt<string>($"{label} (GUID){hint}")
                .AllowEmpty()
                .DefaultValue(existing)
                .HideDefaultValue());
            if (string.Equals(input, "x", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(input))
                throw new OperationCanceledException();
            if (Guid.TryParse(input, out var guid)) return guid;
            AnsiConsole.MarkupLine($"[red]GUID invalido:[/] '{Markup.Escape(input)}'. (escribe 'x' para cancelar)");
        }
    }

    private static void MostrarContexto()
    {
        if (_ctx.Count == 0) { AnsiConsole.MarkupLine("[grey](contexto vacio)[/]"); return; }
        var t = new Table().AddColumns("Clave", "Valor");
        foreach (var (k, v) in _ctx) t.AddRow(k, v.ToString());
        AnsiConsole.Write(t);
    }

    // ---------- Selector de GUID desde lista ----------

    private const string Manual = "<< Ingresar GUID manualmente >>";
    private const string Cancelar = "<< Cancelar >>";

    // Muestra un menu de seleccion con la opcion manual al inicio. items = (texto, id).
    private static Guid ChooseFrom(string label, List<(string text, Guid id)> items)
    {
        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine($"[grey]Sin {Markup.Escape(label)} en lista; ingresa el GUID manualmente.[/]");
            return PromptGuid(label);
        }
        var choices = new List<string> { Manual };
        choices.AddRange(items.Select(i => $"{i.text}  |  {i.id}"));
        choices.Add(Cancelar);
        var sel = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"Selecciona [yellow]{Markup.Escape(label)}[/]")
            .PageSize(15).UseConverter(Markup.Escape).AddChoices(choices));
        if (sel == Cancelar) throw new OperationCanceledException();
        if (sel == Manual) return PromptGuid(label);
        var chosen = items[choices.IndexOf(sel) - 1];
        GuardarCtx(label, chosen.id);
        return chosen.id;
    }

    // Descarga una lista (array JSON o el primer array dentro de un objeto) del servicio.
    private static async Task<List<JsonElement>> FetchList(string svc, string path)
    {
        var result = new List<JsonElement>();
        try
        {
            var resp = await _clients[svc].GetAsync(path);
            if (!resp.IsSuccessStatusCode) return result;
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var arr = doc.RootElement;
            if (arr.ValueKind == JsonValueKind.Object)
                foreach (var p in arr.EnumerateObject())
                    if (p.Value.ValueKind == JsonValueKind.Array) { arr = p.Value; break; }
            if (arr.ValueKind == JsonValueKind.Array)
                foreach (var el in arr.EnumerateArray())
                    result.Add(el.Clone());
        }
        catch { /* vacio */ }
        return result;
    }

    private static async Task<Guid> PickGuid(string label, string svc, string path, string idField, Func<JsonElement, string> display)
    {
        var items = (await FetchList(svc, path))
            .Where(e => e.TryGetProperty(idField, out var idp) && Guid.TryParse(idp.GetString(), out _))
            .Select(e => (display(e), Guid.Parse(e.GetProperty(idField).GetString()!)))
            .ToList();
        return ChooseFrom(label, items);
    }

    private static string Str(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) ? v.ToString() : "";
    private static string Short(string s, int n) => s.Length <= n ? s : s[..n];

    // Pickers simples por entidad
    private static Task<Guid> PickEspectador() => PickGuid("idEspectador", "Clientes", "/v1/clientes", "id",
        e => $"{Str(e, "nombre")} [{Str(e, "nivel")}]");
    private static Task<Guid> PickPelicula() => PickGuid("idPelicula", "Programacion", "/v1/programacion/peliculas", "id",
        e => $"{Str(e, "titulo")} ({Str(e, "clasificacion")}, {Str(e, "duracion")}min)");
    private static Task<Guid> PickOrden() => PickGuid("idOrden", "Ventas", "/v1/ventas/orden", "id",
        e => $"{Str(e, "estado")} ${Str(e, "total")} {Str(e, "moneda")}");
    private static Task<Guid> PickTransaccion() => PickGuid("idTransaccion", "Financiero", "/v1/financiera/transacciones", "idTransaccion",
        e => $"{Str(e, "estadoPago")} ${Str(e, "valorTotal")} {Str(e, "moneda")}");
    private static Task<Guid> PickSucursal() => PickGuid("idSucursal", "Cadena", "/v1/cadena/sucursales", "id",
        e => $"{Str(e, "nombre")} ({Str(e, "moneda")})");
    private static Task<Guid> PickProducto() => PickGuid("idProducto", "Ventas", "/v1/ventas/productos", "id",
        e => $"{Str(e, "nombre")} ${Str(e, "precio")} stock={Str(e, "stock")}");

    // Funcion: muestra titulo de pelicula y nombre de sala (cruza las 3 listas).
    private static async Task<Guid> PickFuncion()
    {
        var funciones = await FetchList("Programacion", "/v1/programacion/funciones");
        var peliculas = (await FetchList("Programacion", "/v1/programacion/peliculas"))
            .ToDictionary(p => Str(p, "id"), p => Str(p, "titulo"));
        var salas = (await FetchList("Infraestructura", "/v1/infraestructura/salas"))
            .ToDictionary(s => Str(s, "id"), s => Str(s, "nombre"));

        var items = funciones
            .Where(f => Guid.TryParse(Str(f, "id"), out _))
            .Select(f =>
            {
                var pel = peliculas.GetValueOrDefault(Str(f, "idPelicula"), "?");
                var sala = salas.GetValueOrDefault(Str(f, "idSala"), Short(Str(f, "idSala"), 8));
                return ($"{pel} @ {sala} [{Str(f, "formato")}] {Str(f, "inicio")}", Guid.Parse(Str(f, "id")));
            }).ToList();
        return ChooseFrom("idFuncion", items);
    }

    // Sala con filtro opcional por estado (ej: solo EnMantenimiento al reactivar).
    private static async Task<Guid> PickSala(string? soloEstado = null, bool excluirEstado = false)
    {
        var salas = await FetchList("Infraestructura", "/v1/infraestructura/salas");
        var items = salas
            .Where(s => Guid.TryParse(Str(s, "id"), out _))
            .Where(s => soloEstado is null
                || (excluirEstado ? Str(s, "estado") != soloEstado : Str(s, "estado") == soloEstado))
            .Select(s => ($"{Str(s, "nombre")} [{Str(s, "tipo")}/{Str(s, "estado")}] disp={Str(s, "disponibles")}", Guid.Parse(Str(s, "id"))))
            .ToList();
        return ChooseFrom("idSala", items);
    }

    // Obtiene el idSala de una funcion (consultando su detalle).
    private static async Task<Guid> FuncionSala(Guid idFuncion)
    {
        try
        {
            var resp = await _clients["Programacion"].GetAsync($"/v1/programacion/funcion/{idFuncion}");
            if (resp.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                if (Guid.TryParse(Str(doc.RootElement, "idSala"), out var g)) return g;
            }
        }
        catch { }
        return Guid.Empty;
    }

    // Silla disponible de una sala (selector de lista, para flujos sin contexto de sala).
    private static async Task<Guid> PickSilla(Guid idSala)
    {
        var items = new List<(string text, Guid id)>();
        try
        {
            var resp = await _clients["Infraestructura"].GetAsync($"/v1/infraestructura/salas/{idSala}/disponibilidad");
            if (resp.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("sillas", out var sillas) && sillas.ValueKind == JsonValueKind.Array)
                    foreach (var s in sillas.EnumerateArray())
                        if (Str(s, "estado") == "Disponible" && Guid.TryParse(Str(s, "idSilla"), out var gid))
                            items.Add(($"{Str(s, "fila")}{Str(s, "columna")} [{Str(s, "tipo")}]", gid));
            }
        }
        catch { /* manual */ }
        return ChooseFrom("idSilla", items);
    }

    // Muestra la matriz visual de sillas y deja elegir por fila+columna (ej: "A3").
    private static async Task<Guid> PickSillaMatriz(Guid idSala)
    {
        var allSillas = new List<JsonElement>();
        try
        {
            var resp = await _clients["Infraestructura"].GetAsync($"/v1/infraestructura/salas/{idSala}/disponibilidad");
            if (resp.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("sillas", out var arr) && arr.ValueKind == JsonValueKind.Array)
                    foreach (var s in arr.EnumerateArray()) allSillas.Add(s.Clone());
            }
        }
        catch { }

        if (allSillas.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No se pudo obtener disponibilidad. Ingresa el GUID manualmente.[/]");
            return PromptGuid("idSilla");
        }

        var filas = allSillas.GroupBy(s => Str(s, "fila")).OrderBy(g => g.Key).ToList();
        var allCols = allSillas.Select(s => Str(s, "columna"))
            .Distinct().OrderBy(c => int.TryParse(c, out var n) ? n : int.MaxValue).ToList();

        AnsiConsole.MarkupLine("\n[bold]Mapa de sillas[/]   [green]■[/] Disponible   [red]■[/] No disponible\n");
        AnsiConsole.Markup("      ");
        foreach (var col in allCols) AnsiConsole.Markup($"[grey]{col,3}[/]");
        AnsiConsole.WriteLine();

        foreach (var fila in filas)
        {
            AnsiConsole.Markup($"[bold]{fila.Key,3}[/]   ");
            var byCol = fila.ToDictionary(s => Str(s, "columna"), s => s);
            foreach (var col in allCols)
            {
                if (byCol.TryGetValue(col, out var silla))
                    AnsiConsole.Markup(Str(silla, "estado") == "Disponible" ? "[green]  ■[/]" : "[red]  ■[/]");
                else
                    AnsiConsole.Markup("   .");
            }
            AnsiConsole.WriteLine();
        }
        AnsiConsole.WriteLine();

        while (true)
        {
            var input = AnsiConsole.Prompt(new TextPrompt<string>("Ingresa fila+columna (ej: [bold]A3[/], [bold]B10[/]) o 'x' para cancelar:"));
            if (string.Equals(input, "x", StringComparison.OrdinalIgnoreCase)) throw new OperationCanceledException();

            var filaStr = new string(input.TakeWhile(char.IsLetter).ToArray()).ToUpper();
            var colStr  = new string(input.SkipWhile(char.IsLetter).ToArray()).Trim();

            if (string.IsNullOrEmpty(filaStr) || string.IsNullOrEmpty(colStr))
            {
                AnsiConsole.MarkupLine("[red]Formato invalido. Usa letra(s) para fila y numero(s) para columna, ej: A3[/]");
                continue;
            }

            var match = allSillas.FirstOrDefault(s =>
                string.Equals(Str(s, "fila"), filaStr, StringComparison.OrdinalIgnoreCase) &&
                Str(s, "columna") == colStr);

            if (match.ValueKind == JsonValueKind.Undefined)
            {
                AnsiConsole.MarkupLine($"[red]Silla {Markup.Escape(filaStr + colStr)} no existe en esta sala.[/]");
                continue;
            }
            if (Str(match, "estado") != "Disponible")
            {
                AnsiConsole.MarkupLine($"[red]Silla {Markup.Escape(filaStr + colStr)} no disponible (estado: {Markup.Escape(Str(match, "estado"))}).[/]");
                continue;
            }
            if (!Guid.TryParse(Str(match, "idSilla"), out var guid))
            {
                AnsiConsole.MarkupLine("[red]Error al leer el ID de la silla.[/]");
                continue;
            }

            AnsiConsole.MarkupLine($"[green]Silla {Markup.Escape(filaStr + colStr)} seleccionada (tipo: {Markup.Escape(Str(match, "tipo"))}).[/]");
            return guid;
        }
    }

    // ---------- Menus por servicio ----------

    private static async Task MenuClientes()
    {
        var op = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Clientes[/]")
            .AddChoices("Ver todos los espectadores", "Buscar por nombre/documento", "Registrar espectador", "Consultar descuento", "Ascender nivel", "Descender nivel", "<- volver"));
        switch (op)
        {
            case "Ver todos los espectadores":
                await GetAsync<JsonElement>("Clientes", "/v1/clientes"); break;
            case "Buscar por nombre/documento":
                var nom = AnsiConsole.Prompt(new TextPrompt<string>("Nombre (vacio = omitir):").AllowEmpty());
                var docb = AnsiConsole.Prompt(new TextPrompt<string>("Documento (vacio = omitir):").AllowEmpty());
                await GetAsync<JsonElement>("Clientes", $"/v1/clientes/buscar?nombre={Uri.EscapeDataString(nom)}&documento={Uri.EscapeDataString(docb)}"); break;
            case "Registrar espectador":
                var body = new
                {
                    Nombre = AnsiConsole.Ask<string>("Nombre:"),
                    Apellido = AnsiConsole.Ask<string>("Apellido:"),
                    Correo = AnsiConsole.Ask<string>("Correo:"),
                    TipoDocumento = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("TipoDocumento").AddChoices("CC", "CE", "PAS")),
                    NumeroDocumento = AnsiConsole.Ask<string>("NumeroDocumento:")
                };
                var r = await PostAsync<JsonElement>("Clientes", "/v1/clientes/registro", body);
                if (TryProp(r, "idEspectador", out var idE)) GuardarCtx("idEspectador", idE.GetGuid());
                break;
            case "Consultar descuento":
                await GetAsync<JsonElement>("Clientes", $"/v1/clientes/{await PickEspectador()}/descuento"); break;
            case "Ascender nivel":
                var espAsc = await PickEspectador();
                // Genera automaticamente la transaccion de pago de la membresia en Financiero.
                AnsiConsole.MarkupLine("[grey]Generando pago de membresia automaticamente...[/]");
                var pagoBody = new
                {
                    IdOrden = Guid.NewGuid(),
                    Conceptos = new[] { new { Descripcion = "Pago membresia (ascenso de nivel)", Valor = 30000m } },
                    Descuentos = Array.Empty<decimal>(),
                    ValorTotal = 30000m,
                    Moneda = "COP",
                    MetodoPago = "TarjetaCredito"
                };
                var pago = await PostAsync<JsonElement>("Financiero", "/v1/financiera/transacciones", pagoBody);
                if (!TryProp(pago, "id", out var idPago)) { AnsiConsole.MarkupLine("[red]No se pudo generar el pago.[/]"); break; }
                await PostRawAsync("Clientes", $"/v1/clientes/{espAsc}/ascender", new { IdOrdenPago = idPago.GetGuid() });
                AnsiConsole.MarkupLine("[green]Nivel ascendido (pago generado automaticamente).[/]");
                break;
            case "Descender nivel":
                await PostRawAsync("Clientes", $"/v1/clientes/{await PickEspectador()}/descender", new { }); break;
        }
    }

    private static async Task MenuProgramacion()
    {
        var op = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Programacion[/]")
            .AddChoices("Ver todas las peliculas", "Ver todas las funciones", "Crear pelicula", "Programar funcion", "Consultar funcion", "Cancelar funcion", "Ver cartelera", "<- volver"));
        switch (op)
        {
            case "Ver todas las peliculas":
                await GetAsync<JsonElement>("Programacion", "/v1/programacion/peliculas"); break;
            case "Ver todas las funciones":
                await GetAsync<JsonElement>("Programacion", "/v1/programacion/funciones"); break;
            case "Crear pelicula":
                var body = new
                {
                    Titulo = AnsiConsole.Ask<string>("Titulo:"),
                    Clasificacion = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Clasificacion").AddChoices("G", "PG", "PG13", "R")),
                    Genero = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Genero").AddChoices(
                        "Accion", "Aventura", "Comedia", "Drama", "Terror", "Ciencia Ficcion", "Romance", "Animacion", "Documental", "Suspenso")),
                    DuracionMinutos = AnsiConsole.Ask<int>("DuracionMinutos:"),
                    Formato = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Formato").AddChoices("Formato2D", "Formato3D", "FormatoIMAX", "Formato4DX"))
                };
                var r = await PostAsync<JsonElement>("Programacion", "/v1/programacion/peliculas", body);
                if (TryProp(r, "id", out var id)) GuardarCtx("idPelicula", id.GetGuid());
                break;
            case "Programar funcion":
                var pelF = await PickPelicula();
                var salaF = await PickSala();
                var body2 = new
                {
                    IdPelicula = pelF,
                    IdSala = salaF,
                    Inicio = AnsiConsole.Ask<DateTime>("Inicio (UTC ISO):", DateTime.UtcNow.AddHours(1)),
                    Fin = AnsiConsole.Ask<DateTime>("Fin (UTC ISO):", DateTime.UtcNow.AddHours(3)),
                    Formato = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Formato").AddChoices("Formato2D", "Formato3D", "FormatoIMAX", "Formato4DX"))
                };
                var r2 = await PostAsync<JsonElement>("Programacion", "/v1/programacion/funciones", body2);
                if (TryProp(r2, "id", out var idF)) GuardarCtx("idFuncion", idF.GetGuid());
                break;
            case "Consultar funcion":
                await GetAsync<JsonElement>("Programacion", $"/v1/programacion/funcion/{await PickFuncion()}"); break;
            case "Cancelar funcion":
                var funCancel = await PickFuncion();
                var motivo = AnsiConsole.Ask<string>("Motivo:");
                await DeleteAsync("Programacion", $"/v1/programacion/funciones/{funCancel}?motivo={Uri.EscapeDataString(motivo)}"); break;
            case "Ver cartelera":
                await GetAsync<JsonElement>("Programacion", "/v1/programacion/cartelera"); break;
        }
    }

    private static async Task MenuInfraestructura()
    {
        var op = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Infraestructura[/]")
            .AddChoices("Ver todas las salas", "Ver sala", "Disponibilidad sala", "Enviar a mantenimiento", "Reactivar sala", "Reservar silla", "Liberar silla", "Ver silla", "<- volver"));
        switch (op)
        {
            case "Ver todas las salas":
                await GetAsync<JsonElement>("Infraestructura", "/v1/infraestructura/salas"); break;
            case "Ver sala": await GetAsync<JsonElement>("Infraestructura", $"/v1/infraestructura/salas/{await PickSala()}"); break;
            case "Disponibilidad sala": await GetAsync<JsonElement>("Infraestructura", $"/v1/infraestructura/salas/{await PickSala()}/disponibilidad"); break;
            case "Enviar a mantenimiento": await PostRawAsync("Infraestructura", $"/v1/infraestructura/salas/{await PickSala("EnMantenimiento", excluirEstado: true)}/mantenimiento", new { }); break;
            case "Reactivar sala": await PostRawAsync("Infraestructura", $"/v1/infraestructura/salas/{await PickSala("EnMantenimiento")}/reactivar", new { }); break;
            case "Reservar silla":
                var funR = await PickFuncion();
                var ordR = await PickOrden();
                var salaR = await PickSala();
                var sillaR = await PickSilla(salaR);
                var br = new
                {
                    IdFuncion = funR,
                    IdOrden = ordR,
                    Expiracion = AnsiConsole.Ask<DateTime>("Expiracion (UTC):", DateTime.UtcNow.AddMinutes(15))
                };
                await PutRawAsync("Infraestructura", $"/v1/infraestructura/sillas/{sillaR}/reservar", br); break;
            case "Liberar silla":
                var salaL = await PickSala();
                var sillaL = await PickSilla(salaL);
                await PostRawAsync("Infraestructura", $"/v1/infraestructura/sillas/{sillaL}/liberar",
                    new { Motivo = AnsiConsole.Ask<string>("Motivo:") }); break;
            case "Ver silla":
                var salaV = await PickSala();
                await GetAsync<JsonElement>("Infraestructura", $"/v1/infraestructura/sillas/{await PickSilla(salaV)}"); break;
        }
    }

    private static async Task MenuVentas()
    {
        var op = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Ventas[/]")
            .AddChoices("Ver todas las ordenes", "Crear orden", "Confirmar orden", "Cancelar orden", "<- volver"));
        switch (op)
        {
            case "Ver todas las ordenes":
                await GetAsync<JsonElement>("Ventas", "/v1/ventas/orden"); break;
            case "Crear orden":
                var idEsp = await PickEspectador();
                // Una orden = una sola funcion. Se elige la funcion una vez y su sala se deriva.
                var fun = await PickFuncion();
                var salaFun = await FuncionSala(fun);
                if (salaFun == Guid.Empty) { AnsiConsole.MarkupLine("[red]No se pudo obtener la sala de la funcion.[/]"); break; }
                AnsiConsole.MarkupLine($"[grey]Funcion {fun} en sala {salaFun}. Todas las boletas son de esta funcion.[/]");
                var boletas = new List<object>();
                do
                {
                    var silla = await PickSillaMatriz(salaFun);
                    boletas.Add(new { IdFuncion = fun, IdSilla = silla });
                } while (AnsiConsole.Confirm("Agregar otra boleta (misma funcion)?", false));
                var conf = new List<object>();
                while (AnsiConsole.Confirm("Agregar confiteria?", false))
                    conf.Add(new { IdProducto = await PickProducto(), Cantidad = AnsiConsole.Ask<int>("Cantidad:") });
                var body = new
                {
                    IdEspectador = idEsp,
                    Boletas = boletas,
                    Confiterias = conf,
                    MinutosExpiracion = AnsiConsole.Ask("MinutosExpiracion:", 15),
                    EsEventoCorporativo = AnsiConsole.Confirm("EsEventoCorporativo?", false),
                    TerceroCorporativo = (string?)null
                };
                var r = await PostAsync<JsonElement>("Ventas", "/v1/ventas/orden", body);
                if (TryProp(r, "idOrden", out var idO)) GuardarCtx("idOrden", idO.GetGuid());
                break;
            case "Confirmar orden":
                await PostRawAsync("Ventas", $"/v1/ventas/orden/{await PickOrden()}/confirmar", new { }); break;
            case "Cancelar orden":
                var ordC = await PickOrden();
                var motivo = AnsiConsole.Ask<string>("Motivo:");
                await DeleteAsync("Ventas", $"/v1/ventas/orden/{ordC}?motivo={Uri.EscapeDataString(motivo)}"); break;
        }
    }

    private static async Task MenuFinanciero()
    {
        var op = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Financiero[/]")
            .AddChoices("Ver todas las transacciones", "Registrar transaccion", "Ver transaccion", "Revertir transaccion", "Consultar historial", "<- volver"));
        switch (op)
        {
            case "Ver todas las transacciones":
                await GetAsync<JsonElement>("Financiero", "/v1/financiera/transacciones"); break;
            case "Registrar transaccion":
                var conceptos = new List<object>();
                while (AnsiConsole.Confirm("Agregar concepto?", true))
                    conceptos.Add(new { Descripcion = AnsiConsole.Ask<string>("Descripcion:"), Valor = AnsiConsole.Ask<decimal>("Valor:") });
                var idOrdTx = await PickOrden();
                var body = new
                {
                    IdOrden = idOrdTx,
                    Conceptos = conceptos,
                    Descuentos = new decimal[0],
                    ValorTotal = AnsiConsole.Ask<decimal>("ValorTotal:"),
                    Moneda = AnsiConsole.Ask("Moneda:", "COP"),
                    MetodoPago = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("MetodoPago").AddChoices("TarjetaCredito", "TarjetaDebito", "EfectivoTaquilla", "BilleteraDigital"))
                };
                var r = await PostAsync<JsonElement>("Financiero", "/v1/financiera/transacciones", body);
                if (TryProp(r, "id", out var idT)) GuardarCtx("idTransaccion", idT.GetGuid());
                break;
            case "Ver transaccion":
                await GetAsync<JsonElement>("Financiero", $"/v1/financiera/transacciones/{await PickTransaccion()}"); break;
            case "Revertir transaccion":
                var txRev = await PickTransaccion();
                await PostRawAsync("Financiero", $"/v1/financiera/transacciones/{txRev}/revertir",
                    new { Motivo = AnsiConsole.Ask<string>("Motivo:") }); break;
            case "Consultar historial":
                var ini = AnsiConsole.Ask("Inicio (UTC):", DateTime.UtcNow.AddDays(-30));
                var fin = AnsiConsole.Ask("Fin (UTC):", DateTime.UtcNow);
                await GetAsync<JsonElement>("Financiero", $"/v1/financiera/historial?inicio={ini:O}&fin={fin:O}"); break;
        }
    }

    private static async Task MenuCadena()
    {
        var op = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Cadena[/]")
            .AddChoices("Ver todas las sucursales", "Ver contratos activos", "Consultar configuracion", "Actualizar configuracion", "Registrar contrato", "Buscar contrato vigente", "Cancelar contrato", "<- volver"));
        switch (op)
        {
            case "Ver todas las sucursales":
                await GetAsync<JsonElement>("Cadena", "/v1/cadena/sucursales"); break;
            case "Ver contratos activos":
                if (AnsiConsole.Confirm("Filtrar por una sucursal?", false))
                    await GetAsync<JsonElement>("Cadena", $"/v1/cadena/contratos/activos?idSucursal={await PickSucursal()}");
                else
                    await GetAsync<JsonElement>("Cadena", "/v1/cadena/contratos/activos");
                break;
            case "Consultar configuracion":
                await GetAsync<JsonElement>("Cadena", $"/v1/cadena/configuracion/{await PickSucursal()}"); break;
            case "Actualizar configuracion":
                var sucActualizar = await PickSucursal();
                var pars = new List<object>();
                while (AnsiConsole.Confirm("Agregar parametro?", true))
                    pars.Add(new
                    {
                        Clave = AnsiConsole.Ask<string>("Clave:"),
                        Valor = AnsiConsole.Ask<string>("Valor:"),
                        Tipo = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Tipo").AddChoices("String", "Entero", "Booleano", "Decimal"))
                    });
                await PutRawAsync("Cadena", $"/v1/cadena/configuracion/{sucActualizar}", new { Parametros = pars }); break;
            case "Registrar contrato":
                var sucContrato = await PickSucursal();
                var body = new
                {
                    IdSucursal = sucContrato,
                    Tercero = AnsiConsole.Ask<string>("Tercero:"),
                    VigenciaInicio = AnsiConsole.Ask("VigenciaInicio (UTC):", DateTime.UtcNow),
                    VigenciaFin = AnsiConsole.Ask("VigenciaFin (UTC):", DateTime.UtcNow.AddYears(1)),
                    Condiciones = AnsiConsole.Ask<string>("Condiciones:")
                };
                var r = await PostAsync<JsonElement>("Cadena", "/v1/cadena/contratos", body);
                if (TryProp(r, "id", out var idC)) GuardarCtx("idContrato", idC.GetGuid());
                break;
            case "Buscar contrato vigente":
                await GetAsync<JsonElement>("Cadena", $"/v1/cadena/contratos?tercero={Uri.EscapeDataString(AnsiConsole.Ask<string>("Tercero:"))}"); break;
            case "Cancelar contrato":
                var m = AnsiConsole.Ask<string>("Motivo:");
                await DeleteAsync("Cadena", $"/v1/cadena/contratos/{PromptGuid("idContrato")}?motivo={Uri.EscapeDataString(m)}"); break;
        }
    }

    // ---------- Flujo demo ----------

    private static void Paso(string n, string txt) => AnsiConsole.MarkupLine($"\n[bold cyan]== {n} ==[/] {Markup.Escape(txt)}");
    private static void Info(string k, object? v) => AnsiConsole.MarkupLine($"   [grey]{Markup.Escape(k)}:[/] [white]{Markup.Escape(v?.ToString() ?? "")}[/]");

    // Devuelve el primer inicio libre para una sala, evitando solapamiento con funciones existentes.
    private static async Task<DateTime> NextAvailableSlot(Guid idSala, TimeSpan duracion)
    {
        var funciones = await FetchList("Programacion", "/v1/programacion/funciones");
        var ultimoFin = funciones
            .Where(f => string.Equals(Str(f, "idSala"), idSala.ToString(), StringComparison.OrdinalIgnoreCase)
                     && Str(f, "estado") != "Cancelada")
            .Select(f => DateTime.TryParse(Str(f, "fin"), out var d) ? d.ToUniversalTime() : DateTime.MinValue)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();

        var minInicio = DateTime.UtcNow.AddHours(2);
        return ultimoFin.AddMinutes(15) > minInicio ? ultimoFin.AddMinutes(15) : minInicio;
    }

    // Primera sala Disponible y su primera silla libre.
    private static async Task<(Guid sala, Guid silla)> SalaYSillaDisponible()
    {
        var salas = await FetchList("Infraestructura", "/v1/infraestructura/salas");
        foreach (var s in salas.Where(x => Str(x, "estado") == "Disponible"))
        {
            if (!Guid.TryParse(Str(s, "id"), out var idSala)) continue;
            var resp = await _clients["Infraestructura"].GetAsync($"/v1/infraestructura/salas/{idSala}/disponibilidad");
            if (!resp.IsSuccessStatusCode) continue;
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("sillas", out var sillas))
                foreach (var si in sillas.EnumerateArray())
                    if (Str(si, "estado") == "Disponible" && Guid.TryParse(Str(si, "idSilla"), out var idSilla))
                        return (idSala, idSilla);
        }
        return (Guid.Empty, Guid.Empty);
    }

    private static async Task<JsonElement?> GetJson(string svc, string path)
    {
        try
        {
            var resp = await _clients[svc].GetAsync(path);
            if (!resp.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.Clone();
        }
        catch { return null; }
    }

    private static async Task FlujoDemoAsync()
    {
        AnsiConsole.Write(new Rule("[yellow]Flujo demo end-to-end[/]").LeftJustified());
        var creado = new Dictionary<string, Guid>();

        // 1. Espectador (idempotente: reutiliza si el documento fijo ya existe)
        Paso("1. Clientes", "Registrar espectador de prueba (reutiliza si ya existe)");
        const string demoDoc = "CLI_DEMO_001";
        var espBuscar = await FetchList("Clientes", $"/v1/clientes/buscar?documento={Uri.EscapeDataString(demoDoc)}");
        Guid idEsp;
        if (espBuscar.Count > 0 && Guid.TryParse(Str(espBuscar[0], "id"), out var idEspExist))
        {
            idEsp = idEspExist; creado["espectador"] = idEsp;
            AnsiConsole.MarkupLine("[grey]Espectador ya existe, reutilizando.[/]");
        }
        else
        {
            var esp = await PostAsync<JsonElement>("Clientes", "/v1/clientes/registro", new
            {
                Nombre = "Demo", Apellido = "CLI", Correo = "demo.cli@multiplex.test",
                TipoDocumento = "CC", NumeroDocumento = demoDoc
            });
            if (!TryProp(esp, "idEspectador", out var idE)) { AnsiConsole.MarkupLine("[red]Fallo registro.[/]"); return; }
            idEsp = idE.GetGuid(); creado["espectador"] = idEsp;
        }
        Info("idEspectador", idEsp); Info("documento", "CC " + demoDoc);

        var desc0 = await GetJson("Clientes", $"/v1/clientes/{idEsp}/descuento");
        Info("nivel inicial", Str(desc0 ?? default, "nivel")); Info("descuento", Str(desc0 ?? default, "porcentaje"));

        // 2. Pelicula (idempotente: reutiliza si el titulo fijo ya existe)
        Paso("2. Programacion", "Crear pelicula (reutiliza si ya existe)");
        const string demoTitulo = "Demo Movie CLI";
        var pelLista = await FetchList("Programacion", "/v1/programacion/peliculas");
        var pelExist = pelLista.FirstOrDefault(p => string.Equals(Str(p, "titulo"), demoTitulo, StringComparison.OrdinalIgnoreCase));
        Guid idPel;
        if (pelExist.ValueKind != JsonValueKind.Undefined && Guid.TryParse(Str(pelExist, "id"), out var idPelExist))
        {
            idPel = idPelExist; creado["pelicula"] = idPel;
            AnsiConsole.MarkupLine("[grey]Pelicula ya existe, reutilizando.[/]");
        }
        else
        {
            var pel = await PostAsync<JsonElement>("Programacion", "/v1/programacion/peliculas", new
            {
                Titulo = demoTitulo, Clasificacion = "PG13",
                Genero = "Ciencia Ficcion", DuracionMinutos = 120, Formato = "Formato2D"
            });
            if (!TryProp(pel, "id", out var idP)) { AnsiConsole.MarkupLine("[red]Fallo pelicula.[/]"); return; }
            idPel = idP.GetGuid(); creado["pelicula"] = idPel;
        }
        Info("idPelicula", idPel);

        // 3. Sala + silla disponible
        Paso("3. Infraestructura", "Buscar sala y silla disponibles");
        var (idSala, idSilla) = await SalaYSillaDisponible();
        if (idSala == Guid.Empty) { AnsiConsole.MarkupLine("[red]No hay sala/silla disponible.[/]"); return; }
        Info("idSala", idSala); Info("idSilla", idSilla);

        // 4. Funcion — busca un horario libre para no solapar con funciones existentes en esa sala
        Paso("4. Programacion", "Programar funcion en cartelera");
        var slot1 = await NextAvailableSlot(idSala, TimeSpan.FromHours(2));
        var fun = await PostAsync<JsonElement>("Programacion", "/v1/programacion/funciones", new
        {
            IdPelicula = idPel, IdSala = idSala,
            Inicio = slot1, Fin = slot1.AddHours(2), Formato = "Formato2D"
        });
        if (!TryProp(fun, "id", out var idF)) { AnsiConsole.MarkupLine("[red]Fallo funcion.[/]"); return; }
        var idFun = idF.GetGuid(); creado["funcion"] = idFun;
        Info("idFuncion", idFun); Info("inicio", slot1);

        // 5. Orden con 1 boleta
        Paso("5. Ventas", "Crear orden (1 boleta)");
        var orden = await PostAsync<JsonElement>("Ventas", "/v1/ventas/orden", new
        {
            IdEspectador = idEsp,
            Boletas = new[] { new { IdFuncion = idFun, IdSilla = idSilla } },
            Confiterias = Array.Empty<object>(),
            MinutosExpiracion = 15, EsEventoCorporativo = false, TerceroCorporativo = (string?)null
        });
        if (!TryProp(orden, "idOrden", out var idO)) { AnsiConsole.MarkupLine("[red]Fallo orden.[/]"); return; }
        var idOrden = idO.GetGuid(); creado["orden"] = idOrden;
        var total = TryProp(orden, "total", out var vt) ? vt.GetDecimal() : 0m;
        Info("idOrden", idOrden); Info("total", total);

        // 6. Transaccion + confirmar
        Paso("6. Financiero", "Registrar transaccion y confirmar orden");
        var tx = await PostAsync<JsonElement>("Financiero", "/v1/financiera/transacciones", new
        {
            IdOrden = idOrden,
            Conceptos = new[] { new { Descripcion = "Boleta demo", Valor = total } },
            Descuentos = Array.Empty<decimal>(), ValorTotal = total, Moneda = "COP", MetodoPago = "TarjetaCredito"
        });
        if (TryProp(tx, "id", out var idT)) { creado["transaccion"] = idT.GetGuid(); Info("idTransaccion", idT.GetGuid()); }
        await PostRawAsync("Ventas", $"/v1/ventas/orden/{idOrden}/confirmar", new { });
        Info("orden", "confirmada");

        // 7. Ascender nivel (pago automatico) y ver descuento
        Paso("7. Clientes", "Ascender nivel (pago de membresia automatico)");
        var pago = await PostAsync<JsonElement>("Financiero", "/v1/financiera/transacciones", new
        {
            IdOrden = Guid.NewGuid(),
            Conceptos = new[] { new { Descripcion = "Membresia", Valor = 30000m } },
            Descuentos = Array.Empty<decimal>(), ValorTotal = 30000m, Moneda = "COP", MetodoPago = "TarjetaCredito"
        });
        if (TryProp(pago, "id", out var idPagoAsc))
        {
            await PostRawAsync("Clientes", $"/v1/clientes/{idEsp}/ascender", new { IdOrdenPago = idPagoAsc.GetGuid() });
            var desc1 = await GetJson("Clientes", $"/v1/clientes/{idEsp}/descuento");
            Info("nivel tras ascender", Str(desc1 ?? default, "nivel")); Info("descuento", Str(desc1 ?? default, "porcentaje"));
        }

        // 8. Descender nivel y ver descuento
        Paso("8. Clientes", "Descender nivel");
        await PostRawAsync("Clientes", $"/v1/clientes/{idEsp}/descender", new { });
        var desc2 = await GetJson("Clientes", $"/v1/clientes/{idEsp}/descuento");
        Info("nivel tras descender", Str(desc2 ?? default, "nivel")); Info("descuento", Str(desc2 ?? default, "porcentaje"));

        // 9. Segunda compra: orden con confiteria
        Paso("9. Ventas", "Segunda compra con confiteria");
        var (sala2, silla2) = await SalaYSillaDisponible();
        var productos = await FetchList("Ventas", "/v1/ventas/productos");
        var confiteria = new List<object>();
        if (productos.Count > 0 && Guid.TryParse(Str(productos[0], "id"), out var idProd))
        {
            confiteria.Add(new { IdProducto = idProd, Cantidad = 2 });
            Info("producto", $"{Str(productos[0], "nombre")} x2");
        }
        // funcion para la segunda sala — horario libre para no solapar
        var slot2 = await NextAvailableSlot(sala2, TimeSpan.FromHours(2));
        var fun2 = await PostAsync<JsonElement>("Programacion", "/v1/programacion/funciones", new
        {
            IdPelicula = idPel, IdSala = sala2,
            Inicio = slot2, Fin = slot2.AddHours(2), Formato = "Formato2D"
        });
        if (TryProp(fun2, "id", out var idF2) && silla2 != Guid.Empty)
        {
            var orden2 = await PostAsync<JsonElement>("Ventas", "/v1/ventas/orden", new
            {
                IdEspectador = idEsp,
                Boletas = new[] { new { IdFuncion = idF2.GetGuid(), IdSilla = silla2 } },
                Confiterias = confiteria,
                MinutosExpiracion = 15, EsEventoCorporativo = false, TerceroCorporativo = (string?)null
            });
            if (TryProp(orden2, "idOrden", out var idO2)) { creado["orden2"] = idO2.GetGuid(); Info("idOrden2", idO2.GetGuid()); }
            var total2 = TryProp(orden2, "total", out var vt2) ? vt2.GetDecimal() : 0m;
            Info("total2 (con confiteria)", total2);
        }

        // Resumen
        AnsiConsole.Write(new Rule("[green]Resumen objetos creados[/]").LeftJustified());
        var tabla = new Table().AddColumns("Objeto", "Id");
        foreach (var (k, v) in creado) tabla.AddRow(k, v.ToString());
        AnsiConsole.Write(tabla);
        AnsiConsole.MarkupLine("[green]Flujo demo finalizado correctamente.[/]");
    }
}
