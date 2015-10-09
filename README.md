# scale-dotnet
```
c:\ScaleDotNet\DeployFromHere>cf push scale-dotNet  -s windows2012R2 --no-start
-b https://github.com/cloudfoundry/binary_buildpack -m 512M

c:\ScaleDotNet\DeployFromHere>cf enable-diego scale-dotnet

c:\ScaleDotNet\DeployFromHere>cf start scale-dotnet
```
