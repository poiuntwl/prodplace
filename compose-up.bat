@echo off
REM ProdPlace Docker Compose Launcher
REM =================================
REM Usage:
REM   compose-up.bat           - Start infrastructure only (keycloak, rabbitmq)
REM   compose-up.bat db        - Start infrastructure + databases
REM   compose-up.bat app       - Start infrastructure + app services (databases must be healthy)
REM   compose-up.bat full      - Start everything
REM   compose-up.bat down      - Stop and remove all containers

setlocal

if "%1"=="" (
    echo Starting infrastructure only...
    docker-compose up --build
    goto :EOF
)

if "%1"=="down" (
    echo Stopping all containers...
    docker-compose --profile full down
    goto :EOF
)

if "%1"=="clean" (
    echo Stopping and removing volumes...
    docker-compose --profile full down --volumes
    goto :EOF
)

echo Starting with profile: %1
docker-compose --profile %1 up --build

if %ERRORLEVEL% neq 0 (
    echo Failed with error #%ERRORLEVEL%.
    exit /b %ERRORLEVEL%
)