@echo off
   REM Save this as docker-compose-reset.bat

   docker-compose down --volumes
   if %ERRORLEVEL% neq 0 goto :error

   docker-compose build --no-cache
   if %ERRORLEVEL% neq 0 goto :error

   docker-compose up
   if %ERRORLEVEL% neq 0 goto :error

   goto :EOF

   :error
   echo Failed with error #%ERRORLEVEL%.
   exit /b %ERRORLEVEL%