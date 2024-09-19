#!/bin/bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "IF DB_ID('ProductsDB') IS NOT NULL SELECT 1 ELSE SELECT 0"