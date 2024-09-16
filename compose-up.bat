@echo off
   REM Save this as docker-compose-reset.bat

   docker-compose down
   if %ERRORLEVEL% neq 0 goto :error

   docker-compose -f docker-compose.yml -f .\docker-compose.db.yml -f .\docker-compose.app.yml up --build
   if %ERRORLEVEL% neq 0 goto :error

   goto :EOF

   :error
   echo Failed with error #%ERRORLEVEL%.
   exit /b %ERRORLEVEL%