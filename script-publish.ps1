Remove-Item -Path ./publish/* -Recurse -Force

dotnet pack -c Release -o ./publish ./SeeingSharp
dotnet pack -c Release -o ./publish ./SeeingSharp.AssimpImporter
dotnet pack -c Release -o ./publish ./SeeingSharp.WinForms
dotnet pack -c Release -o ./publish ./SeeingSharp.WinFormsCore
dotnet pack -c Release -o ./publish ./SeeingSharp.Wpf
dotnet pack -c Release -o ./publish ./SeeingSharp.WpfCore

dotnet publish -c Release -f netcoreapp3.1 -o ./publish/WinFormsCoreSamples ./Samples/SeeingSharp.WinFormsCoreSamples
dotnet publish -c Release -f netcoreapp3.1 -o ./publish/WpfCoreSamples ./Samples/SeeingSharp.WpfCoreSamples
dotnet publish -c Release -f netcoreapp3.1 -o ./publish/ModelViewer ./Tools/SeeingSharp.ModelViewer

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