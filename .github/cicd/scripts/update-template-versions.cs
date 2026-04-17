#:sdk Microsoft.NET.Sdk

using System.Diagnostics;

var currentDirPath = Directory.GetCurrentDirectory();

var templateFilePaths = Directory.GetFiles(currentDirPath, "template.json", SearchOption.AllDirectories);

var varInjectionStr = "${TEMPLATE_VERSION}";

// Process each template.json file and replace the variable injection string with the new version string.
foreach (var filePath in templateFilePaths)
{
    var fileData = File.ReadAllText(filePath);

    // If the file contains the variable injection string
    if (fileData.Contains(varInjectionStr))
    {
        fileData = fileData.Replace(varInjectionStr, "v1.2.3");

        File.WriteAllText(filePath, fileData);
    }
}
