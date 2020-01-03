Remove-Item -path ./publish/*

dotnet pack -c Release -o ./publish ./SeeingSharp
dotnet pack -c Release -o ./publish ./SeeingSharp.WinForms
dotnet pack -c Release -o ./publish ./SeeingSharp.WinFormsCore
dotnet pack -c Release -o ./publish ./SeeingSharp.Wpf
dotnet pack -c Release -o ./publish ./SeeingSharp.WpfCore

dotnet publish -c Release -f net471 -r win7-x86 -o ./publish/WinFormsSamples ./Samples/SeeingSharp.WinFormsSamples
dotnet publish -c Release -f netcoreapp3.1 -r Portable -o ./publish/WinFormsCoreSamples ./Samples/SeeingSharp.WinFormsCoreSamples
dotnet publish -c Release -f net471 -r win7-x86 -o ./publish/WpfSamples ./Samples/SeeingSharp.WpfSamples
dotnet publish -c Release -f netcoreapp3.1 -r Portable -o ./publish/WpfCoreSamples ./Samples/SeeingSharp.WpfCoreSamples