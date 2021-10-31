# Search MSBuild...
# MSBuild is needed for SeeingSharp.Uwp
function GetMSBuildPath()
{
    $allPaths = .\misc\vswhere.exe -nologo -property installationPath -format value | Out-String
	
	$allPathsArray = $allPaths -split "`r`n" | Where { $_ -match '\S' }
	if($allPathsArray.Length -eq 0)
    {
        Write-Error "No Visual Studio installed!"
        exit
    }

    $msbVersion = "15.0"
	$vsPath = ""
	if($allPathsArray -isnot [system.array])
	{
		$vsPath = $allPathsArray
	}
	else
	{
		$vsPath = $allPathsArray[0]		
	}

    $msbPath = Join-Path $vsPath "MSBuild\Current\Bin\amd64\MSBuild.exe"
    return $msbPath
}

# ************************** Main script

# Clear previous published content
Remove-Item -Path ./publish/* -Recurse -Force

# ** Get MSBuild path (for SeeingSharp.Uwp)
$msbuildExe = GetMSBuildPath
Write-Output "MSBuild-Path: $msbuildExe"
Write-Output ""

# Build and pack all projects on the new project format
dotnet pack -c Release -o ./publish ./src/SeeingSharp /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg
dotnet pack -c Release -o ./publish ./src/SeeingSharp.AssimpImporter /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg
dotnet pack -c Release -o ./publish ./src/SeeingSharp.WinForms /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg
dotnet pack -c Release -o ./publish ./src/SeeingSharp.Wpf /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg
dotnet pack -c Release -o ./publish ./src/SeeingSharp.WinUI /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg

# Build and pack SeeingSharp.Uwp by MSBuild (still old project format)
&$msbuildExe src/SeeingSharp.Uwp/SeeingSharp.Uwp.csproj /t:Build /p:Configuration=Release /p:ContinuousIntegrationBuild=true
nuget pack src/SeeingSharp.Uwp/SeeingSharp.Uwp.nuspec -OutputDirectory publish

# Publish sample applications
dotnet publish -c Release -f net5.0-windows -o ./publish/WinFormsSamples ./src/Samples/SeeingSharp.WinFormsSamples
dotnet publish -c Release -f net5.0-windows -o ./publish/WpfSamples ./src/Samples/SeeingSharp.WpfSamples
dotnet publish -c Release -f net5.0-windows -o ./publish/ModelViewer ./src/Tools/SeeingSharp.ModelViewer

# Compress sample applications to have one zip archive for each
$compress = @{
  Path = "./publish/WinFormsSamples", "./publish/WpfSamples"
  CompressionLevel = "Optimal"
  DestinationPath = "./publish/SeeingSharp.Samples.zip"
}
Compress-Archive @compress

$compress = @{
  Path = "./publish/ModelViewer"
  CompressionLevel = "Optimal"
  DestinationPath = "./publish/SeeingSharp.ModelViewer.zip"
}
Compress-Archive @compress