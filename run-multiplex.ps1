Write-Host "`n=== 1. Limpieza completa ===" -ForegroundColor Cyan

Write-Host "Limpiando directorios bin/obj..." -ForegroundColor Yellow
Get-ChildItem -Path src -Directory -Recurse | Where-Object { $_.Name -in @("bin", "obj") } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Reseteando Docker (bases de datos + RabbitMQ)..." -ForegroundColor Yellow
docker compose down -v 2>$null
Start-Sleep -Seconds 2

Write-Host "`n=== 2. Levantando Infraestructura Local (Docker) ===" -ForegroundColor Cyan
docker compose up -d

Write-Host "Esperando a que las bases de datos y RabbitMQ estén listos (15s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`n=== 3. Restaurando y Compilando la Solución .NET 10 ===" -ForegroundColor Cyan
dotnet restore Multiplex.slnx
dotnet build Multiplex.slnx

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error en la compilación. Abortando." -ForegroundColor Red
    exit
}

Write-Host "`n=== 4. Lanzando los componentes en ventanas independientes ===" -ForegroundColor Cyan

# Diccionario con todos los servicios del Backend
$Services = @{
    "Clientes"        = "src/Clientes/Clientes.Api"
    "Programacion"    = "src/Programacion/Programacion.Api"
    "Infraestructura" = "src/Infraestructura/Infraestructura.Api"
    "Ventas"          = "src/Ventas/Ventas.Api"
    "Financiero"      = "src/Financiero/Financiero.Api"
    "Cadena"          = "src/Cadena/Cadena.Api"
}

# 4.1 Iniciar Backend
foreach ($Name in $Services.Keys) {
    $Path = $Services[$Name]
    Write-Host "Iniciando servicio API: $Name..." -ForegroundColor Green
    Start-Process cmd -ArgumentList "/k title API $Name && dotnet run --project $Path"
}

Write-Host "`n==============================================================================" -ForegroundColor Cyan
Write-Host "¡Todo en marcha! Se han abierto 6 terminales con los microservicios." -ForegroundColor Green
Write-Host "" -ForegroundColor Cyan
Write-Host "Para interactuar con los servicios, ejecuta en otra terminal:" -ForegroundColor Cyan
Write-Host "  dotnet run --project src/Cli/Multiplex.Cli" -ForegroundColor Yellow
Write-Host "" -ForegroundColor Cyan
Write-Host "URLs de los servicios:" -ForegroundColor Cyan
Write-Host "  Clientes:        http://localhost:5001" -ForegroundColor Gray
Write-Host "  Programacion:    http://localhost:5002" -ForegroundColor Gray
Write-Host "  Infraestructura: http://localhost:5003" -ForegroundColor Gray
Write-Host "  Ventas:          http://localhost:5004" -ForegroundColor Gray
Write-Host "  Financiero:      http://localhost:5005" -ForegroundColor Gray
Write-Host "  Cadena:          http://localhost:5006" -ForegroundColor Gray
Write-Host "==============================================================================" -ForegroundColor Cyan