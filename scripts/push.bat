cf push scale-dotnet-jkj -s windows2012R2 --no-start -b  https://github.com/cloudfoundry/binary-buildpack.git -m 256M
cf enable-diego scale-dotnet-jkj
cf set-health-check scale-dotnet-jkj none
cf bs service-bindings-dotnet pcf-sql-connection
cf bs service-bindings-dotnet pcf-redis
cf start scale-dotnet-jkj