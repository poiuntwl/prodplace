#!/bin/bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "IF DB_ID('CurrencyRatesDB') IS NOT NULL and DB_ID('HangfireDB') IS NOT NULL SELECT 1 ELSE SELECT 0"