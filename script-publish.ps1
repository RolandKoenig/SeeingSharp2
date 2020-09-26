Remove-Item -Path ./publish/* -Recurse -Force

# Build and pack all projects on the new project format
dotnet pack -c Release -o ./publish ./SeeingSharp /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg
dotnet pack -c Release -o ./publish ./SeeingSharp.AssimpImporter /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg
dotnet pack -c Release -o ./publish ./SeeingSharp.WinForms /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg
dotnet pack -c Release -o ./publish ./SeeingSharp.Wpf /p:ContinuousIntegrationBuild=true /p:IncludeSymbols=true /p:EmbedUntrackedSources=true -p:SymbolPackageFormat=snupkg

# Pack UWP library (still old project format)
# Be carefull: This command does not build. You have to execute release build manually in VisualStudio
nuget pack SeeingSharp.Uwp/SeeingSharp.Uwp.nuspec -OutputDirectory publish

# Publish sample applications
dotnet publish -c Release -f netcoreapp3.1 -o ./publish/WinFormsCoreSamples ./Samples/SeeingSharp.WinFormsCoreSamples
dotnet publish -c Release -f netcoreapp3.1 -o ./publish/WpfCoreSamples ./Samples/SeeingSharp.WpfCoreSamples
dotnet publish -c Release -f netcoreapp3.1 -o ./publish/ModelViewer ./Tools/SeeingSharp.ModelViewer

# Compress sample applications to have one zip archive for each
$compress = @{
  Path = "./publish/WinFormsCoreSamples", "./publish/WpfCoreSamples"
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