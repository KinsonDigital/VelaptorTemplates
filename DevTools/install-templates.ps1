dotnet new uninstall .\VelaptorTemplates
dotnet new install .\VelaptorTemplates

# Delete the test project
$templateTestOutputDir = ".\template-test-output";
Get-ChildItem -Path $templateTestOutputDir -Recurse | Remove-Item -Recurse -Force

dotnet new VelaptorRenderTexture --name "TemplateTestProject" --output $templateTestOutputDir;
