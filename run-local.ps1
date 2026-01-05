<#
.SYNOPSIS
    Launches all Prodplace application services locally.
    Supports Windows Terminal (wt.exe) for a tabbed experience.
    Assumes infrastructure (Databases, RabbitMQ, Redis, Keycloak) is running.
#>

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   Prodplace Local Launcher" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Check/Warn about Infrastructure
Write-Host "NOTE: This script assumes required infrastructure is running." -ForegroundColor Yellow
Write-Host "If you have not started the databases/queues, please run:" -ForegroundColor Gray
Write-Host "   docker-compose --profile db up -d" -ForegroundColor White
Write-Host ""

# Check for Windows Terminal
$UseWT = $false
if (Get-Command "wt.exe" -ErrorAction SilentlyContinue) {
    $UseWT = $true
    Write-Host "Windows Terminal (wt.exe) detected. Launching in tabs." -ForegroundColor Green
} else {
    Write-Host "Windows Terminal not found. Launching in separate windows." -ForegroundColor Yellow
}

$response = Read-Host "Press Enter to continue launch (or Ctrl+C to abort)"

# Store current path safely
$CurrentDir = $PWD.Path

# 2. Define Services

function Start-DotNetService {
    param(
        [string]$ProjectParams,
        [string]$Urls,
        [string]$Title,
        [string]$ExtraEnv = ""
    )
    # Return an object describing the service
    return @{
        Title = $Title
        Color = "#16C60C" # Green
        Cmd   = "$ExtraEnv`$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run $ProjectParams -c Debug --urls '$Urls'"
    }
}

$Services = @()

# ProductsService
$Services += Start-DotNetService -ProjectParams "--project ProductsService" -Urls "https://localhost:44301" -Title "ProductsService"
$Services += Start-DotNetService -ProjectParams "--project PriceService" -Urls "https://localhost:44302" -Title "PriceService"
$Services += Start-DotNetService -ProjectParams "--project CurrencyRatesService" -Urls "https://localhost:44303" -Title "CurrencyRatesService"
$Services += Start-DotNetService -ProjectParams "--project IdentityService" -Urls "https://localhost:44304" -Title "IdentityService"
$Services += Start-DotNetService -ProjectParams "--project OrderService" -Urls "https://localhost:44305" -Title "OrderService"
$Services += Start-DotNetService -ProjectParams "--project CustomerService" -Urls "https://localhost:44306" -Title "CustomerService"
$Services += Start-DotNetService -ProjectParams "--project Prodplace.Admin" -Urls "https://localhost:44307" -Title "AdminService" -ExtraEnv "`$env:Services__CurrencyRatesService='https://localhost:44303'; "
$Services += Start-DotNetService -ProjectParams "--project ProxyService" -Urls "http://localhost:44300" -Title "ProxyService"
# Override Proxy Color
$Services[-1].Color = "#FFA500" 

# Frontend
if (Test-Path "fe") {
    $Services += @{
        Title = "Frontend"
        Color = "#3b82f6"
        Cmd   = "cd fe; bun install; bun run serve"
    }
}

# Backoffice
if (Test-Path "backoffice-web") {
    $Services += @{
        Title = "Backoffice"
        Color = "#8b5cf6"
        Cmd   = "cd backoffice-web; bun install; bun run dev --port 12346"
    }
}

# 3. Execution Logic

if ($UseWT) {
    # Build the wt command arguments string
    # Syntax: wt -w 0 new-tab ... ; new-tab ...
    $WtArgs = @("-w", "0")
    
    foreach ($svc in $Services) {
        # Construct arguments for this tab
        # We use -d "$CurrentDir" to ensure we are in the repo root
        # We use powershell -NoExit -Command "..." to keep window open
        
        # Escape double quotes in the command string for passing to WT
        $SafeCmd = $svc.Cmd.Replace('"', '""') # Corrected escaping for double quotes within a double-quoted string
        
        # Add separator if not first (actually wt arguments are space separated, but commands separated by ';')
        # However, passing as an array to Start-Process, we rely on WT parsing.
        # The syntax `wt ; new-tab` allows chaining.
        if ($WtArgs.Count -gt 2) { 
            # If we already have commands (more than just -w 0), add the separator
            $WtArgs += ";" 
        }

        $WtArgs += "new-tab"
        $WtArgs += "--title", $svc.Title
        $WtArgs += "--tabColor", $svc.Color
        $WtArgs += "-d", "$CurrentDir"
        $WtArgs += "powershell"
        $WtArgs += "-NoExit"
        $WtArgs += "-Command"
        $WtArgs += "$SafeCmd"
    }

    Write-Host "Launching Windows Terminal..." -ForegroundColor Cyan
    # Debug: Uncomment to see the raw arguments
    # Write-Host ($WtArgs -join " ") -ForegroundColor DarkGray
    
    Start-Process "wt.exe" -ArgumentList $WtArgs
}
else {
    # Fallback: Separate Windows
    foreach ($svc in $Services) {
        Write-Host "Starting $($svc.Title)..." -ForegroundColor Green
        Start-Process -FilePath "powershell" -ArgumentList "-NoExit", "-Command", $svc.Cmd -WorkingDirectory $CurrentDir -WindowStyle Normal
    }
}

Write-Host "All services launched." -ForegroundColor Cyan

# 4. Open Browser
Write-Host ""
Write-Host "Waiting for services to initialize..." -ForegroundColor Gray
Start-Sleep -Seconds 5
Write-Host "Opening Frontend at http://localhost:8080..." -ForegroundColor Cyan
Start-Process "http://localhost:8080"
Write-Host "Opening Backoffice at http://localhost:12346..." -ForegroundColor Cyan
Start-Process "http://localhost:12346"