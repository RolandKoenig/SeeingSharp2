Remove-Item -Path ./publish/* -Recurse -Force

dotnet pack -c Release -o ./publish ./SeeingSharp
dotnet pack -c Release -o ./publish ./SeeingSharp.WinForms
dotnet pack -c Release -o ./publish ./SeeingSharp.WinFormsCore
dotnet pack -c Release -o ./publish ./SeeingSharp.Wpf
dotnet pack -c Release -o ./publish ./SeeingSharp.WpfCore

dotnet publish -c Release -f netcoreapp3.1 -o ./publish/WinFormsCoreSamples ./Samples/SeeingSharp.WinFormsCoreSamples
dotnet publish -c Release -f netcoreapp3.1 -o ./publish/WpfCoreSamples ./Samples/SeeingSharp.WpfCoreSamples