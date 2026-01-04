<#
.SYNOPSIS
    Launches all Prodplace application services locally in separate windows.
    Assumes infrastructure (Databases, RabbitMQ, Redis, Keycloak) is running (e.g., via Docker).

.DESCRIPTION
    This script starts the following services:
    - ProductsService (HTTPS 44301)
    - PriceService (HTTPS 44302)
    - CurrencyRatesService (HTTPS 44303)
    - IdentityService (HTTPS 44304)
    - OrderService (HTTPS 44305)
    - CustomerService (HTTPS 44306)
    - ProxyService (HTTP 44300) - Matches Frontend Proxy Config
    - Frontend (npm run serve) - via Vue CLI
#>

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   Prodplace Local Launcher" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Check/Warn about Infrastructure
Write-Host "NOTE: This script assumes required infrastructure is running." -ForegroundColor Yellow
Write-Host "If you have not started the databases/queues, please run:" -ForegroundColor Gray
Write-Host "   docker-compose --profile db up -d" -ForegroundColor White
Write-Host ""
$response = Read-Host "Pres Enter to continue launch (or Ctrl+C to abort)"

# Helper to start .NET Service
function Start-DotNetService {
    param(
        [string]$ProjectParams,
        [string]$Urls,
        [string]$Title
    )
    Write-Host "Starting $Title..." -ForegroundColor Green
    # We set environment variables and use '-c Debug' to ensure debug mode.
    # We use 'powershell -NoExit' to keep the window open so you can see logs/errors.
    $Command = "`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run $ProjectParams -c Debug --urls '$Urls'"
    Start-Process -FilePath "powershell" -ArgumentList "-NoExit", "-Command", $Command -WorkingDirectory $PWD -WindowStyle Normal
}

# 2. Launch Backend Services
# ProductsService -> https://localhost:44301
Start-DotNetService -ProjectParams "--project ProductsService" -Urls "https://localhost:44301" -Title "ProductsService"

# PriceService -> https://localhost:44302
Start-DotNetService -ProjectParams "--project PriceService" -Urls "https://localhost:44302" -Title "PriceService"

# CurrencyRatesService -> https://localhost:44303
Start-DotNetService -ProjectParams "--project CurrencyRatesService" -Urls "https://localhost:44303" -Title "CurrencyRatesService"

# IdentityService -> https://localhost:44304
Start-DotNetService -ProjectParams "--project IdentityService" -Urls "https://localhost:44304" -Title "IdentityService"

# OrderService -> https://localhost:44305
Start-DotNetService -ProjectParams "--project OrderService" -Urls "https://localhost:44305" -Title "OrderService"

# CustomerService -> https://localhost:44306
Start-DotNetService -ProjectParams "--project CustomerService" -Urls "https://localhost:44306" -Title "CustomerService"

# 3. Launch Proxy Service
# Proxy listens on HTTP 44300 because standard Vue config proxies to http://localhost:44300
Start-DotNetService -ProjectParams "--project ProxyService" -Urls "http://localhost:44300" -Title "ProxyService"

# 4. Launch Frontend
Write-Host "Starting Frontend (Vue.js)..." -ForegroundColor Green
if (Test-Path "fe") {
    $FeCommand = "cd fe; bun install; bun run serve"
    Start-Process -FilePath "powershell" -ArgumentList "-NoExit", "-Command", $FeCommand -WorkingDirectory $PWD -WindowStyle Normal
}
else {
    Write-Host "Frontend directory 'fe' not found!" -ForegroundColor Red
}

Write-Host "All services launched. Check the separate windows for logs." -ForegroundColor Cyan

# 5. Open Browser (Wait a few seconds for apps to warm up)
Write-Host ""
Write-Host "Waiting for services to initialize..." -ForegroundColor Gray
Start-Sleep -Seconds 5
Write-Host "Opening Frontend at http://localhost:8080..." -ForegroundColor Cyan
Start-Process "http://localhost:8080"
