sleep 10s

/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Password123!! -d master -i create-db.sql