$localPkgDir = "S:/LocalNugetSource";
Get-ChildItem -Path $localPkgDir -Filter "KinsonDigital.VelaptorTelemetryTool*" | Remove-Item -Force
dotnet pack "./VelaptorTelemetryTool/VelaptorTelemetryTool.csproj" -c Release -o $localPkgDir;
dotnet tool install -g KinsonDigital.VelaptorTelemetryTool --add-source $localPkgDir;
