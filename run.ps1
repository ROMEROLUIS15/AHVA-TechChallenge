# Arranque del portal de acceso CEPLAN.
# Uso:  .\run.ps1      (desde la raiz del proyecto, en PowerShell)
# Deja el .NET SDK local en el PATH de ESTA sesion, levanta SQL Server y ejecuta la app.

$ErrorActionPreference = "Stop"

# 1) .NET SDK local (por si no esta en el PATH de la terminal)
$env:DOTNET_ROOT = "$HOME\.dotnet"
if ($env:Path -notlike "*$env:DOTNET_ROOT*") {
    $env:Path = "$env:DOTNET_ROOT;$env:Path"
}

# 2) SQL Server (Docker). Si ya esta corriendo, no hace nada.
Write-Host "==> Levantando SQL Server (Docker)..." -ForegroundColor Cyan
docker compose up -d

# 3) Ejecutar la app
Write-Host "==> Iniciando la aplicacion en https://localhost:5443 ..." -ForegroundColor Cyan
Write-Host "    Abre: https://localhost:5443/Account/Activation" -ForegroundColor Green
dotnet run --project src/Ceplan.Web --urls "https://localhost:5443;http://localhost:5080"
