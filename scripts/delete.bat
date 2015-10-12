cf stop scale-dotnet-jkj
cf us service-bindings-dotnet pcf-sql-connection
cf us service-bindings-dotnet pcf-redis
cf d scale-dotnet-jkj -f